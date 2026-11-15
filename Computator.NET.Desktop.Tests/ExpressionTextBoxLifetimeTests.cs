using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Controls;
using NUnit.Framework;
using Menu = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteMenu;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class ExpressionTextBoxLifetimeTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void DisposingAnExpressionEditorReleasesItsCompletionBinding(bool createHandle)
        {
            using var fixture = new ExpressionFixture();
            if (createHandle) fixture.Show();
            NUnit.Framework.Assert.That(Bindings("AutocompleteMenuByControls").Contains(fixture.Input), Is.True);
            NUnit.Framework.Assert.That(Bindings("WrapperByControls").Contains(fixture.Input), Is.True);

            fixture.Input.Dispose();
            fixture.Input.Dispose();

            NUnit.Framework.Assert.Multiple(() =>
            {
                NUnit.Framework.Assert.That(fixture.MenuDisposals, Is.EqualTo(1));
                NUnit.Framework.Assert.That(fixture.Menu.Host.IsDisposed, Is.True);
                NUnit.Framework.Assert.That(Bindings("AutocompleteMenuByControls").Contains(fixture.Input), Is.False);
                NUnit.Framework.Assert.That(Bindings("WrapperByControls").Contains(fixture.Input), Is.False);
                NUnit.Framework.Assert.That(fixture.State.SubscriberCount, Is.Zero);
            });
        }

        [Test]
        public void DisposingTheOwnerCancelsCompletionBeforeItsTimerCanOpenAPopup()
        {
            using var fixture = new ExpressionFixture();
            fixture.Show();
            fixture.Menu.AppearInterval = 40;
            var openings = 0;
            fixture.Menu.Opening += (_, _) => openings++;
            fixture.Input.RaiseKeyDown();
            NUnit.Framework.Assert.That(fixture.PendingDisplay.Enabled, Is.True);

            fixture.Window.Dispose();
            Pump(180);

            NUnit.Framework.Assert.Multiple(() =>
            {
                NUnit.Framework.Assert.That(fixture.PendingDisplay.Enabled, Is.False);
                NUnit.Framework.Assert.That(openings, Is.Zero);
                NUnit.Framework.Assert.That(fixture.MenuDisposals, Is.EqualTo(1));
                NUnit.Framework.Assert.That(fixture.Menu.Host.IsDisposed, Is.True);
                NUnit.Framework.Assert.That(fixture.Input.IsHandleCreated, Is.False);
            });
        }

        [TestCase(TooltipType.Default)]
        [TestCase(TooltipType.Form)]
        public void DisposingTheOwnerClosesTheVisibleCompletionAndReference(TooltipType tooltipType)
        {
            var original = Settings.Default.TooltipType;
            try
            {
                Settings.Default.TooltipType = tooltipType;
                using var fixture = new ExpressionFixture();
                fixture.Show();
                var help = Field<Control>(fixture.Menu.Host.ListView,
                    tooltipType == TooltipType.Default ? "toolTip" : "formTip");
                fixture.Menu.Show(fixture.Input, false);
                Pump(40);
                // A standalone help window may take focus and close the list.
                if (tooltipType == TooltipType.Default)
                    NUnit.Framework.Assert.That(fixture.Menu.Visible, Is.True);
                NUnit.Framework.Assert.That(help.Visible, Is.True);

                fixture.Window.Dispose();
                Pump(40);

                NUnit.Framework.Assert.Multiple(() =>
                {
                    NUnit.Framework.Assert.That(fixture.MenuDisposals, Is.EqualTo(1));
                    NUnit.Framework.Assert.That(fixture.Menu.Visible, Is.False);
                    NUnit.Framework.Assert.That(help.Visible, Is.False);
                    NUnit.Framework.Assert.That(help.IsDisposed, Is.True);
                });
            }
            finally { Settings.Default.TooltipType = original; }
        }

        [Test]
        public void SettingsAndSharedStateOnlyUpdateTheSurvivingExpressionEditor()
        {
            var originalOrder = Settings.Default.FunctionsOrder;
            var originalFont = Settings.Default.ExpressionFont;
            using var replacementFont = new Font(originalFont.FontFamily, originalFont.Size + 2,
                originalFont.Style, originalFont.Unit);
            var state = new RecordingViewState();
            using var closed = new ExpressionFixture(state);
            using var live = new ExpressionFixture(state);
            try
            {
                NUnit.Framework.Assert.That(state.SubscriberCount, Is.EqualTo(2));
                var closedReads = closed.Provider.Reads;
                var liveReads = live.Provider.Reads;
                var closedFontChanges = 0;
                var liveFontChanges = 0;
                closed.Input.FontChanged += (_, _) => closedFontChanges++;
                live.Input.FontChanged += (_, _) => liveFontChanges++;
                closed.Input.Dispose();

                Settings.Default.FunctionsOrder = originalOrder == FunctionsOrder.Default
                    ? FunctionsOrder.Alphabetical : FunctionsOrder.Default;
                Settings.Default.ExpressionFont = replacementFont;

                NUnit.Framework.Assert.Multiple(() =>
                {
                    NUnit.Framework.Assert.That(closed.Provider.Reads, Is.EqualTo(closedReads));
                    NUnit.Framework.Assert.That(live.Provider.Reads, Is.EqualTo(liveReads + 1));
                    NUnit.Framework.Assert.That(closedFontChanges, Is.Zero);
                    NUnit.Framework.Assert.That(liveFontChanges, Is.EqualTo(1));
                    NUnit.Framework.Assert.That(live.Input.Font.Size, Is.EqualTo(replacementFont.Size));
                    NUnit.Framework.Assert.That(state.SubscriberCount, Is.EqualTo(1));
                    NUnit.Framework.Assert.DoesNotThrow(() => state.IsExponent = true);
                    NUnit.Framework.Assert.That(closed.Input.IsHandleCreated, Is.False);
                });

                live.Input.Dispose();
                NUnit.Framework.Assert.That(state.SubscriberCount, Is.Zero);
                NUnit.Framework.Assert.DoesNotThrow(() => state.IsExponent = false);
            }
            finally
            {
                closed.Dispose();
                live.Dispose();
                Settings.Default.FunctionsOrder = originalOrder;
                Settings.Default.ExpressionFont = originalFont;
            }
        }

        private static IDictionary Bindings(string name) =>
            (IDictionary)typeof(Menu).GetField(name, BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

        private static T Field<T>(object instance, string name) =>
            (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);

        private static void Pump(int milliseconds)
        {
            var elapsed = Stopwatch.StartNew();
            while (elapsed.ElapsedMilliseconds < milliseconds)
            {
                Application.DoEvents();
                Thread.Sleep(5);
            }
        }

        private sealed class ExpressionInput : ExpressionTextBox
        {
            internal ExpressionInput(ISharedViewState state, IAutocompleteProvider provider) : base(state, provider) { }
            internal void RaiseKeyDown() => OnKeyDown(new KeyEventArgs(Keys.I));
        }

        private sealed class ExpressionFixture : IDisposable
        {
            private bool _disposed;
            internal readonly RecordingViewState State;
            internal readonly RecordingProvider Provider = new RecordingProvider();
            internal readonly Form Window = new Form { Size = new Size(500, 220), ShowInTaskbar = false };
            internal readonly ExpressionInput Input;
            internal readonly Menu Menu;
            internal int MenuDisposals;
            internal System.Windows.Forms.Timer PendingDisplay => Field<System.Windows.Forms.Timer>(Menu, "_timer");

            internal ExpressionFixture(RecordingViewState state = null)
            {
                State = state ?? new RecordingViewState();
                Input = new ExpressionInput(State, Provider) { Location = new Point(10, 10), Width = 220 };
                Menu = (Menu)typeof(ExpressionTextBox).GetField("_autocompleteMenu",
                    BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Input);
                Menu.Disposed += (_, _) => MenuDisposals++;
                Window.Controls.Add(Input);
            }

            internal void Show()
            {
                Window.Show();
                Input.Focus();
                Input.Text = "si";
                Input.SelectionStart = Input.TextLength;
                Pump(40);
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                Input.Dispose();
                // Also clean up an uncorrected production assembly, so a
                // failing regression cannot retain controls in later tests.
                if (MenuDisposals == 0)
                {
                    Menu.Close();
                    Menu.SetAutocompleteMenu(Input, null);
                    Menu.Dispose();
                }
                var handlers = (PropertyChangedEventHandler)typeof(Settings).GetField("PropertyChanged",
                    BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Settings.Default);
                foreach (var handler in handlers?.GetInvocationList() ?? Array.Empty<Delegate>())
                    if (ReferenceEquals(handler.Target, Input))
                        Settings.Default.PropertyChanged -= (PropertyChangedEventHandler)handler;
                State.RemoveSubscriber(Input);
                Window.Dispose();
            }
        }

        private sealed class RecordingProvider : IAutocompleteProvider
        {
            internal int Reads;
            public IEnumerable<AutocompleteItem> ExpressionAutocompleteItems
            {
                get
                {
                    Reads++;
                    return new[] { new AutocompleteItem("sin(x)") { Details = new FunctionInfo
                    { Signature = "sin(x)", Title = "Sine", Description = "<p>Sine function.</p>" } } };
                }
            }
            public IEnumerable<AutocompleteItem> ScriptingAutocompleteItems => Array.Empty<AutocompleteItem>();
        }

        private sealed class RecordingViewState : ISharedViewState
        {
            private bool _isExponent;
            public event PropertyChangedEventHandler PropertyChanged;
            internal int SubscriberCount => PropertyChanged?.GetInvocationList().Length ?? 0;
            public CalculationsMode CalculationsMode { get; set; }
            public Action<object, EventArgs> CurrentAction => null;
            public ViewName CurrentView { get; set; }
            public Dictionary<ViewName, Action<object, EventArgs>> DefaultActions { get; } = new();
            public bool IsExponent
            {
                get => _isExponent;
                set { _isExponent = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExponent))); }
            }
            internal void RemoveSubscriber(object target)
            {
                foreach (var handler in PropertyChanged?.GetInvocationList() ?? Array.Empty<Delegate>())
                    if (ReferenceEquals(handler.Target, target)) PropertyChanged -= (PropertyChangedEventHandler)handler;
            }
        }
    }
}
