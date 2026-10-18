using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Abstract;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Menu.Commands;
using Computator.NET.Core.Menu.Commands.TopMenuCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Events;
using Computator.NET.Desktop.Views;
using Computator.NET.Localization.Menu;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class FunctionMenuTests
    {
        [Test]
        public void MenuStripPreservesSubjectsAndClickableEntriesWithoutRepeatedTypeMenus()
        {
            var items = new[]
            {
                Item("sin(x)", "ElementaryFunctions", "Trigonometric functions"),
                Item("gamma(x)", "SpecialFunctions", "SpecialFunctions"),
                Item("BesselJ0(x)", "SpecialFunctions", "Bessel functions"),
                Item("PI", "MathematicalConstants", "Mathematical constants"),
                Item("ElementaryCharge", "PhysicalConstants", "Electromagnetism")
            };
            var provider = new Mock<IAutocompleteProvider>();
            provider.SetupGet(value => value.ExpressionAutocompleteItems).Returns(items);
            var expression = new Mock<ITextProvider>();
            expression.SetupProperty(value => value.Text, string.Empty);
            var scripting = new Mock<IScriptProvider>();
            var customFunctions = new Mock<IScriptProvider>();
            var state = new Mock<ISharedViewState>();
            state.SetupGet(value => value.CurrentView).Returns(ViewName.Calculations);
            var mouse = new Mock<IClickedMouseButtonsProvider>();
            mouse.SetupGet(value => value.ClickedMouseButtons).Returns(MouseButtons.Left);
            var details = new Mock<IShowFunctionDetails>();
            var functions = new FunctionsCommand(expression.Object, scripting.Object, customFunctions.Object,
                state.Object, provider.Object, mouse.Object, details.Object);
            var constants = new ConstantsCommand(expression.Object, scripting.Object, customFunctions.Object,
                state.Object, provider.Object, mouse.Object, details.Object);

            using var window = new Form { Width = 640, Height = 480, ShowInTaskbar = false };
            using var view = new MenuStripView(null, null, functions, constants, null, null, null, null)
            {
                Dock = DockStyle.Top
            };
            window.Controls.Add(view);
            window.Show();
            NUnit.Framework.Assert.That(view.Handle, Is.Not.EqualTo(IntPtr.Zero));

            var strip = view.Controls.OfType<MenuStrip>().Single();
            var functionsMenu = Find(strip.Items, functions.Text);
            var special = Find(functionsMenu.DropDownItems, MenuStrings.specialFunctionsToolStripMenuItem_Text);
            NUnit.Framework.Assert.That(special.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text),
                Is.EqualTo(new[] { "gamma(x)", "Bessel functions" }));
            var gamma = Find(special.DropDownItems, "gamma(x)");
            var bessel = Find(special.DropDownItems, "Bessel functions");
            var besselItem = Find(bessel.DropDownItems, "BesselJ0(x)");
            NUnit.Framework.Assert.That(gamma.DropDownItems.Count, Is.Zero);
            NUnit.Framework.Assert.That(besselItem.DropDownItems.Count, Is.Zero);

            var constantsMenu = Find(strip.Items, constants.Text);
            var mathematical = Find(constantsMenu.DropDownItems, MenuStrings.mathematicalConstantsToolStripMenuItem_Text);
            NUnit.Framework.Assert.That(mathematical.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text),
                Is.EqualTo(new[] { "PI" }));
            var pi = Find(mathematical.DropDownItems, "PI");
            NUnit.Framework.Assert.That(pi.DropDownItems.Count, Is.Zero);
            var physical = Find(constantsMenu.DropDownItems, MenuStrings.physicalConstantsToolStripMenuItem_Text);
            var electromagnetism = Find(physical.DropDownItems, "Electromagnetism");
            NUnit.Framework.Assert.That(Find(electromagnetism.DropDownItems, "ElementaryCharge").DropDownItems.Count, Is.Zero);
            NUnit.Framework.Assert.That(Leaves(strip.Items).Select(item => item.Text),
                Is.EquivalentTo(items.Select(item => item.Text)));

            functionsMenu.ShowDropDown();
            special.ShowDropDown();
            gamma.PerformClick();
            NUnit.Framework.Assert.That(expression.Object.Text, Is.EqualTo("gamma(x)"));

            expression.Object.Text = string.Empty;
            functionsMenu.ShowDropDown();
            special.ShowDropDown();
            bessel.ShowDropDown();
            besselItem.PerformClick();
            NUnit.Framework.Assert.That(expression.Object.Text, Is.EqualTo("BesselJ0(x)"));

            expression.Object.Text = string.Empty;
            constantsMenu.ShowDropDown();
            mathematical.ShowDropDown();
            pi.PerformClick();
            NUnit.Framework.Assert.That(expression.Object.Text, Is.EqualTo("PI"));
            details.Verify(value => value.Show(It.IsAny<FunctionInfo>()), Times.Never);
        }

        private static ToolStripMenuItem Find(ToolStripItemCollection items, string text)
        {
            return items.OfType<ToolStripMenuItem>().Single(item => item.Text == text);
        }

        private static IEnumerable<ToolStripMenuItem> Leaves(ToolStripItemCollection items)
        {
            foreach (var item in items.OfType<ToolStripMenuItem>())
            {
                if (item.DropDownItems.Count == 0)
                    yield return item;
                else
                    foreach (var leaf in Leaves(item.DropDownItems))
                        yield return leaf;
            }
        }

        private static AutocompleteItem Item(string signature, string type, string category)
        {
            return new AutocompleteItem(signature)
            {
                Details = new FunctionInfo
                {
                    Signature = signature,
                    Type = type,
                    Category = category,
                    Title = signature
                }
            };
        }
    }
}
