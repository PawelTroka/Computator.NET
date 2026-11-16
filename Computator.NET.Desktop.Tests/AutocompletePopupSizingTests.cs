using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Model;
using NUnit.Framework;
using CompletionMenu = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteMenu;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class AutocompletePopupSizingTests
    {
        [Test]
        public void PopupGrowsForLongSignaturesAndShrinksForShortOnes()
        {
            using var window = new Form { Size = new Size(750, 320), ShowInTaskbar = false };
            var editor = new TextBox { Location = new Point(12, 12), Width = 300 };
            using var menu = new CompletionMenu(new SharedViewState());
            window.Controls.Add(editor);
            menu.SetAutocompleteItems(new[]
            {
                Item("sin(x)"),
                Item("sinh(x)"),
                Item("ultralongparameterizedscientificfunction(alpha,beta,gamma,delta)"),
                Item("ultralongparameterizedscientificfunction2(alpha,beta,gamma,delta)")
            });

            try
            {
                window.Show();
                editor.Focus();

                ShowMatches("si");
                Assert.That(menu.Visible, Is.True);
                Assert.That(menu.VisibleItems.Count, Is.EqualTo(2));
                var narrowWidth = menu.Host.Width;

                ShowMatches("ul");
                Assert.That(menu.VisibleItems.Count, Is.EqualTo(2));
                var wideWidth = menu.Host.Width;

                Assert.That(wideWidth, Is.GreaterThan(narrowWidth + 50),
                    "Long signatures should be readable without a fixed-width completion window.");
                Assert.That(wideWidth, Is.LessThanOrEqualTo(Screen.FromControl(editor).WorkingArea.Width),
                    "Completion must never grow wider than the usable display.");

                ShowMatches("si");
                Assert.That(menu.Host.Width, Is.EqualTo(narrowWidth),
                    "Filtering back to short signatures should reclaim unused screen space.");
            }
            finally
            {
                menu.Close();
                menu.SetAutocompleteMenu(editor, null);
                window.Close();
            }

            void ShowMatches(string prefix)
            {
                editor.Text = prefix;
                editor.SelectionStart = editor.TextLength;
                menu.Show(editor, false);
            }
        }

        private static AutocompleteItem Item(string text) => new AutocompleteItem(text)
        {
            Details = new FunctionInfo()
        };
    }
}
