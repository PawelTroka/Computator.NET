using System;
using System.Collections.Generic;
using Computator.NET.Core.Menu.Commands.TopMenuCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Events;
using NUnit.Framework;
using NUnitAssert = NUnit.Framework.Assert;

namespace Computator.NET.Core.Tests.MenuTests
{
    [TestFixture]
    public class RunCommandTests
    {
        [TestCase(ViewName.CustomFunctions)]
        [TestCase(ViewName.SymbolicCalculations)]
        public void ViewWithoutAnActionDoesNotRunAnotherViewsAction(ViewName view)
        {
            var calls = 0;
            var state = new SharedViewState { CurrentView = view };
            state.DefaultActions[ViewName.Calculations] = (_, __) => calls++;
            state.DefaultActions[ViewName.Scripting] = (_, __) => calls++;
            var command = new RunCommand(state);

            NUnitAssert.DoesNotThrow(command.Execute);

            NUnitAssert.That(calls, Is.Zero);
        }

        [TestCase(ViewName.Calculations)]
        [TestCase(ViewName.Charting)]
        [TestCase(ViewName.NumericalCalculations)]
        [TestCase(ViewName.Scripting)]
        public void RunnableViewInvokesItsActionExactlyOnce(ViewName view)
        {
            var calls = 0;
            object sender = null;
            EventArgs arguments = null;
            var state = new SharedViewState { CurrentView = view };
            state.DefaultActions[view] = (value, args) =>
            {
                calls++;
                sender = value;
                arguments = args;
            };
            var command = new RunCommand(state);

            command.Execute();

            NUnitAssert.That(calls, Is.EqualTo(1));
            NUnitAssert.That(sender, Is.SameAs(command));
            NUnitAssert.That(arguments, Is.Not.Null);
        }

        [Test]
        public void ExistingCommandFollowsViewChangesThroughNonRunnableTabs()
        {
            var calls = new List<ViewName>();
            var state = new SharedViewState();
            state.DefaultActions[ViewName.Calculations] = (_, __) => calls.Add(ViewName.Calculations);
            state.DefaultActions[ViewName.Scripting] = (_, __) => calls.Add(ViewName.Scripting);
            var command = new RunCommand(state);

            foreach (var view in new[]
            {
                ViewName.Calculations, ViewName.CustomFunctions, ViewName.Scripting,
                ViewName.SymbolicCalculations, ViewName.Calculations
            })
            {
                state.CurrentView = view;
                command.Execute();
            }

            NUnitAssert.That(calls, Is.EqualTo(new[]
            {
                ViewName.Calculations, ViewName.Scripting, ViewName.Calculations
            }));
        }

        [Test]
        public void RemovingAnActionDoesNotLeaveAStaleRunnableCommand()
        {
            var calls = 0;
            var state = new SharedViewState { CurrentView = ViewName.Scripting };
            state.DefaultActions[ViewName.Scripting] = (_, __) => calls++;
            var command = new RunCommand(state);
            command.Execute();
            state.DefaultActions.Remove(ViewName.Scripting);

            NUnitAssert.DoesNotThrow(command.Execute);

            NUnitAssert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void ExceptionsFromRegisteredActionsAreStillReported()
        {
            var expected = new InvalidOperationException("Calculation failed.");
            var state = new SharedViewState { CurrentView = ViewName.Calculations };
            state.DefaultActions[ViewName.Calculations] = (_, __) => throw expected;
            var command = new RunCommand(state);

            var actual = NUnitAssert.Throws<InvalidOperationException>(command.Execute);

            NUnitAssert.That(actual, Is.SameAs(expected));
        }
    }
}
