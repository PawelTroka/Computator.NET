using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.SettingsTypes;
using NUnit.Framework;
using Menu = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteMenu;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class AutocompletePopupLifetimeTests
    {
        [Test]
        public void ClosingAutocompleteCancelsAnAlreadyScheduledDisplay()
        {
            using var fixture = new PopupFixture();
            fixture.Input.RaiseKeyDown();
            NUnit.Framework.Assert.That(fixture.PendingDisplay.Enabled, Is.True);
            fixture.Menu.Close();
            Pump(180);
            NUnit.Framework.Assert.That(fixture.Menu.Visible, Is.False);
            NUnit.Framework.Assert.That(fixture.Help.Visible, Is.False);
            NUnit.Framework.Assert.That(fixture.PendingDisplay.Enabled, Is.False);
        }

        [Test]
        public void MovingFocusAfterTypingCannotReopenAutocompleteOverAnotherControl()
        {
            using var fixture = new PopupFixture();
            fixture.Input.RaiseKeyDown();
            fixture.Other.Focus();
            Pump(180);
            NUnit.Framework.Assert.That(fixture.Menu.Visible, Is.False);
            NUnit.Framework.Assert.That(fixture.Help.Visible, Is.False);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AnUnavailableEditorCannotOpenTheMenuOrItsHtmlHelp(bool disabled)
        {
            using var fixture = new PopupFixture();
            if (disabled) fixture.Input.Enabled = false;
            else fixture.Input.Visible = false;
            fixture.Menu.Show(fixture.Input, false);
            Pump(80);
            NUnit.Framework.Assert.That(fixture.Menu.Visible, Is.False);
            NUnit.Framework.Assert.That(fixture.Help.Visible, Is.False);
        }

        [Test]
        public void AModalDialogSuppressesAutocompleteAndCancelRestoresNormalUse()
        {
            using var fixture = new PopupFixture();
            using var dialog = new Form { Size = new Size(220, 100), ShowInTaskbar = false };
            using var pulse = new System.Windows.Forms.Timer { Interval = 100 };
            var attempts = 0;
            var openedWhileModal = 0;
            fixture.Menu.Opening += (_, _) => { if (dialog.Visible) openedWhileModal++; };
            pulse.Tick += (_, _) =>
            {
                if (++attempts == 1) fixture.Menu.Show(fixture.Input, false);
                else dialog.Close();
            };
            dialog.Shown += (_, _) => pulse.Start();
            dialog.ShowDialog(fixture.Window);
            pulse.Stop();
            NUnit.Framework.Assert.That(attempts, Is.GreaterThanOrEqualTo(2));
            NUnit.Framework.Assert.That(openedWhileModal, Is.Zero);
            NUnit.Framework.Assert.That(fixture.Menu.Visible, Is.False);
            NUnit.Framework.Assert.That(fixture.Help.Visible, Is.False);

            fixture.Input.Focus();
            fixture.Menu.Show(fixture.Input, false);
            NUnit.Framework.Assert.That(fixture.Menu.Visible, Is.True,
                "Cancelling a modal interaction must not disable subsequent editor completion.");
        }

        [Test]
        public void ACancelledOwnerCloseStillCancelsAnEarlyBoundEditorsPendingDisplay()
        {
            using var fixture = new PopupFixture(bindBeforeParenting: true);
            fixture.Window.FormClosing += (_, args) => args.Cancel = true;
            fixture.Input.RaiseKeyDown();
            fixture.Window.Close();
            Pump(180);
            NUnit.Framework.Assert.That(fixture.Window.Visible, Is.True);
            NUnit.Framework.Assert.That(fixture.Menu.Visible, Is.False);
            NUnit.Framework.Assert.That(fixture.Help.Visible, Is.False);
            NUnit.Framework.Assert.That(fixture.PendingDisplay.Enabled, Is.False);
        }

        private static void Pump(int milliseconds)
        {
            var elapsed = Stopwatch.StartNew();
            while (elapsed.ElapsedMilliseconds < milliseconds)
            {
                Application.DoEvents();
                Thread.Sleep(5);
            }
        }

        private sealed class InputBox : TextBox
        {
            // Exercise the component event path without sending OS keyboard input.
            internal void RaiseKeyDown() => OnKeyDown(new KeyEventArgs(Keys.I));
        }

        private sealed class PopupFixture : IDisposable
        {
            private readonly TooltipType _originalTooltipType = Settings.Default.TooltipType;
            internal Form Window { get; } = new Form { Size = new Size(500, 220), ShowInTaskbar = false };
            internal InputBox Input { get; } = new InputBox { Location = new Point(10, 10), Width = 200 };
            internal Button Other { get; } = new Button { Location = new Point(10, 80), Text = "Other control" };
            internal Menu Menu { get; } = new Menu(new SharedViewState()) { AppearInterval = 40 };
            internal System.Windows.Forms.Timer PendingDisplay => Field<System.Windows.Forms.Timer>(Menu, "_timer");
            internal ToolStripDropDown Help => Field<ToolStripDropDown>(Menu.Host.ListView, "toolTip");

            internal PopupFixture(bool bindBeforeParenting = false)
            {
                Settings.Default.TooltipType = TooltipType.Default;
                Menu.SetAutocompleteItems(new[] { Item("sin(x)"), Item("sinh(x)") });
                if (bindBeforeParenting) Menu.SetAutocompleteMenu(Input, Menu);
                Window.Controls.AddRange(new Control[] { Input, Other });
                if (!bindBeforeParenting) Menu.SetAutocompleteMenu(Input, Menu);
                Window.Show();
                Input.Focus();
                Input.Text = "si";
                Input.SelectionStart = Input.TextLength;
                Pump(40);
            }

            public void Dispose()
            {
                Menu.Close();
                Menu.SetAutocompleteMenu(Input, null);
                Menu.Dispose();
                Window.Dispose();
                Settings.Default.TooltipType = _originalTooltipType;
            }

            private static AutocompleteItem Item(string signature) => new AutocompleteItem(signature)
            {
                Details = new FunctionInfo
                {
                    Signature = signature, Title = "Function help", Description = "<p>Owned test description.</p>"
                }
            };

            private static T Field<T>(object instance, string name) =>
                (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
        }
    }
}
