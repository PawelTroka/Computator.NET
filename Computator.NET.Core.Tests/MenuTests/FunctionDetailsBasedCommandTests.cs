using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Computator.NET.Core.Abstract;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Menu.Commands;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Menu.Commands.TopMenuCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Events;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.MenuTests
{
    [TestFixture]
    public class FunctionDetailsBasedCommandTests
    {
        private Mock<ITextProvider> _expression;
        private Mock<IScriptProvider> _scripting;
        private Mock<IScriptProvider> _customFunctions;
        private Mock<ISharedViewState> _state;
        private Mock<IClickedMouseButtonsProvider> _mouse;
        private Mock<IShowFunctionDetails> _details;

        [SetUp]
        public void SetUp()
        {
            _expression = new Mock<ITextProvider>();
            _expression.SetupProperty(provider => provider.Text, string.Empty);
            _scripting = new Mock<IScriptProvider>();
            _scripting.SetupProperty(provider => provider.Text, string.Empty);
            _customFunctions = new Mock<IScriptProvider>();
            _customFunctions.SetupProperty(provider => provider.Text, string.Empty);
            _state = new Mock<ISharedViewState>();
            _state.SetupProperty(state => state.CurrentView, ViewName.Calculations);
            _mouse = new Mock<IClickedMouseButtonsProvider>();
            _mouse.SetupGet(mouse => mouse.ClickedMouseButtons).Returns(MouseButtons.Left);
            _details = new Mock<IShowFunctionDetails>();
        }

        [TestCase("SpecialFunctions")]
        [TestCase("Special functions")]
        [TestCase(" SPECIAL\tFUNCTIONS ")]
        [TestCase("Funkcje specjalne")]
        [TestCase("Funkcje &specjalne")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t ")]
        public void RepeatedOrMissingCategoryCreatesADirectItemWithoutChangingSharedDetails(string category)
        {
            var item = Item("gamma(x)", "SpecialFunctions", category);
            var originalDetails = item.Details;
            var originalFields = DetailFields(originalDetails);

            var menu = CreateMenu(new[] { item }, "SpecialFunctions", "Funkcje &specjalne");
            var parent = menu.ChildrenCommands.Single();

            NUnit.Framework.Assert.That(parent.Text, Is.EqualTo("Funkcje &specjalne"));
            NUnit.Framework.Assert.That(parent.ChildrenCommands.Select(command => command.Text),
                Is.EqualTo(new[] { "gamma(x)" }));
            NUnit.Framework.Assert.That(parent.ChildrenCommands.Single(), Is.Not.InstanceOf<DummyCommand>());
            NUnit.Framework.Assert.That(item.Details, Is.SameAs(originalDetails));
            NUnit.Framework.Assert.That(DetailFields(item.Details), Is.EqualTo(originalFields));
        }

        [TestCase("MathematicalConstants", "Mathematical constants", "MathematicalConstants", "PI")]
        [TestCase("MathematicalConstants", "Mathematical constants", "Mathematical constants", "PI")]
        [TestCase("PhysicalConstants", "Physical constants", "Physical constants", "ElementaryCharge")]
        public void ConstantsDoNotRepeatTheirTypeAsASubmenu(string type, string caption, string category, string signature)
        {
            var menu = CreateMenu(new[] { Item(signature, type, category) }, type, caption);
            var child = menu.ChildrenCommands.Single().ChildrenCommands.Single();

            NUnit.Framework.Assert.That(child.Text, Is.EqualTo(signature));
            NUnit.Framework.Assert.That(child, Is.Not.InstanceOf<DummyCommand>());
        }

        [Test]
        public void MixedFallbackAndSubjectCategoriesPreserveGroupAndItemOrder()
        {
            var items = new[]
            {
                Item("gamma(x)", "SpecialFunctions", "SpecialFunctions"),
                Item("Ackley(x,y)", "SpecialFunctions", "Optimization benchmarks"),
                Item("BesselJ0(x)", "SpecialFunctions", "Bessel functions"),
                Item("zeta(x)", "SpecialFunctions", "SpecialFunctions"),
                Item("BesselJ1(x)", "SpecialFunctions", "Bessel functions"),
                Item("Rastrigin(x,y)", "SpecialFunctions", "Optimization benchmarks"),
                Item("PI", "MathematicalConstants", "Mathematical constants"),
                new AutocompleteItem("no reference") { Details = null },
                new AutocompleteItem("empty reference")
            };

            var menu = CreateMenu(items);
            var children = menu.ChildrenCommands.Single().ChildrenCommands.ToArray();

            NUnit.Framework.Assert.That(children.Select(command => command.Text),
                Is.EqualTo(new[] { "gamma(x)", "Optimization benchmarks", "Bessel functions", "zeta(x)" }));
            NUnit.Framework.Assert.That(children[1].ChildrenCommands.Select(command => command.Text),
                Is.EqualTo(new[] { "Ackley(x,y)", "Rastrigin(x,y)" }));
            NUnit.Framework.Assert.That(children[2].ChildrenCommands.Select(command => command.Text),
                Is.EqualTo(new[] { "BesselJ0(x)", "BesselJ1(x)" }));
            NUnit.Framework.Assert.That(Leaves(menu).Select(command => command.Text),
                Is.EquivalentTo(items.Take(6).Select(item => item.Text)));
        }

        [Test]
        public void ASingleMeaningfulSubjectCategoryIsRetained()
        {
            var menu = CreateMenu(new[]
            {
                Item("gamma(x)", "SpecialFunctions", "Gamma and related functions"),
                Item("logGamma(x)", "SpecialFunctions", "Gamma and related functions")
            });
            var category = menu.ChildrenCommands.Single().ChildrenCommands.Single();

            NUnit.Framework.Assert.That(category, Is.InstanceOf<DummyCommand>());
            NUnit.Framework.Assert.That(category.Text, Is.EqualTo("Gamma and related functions"));
            NUnit.Framework.Assert.That(category.ChildrenCommands.Select(command => command.Text),
                Is.EqualTo(new[] { "gamma(x)", "logGamma(x)" }));
        }

        [TestCase(ViewName.Calculations, "SpecialFunctions")]
        [TestCase(ViewName.Scripting, "SpecialFunctions")]
        [TestCase(ViewName.CustomFunctions, "SpecialFunctions")]
        [TestCase(ViewName.Calculations, "Gamma and related functions")]
        [TestCase(ViewName.Scripting, "Gamma and related functions")]
        [TestCase(ViewName.CustomFunctions, "Gamma and related functions")]
        public void DirectAndGroupedEntriesInsertIntoTheActiveEditor(ViewName view, string category)
        {
            _state.Object.CurrentView = view;
            var menu = CreateMenu(new[] { Item("gamma(x)", "SpecialFunctions", category) });

            Leaves(menu).Single().Execute();

            NUnit.Framework.Assert.That(_expression.Object.Text,
                Is.EqualTo(view == ViewName.Calculations ? "gamma(x)" : string.Empty));
            NUnit.Framework.Assert.That(_scripting.Object.Text,
                Is.EqualTo(view == ViewName.Scripting ? "gamma(x)" : string.Empty));
            NUnit.Framework.Assert.That(_customFunctions.Object.Text,
                Is.EqualTo(view == ViewName.CustomFunctions ? "gamma(x)" : string.Empty));
            _details.Verify(details => details.Show(It.IsAny<FunctionInfo>()), Times.Never);
        }

        [TestCase("SpecialFunctions")]
        [TestCase("Gamma and related functions")]
        public void RightClickKeepsTheOriginalReferenceWithoutInsertingText(string category)
        {
            _mouse.SetupGet(mouse => mouse.ClickedMouseButtons).Returns(MouseButtons.Right);
            _expression.Object.Text = "existing expression";
            _scripting.Object.Text = "existing script";
            _customFunctions.Object.Text = "existing functions";
            var item = Item("gamma(x)", "SpecialFunctions", category);
            var menu = CreateMenu(new[] { item });

            Leaves(menu).Single().Execute();

            _details.Verify(details => details.Show(item.Details), Times.Once);
            NUnit.Framework.Assert.That(_expression.Object.Text, Is.EqualTo("existing expression"));
            NUnit.Framework.Assert.That(_scripting.Object.Text, Is.EqualTo("existing script"));
            NUnit.Framework.Assert.That(_customFunctions.Object.Text, Is.EqualTo("existing functions"));
        }

        [Test]
        public void ActualFunctionsAndConstantsMenusPreserveEveryCatalogEntryIncludingOverloads()
        {
            var provider = new AutocompleteProvider(new AutocompleteReflectionSource(), new FunctionsDetailsFileSource());
            var items = provider.ExpressionAutocompleteItems.ToArray();
            var categoriesBefore = items.Select(item => item.Details.Category).ToArray();
            var menus = new IToolbarCommand[]
            {
                new FunctionsCommand(_expression.Object, _scripting.Object, _customFunctions.Object,
                    _state.Object, provider, _mouse.Object, _details.Object),
                new ConstantsCommand(_expression.Object, _scripting.Object, _customFunctions.Object,
                    _state.Object, provider, _mouse.Object, _details.Object)
            };
            var types = new[] { "ElementaryFunctions", "SpecialFunctions", "MathematicalConstants", "PhysicalConstants" };
            var expected = items.Where(item => !item.Details.IsNullOrEmpty() && types.Contains(item.Details.Type))
                .Select(item => item.Details.Signature).ToArray();

            NUnit.Framework.Assert.That(expected, Is.Not.Empty);
            // Compare a multiset: different runtime overloads may share a lookup signature.
            NUnit.Framework.Assert.That(menus.SelectMany(Leaves).Select(command => command.Text), Is.EquivalentTo(expected));
            NUnit.Framework.Assert.That(items.Select(item => item.Details.Category), Is.EqualTo(categoriesBefore));
        }

        private FunctionDetailsBasedCommand CreateMenu(IEnumerable<AutocompleteItem> items,
            string type = "SpecialFunctions", string caption = "Special functions")
        {
            var provider = new Mock<IAutocompleteProvider>();
            provider.SetupGet(value => value.ExpressionAutocompleteItems).Returns(items);
            return new FunctionDetailsBasedCommand("Reference", new Dictionary<string, string> { { type, caption } },
                _expression.Object, _scripting.Object, _customFunctions.Object, _state.Object,
                provider.Object, _mouse.Object, _details.Object);
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
                    Title = "Reference: " + signature,
                    Description = "Explanation of " + signature,
                    Url = "https://example.test/reference"
                }
            };
        }

        private static string[] DetailFields(FunctionInfo details)
        {
            return new[] { details.Category, details.Signature, details.Type, details.Title, details.Description, details.Url };
        }

        private static IEnumerable<IToolbarCommand> Leaves(IToolbarCommand command)
        {
            if (!(command is DummyCommand))
            {
                yield return command;
                yield break;
            }

            foreach (var child in command.ChildrenCommands ?? Enumerable.Empty<IToolbarCommand>())
                foreach (var leaf in Leaves(child))
                    yield return leaf;
        }
    }
}
