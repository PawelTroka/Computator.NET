using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Model;
using Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class AvalonEditFoldingWorkflowTests
    {
        private const string Block = "if (true) {\nwrite(42);\n}";
        private AvalonEditCodeEditor _editor;

        [SetUp]
        public void CreateProductionEditor()
        {
            var autocomplete = new Mock<IAutocompleteProvider>();
            autocomplete.SetupGet(provider => provider.ScriptingAutocompleteItems)
                .Returns(Array.Empty<AutocompleteItem>());
            _editor = new AvalonEditCodeEditor(Mock.Of<ISharedViewState>(), autocomplete.Object);
        }

        [TearDown]
        public void DetachFoldingServices()
        {
            _editor.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        }

        [Test]
        public void NewAndSwitchedDocumentsHaveIndependentFoldedSections()
        {
            _editor.NewDocument("a.tsl", Block);
            var a = _editor.Document;
            var originalManager = Manager();
            originalManager.AllFoldings.Single().IsFolded = true;
            _editor.NewDocument("b.tsl", "write(1);\n" + Block);
            NUnit.Framework.Assert.That(Manager(), Is.Not.SameAs(originalManager));
            NUnit.Framework.Assert.That(Manager().AllFoldings.Single().IsFolded, Is.False);
            _editor.SwitchDocument("a.tsl");

            NUnit.Framework.Assert.That(_editor.Document, Is.SameAs(a));
            NUnit.Framework.Assert.That(Manager().AllFoldings.Single().IsFolded, Is.True);
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(Block));
            NUnit.Framework.Assert.That(a.UndoStack.CanUndo, Is.False, "Folding must not edit the source.");
            AssertSingleInstallation();
        }

        [Test]
        public void DetachedDocumentEditsMoveSavedFoldAnchors()
        {
            _editor.NewDocument("a.tsl", Block);
            var a = _editor.Document;
            Manager().AllFoldings.Single().IsFolded = true;
            _editor.NewDocument("b.tsl", Block);
            a.Insert(0, "// inserted while detached\n");
            _editor.SwitchDocument("a.tsl");

            var section = Manager().AllFoldings.Single();
            NUnit.Framework.Assert.That(section.IsFolded, Is.True);
            NUnit.Framework.Assert.That(section.StartOffset, Is.EqualTo(_editor.Text.IndexOf('{')));
            NUnit.Framework.Assert.That(section.EndOffset, Is.EqualTo(_editor.Text.Length));
        }

        [Test]
        public void ReplacingDetachedFoldedBlockDoesNotCollapseItsReplacement()
        {
            _editor.NewDocument("a.tsl", Block);
            var a = _editor.Document;
            Manager().AllFoldings.Single().IsFolded = true;
            _editor.NewDocument("b.tsl", Block);
            a.Text = Block.Replace("42", "99");
            _editor.SwitchDocument("a.tsl");

            NUnit.Framework.Assert.That(Manager().AllFoldings.Single().IsFolded, Is.False);
            NUnit.Framework.Assert.That(_editor.Text, Does.Contain("99"));
        }

        [Test]
        public void CloseAndRenameKeepFoldStateWithTheSurvivingDocument()
        {
            _editor.NewDocument("a.tsl", Block);
            _editor.NewDocument("b.tsl", "write(1);");
            _editor.SwitchDocument("a.tsl");
            Manager().AllFoldings.Single().IsFolded = true;
            var manager = Manager();
            _editor.CloseDocument("b.tsl");
            NUnit.Framework.Assert.That(Manager(), Is.SameAs(manager));
            _editor.RenameDocument("a.tsl", "renamed.tsl");
            _editor.NewDocument("c.tsl", "write(2);");
            _editor.CloseDocument("c.tsl");
            NUnit.Framework.Assert.That(Manager().AllFoldings.Single().IsFolded, Is.True);
            _editor.CloseDocument("renamed.tsl");

            NUnit.Framework.Assert.That(_editor.Documents, Is.Empty);
            NUnit.Framework.Assert.That(_editor.Text, Is.Empty);
            NUnit.Framework.Assert.That(Manager().AllFoldings, Is.Empty);
            _editor.NewDocument("a.tsl", Block);
            NUnit.Framework.Assert.That(Manager().AllFoldings.Single().IsFolded, Is.False);
            AssertSingleInstallation();
        }

        [Test]
        public void ReloadInstallsOneManagerAndPreservesFolds()
        {
            _editor.NewDocument("a.tsl", Block);
            Manager().AllFoldings.Single().IsFolded = true;
            for (var i = 0; i < 3; i++)
            {
                _editor.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                NUnit.Framework.Assert.That(Manager(), Is.Null);
                NUnit.Framework.Assert.That(_editor.TextArea.LeftMargins.OfType<FoldingMargin>(), Is.Empty);
                _editor.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                AssertSingleInstallation();
                NUnit.Framework.Assert.That(Manager().AllFoldings.Single().IsFolded, Is.True);
            }
        }

        [Test]
        public void VisibleEditorUpdatesFoldsAndCancelsPendingWorkWhenSwitchingOrUnloading()
        {
            var window = new Window { Content = _editor, Width = 600, Height = 400, ShowInTaskbar = false };
            try
            {
                window.Show();
                PumpDispatcher(TimeSpan.FromMilliseconds(20));
                NUnit.Framework.Assert.That(_editor.IsLoaded, Is.True);
                _editor.NewDocument("a.tsl", Block);
                _editor.Document.Insert(_editor.Document.TextLength, "\n" + Block);
                PumpDispatcher(TimeSpan.FromMilliseconds(500));
                NUnit.Framework.Assert.That(Manager().AllFoldings.Count(), Is.EqualTo(2));
                _editor.Document.Insert(0, "// pending change\n");
                _editor.NewDocument("b.tsl", "write(1);");
                PumpDispatcher(TimeSpan.FromMilliseconds(350));
                NUnit.Framework.Assert.That(Manager().AllFoldings, Is.Empty);
                _editor.Text = Block;
                window.Content = null;
                PumpDispatcher(TimeSpan.FromMilliseconds(350));
                NUnit.Framework.Assert.That(Manager(), Is.Null);
                window.Content = _editor;
                PumpDispatcher(TimeSpan.FromMilliseconds(50));
                NUnit.Framework.Assert.That(Manager().AllFoldings.Count(), Is.EqualTo(1));
                AssertSingleInstallation();
            }
            finally
            {
                window.Content = null;
                window.Close();
            }
        }

        private FoldingManager Manager() =>
            (FoldingManager)_editor.TextArea.TextView.Services.GetService(typeof(FoldingManager));

        private void AssertSingleInstallation()
        {
            NUnit.Framework.Assert.That(_editor.TextArea.LeftMargins.OfType<FoldingMargin>().Count(), Is.EqualTo(1));
            NUnit.Framework.Assert.That(_editor.TextArea.TextView.ElementGenerators.OfType<FoldingElementGenerator>().Count(), Is.EqualTo(1));
        }

        private static void PumpDispatcher(TimeSpan duration)
        {
            var frame = new DispatcherFrame();
            var timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
            {
                Interval = duration
            };
            timer.Tick += (sender, args) => { timer.Stop(); frame.Continue = false; };
            timer.Start();
            Dispatcher.PushFrame(frame);
        }
    }
}
