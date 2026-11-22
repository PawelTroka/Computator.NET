using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Model;
using Computator.NET.Core.Presenters;
using Computator.NET.Core.SymbolicCalculations;
using Computator.NET.DataTypes.Events;
using Computator.NET.Engine.Symbolic;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.PresentersTests
{
    [TestFixture]
    public class SymbolicCalculationsPresenterTests
    {
        private FakeView _view;
        private FakeExpression _expression;
        private ControlledService _service;
        private ISharedViewState _state;
        private SymbolicCalculationsPresenter _presenter;

        [SetUp]
        public void CreatePresenter()
        {
            _view = new FakeView();
            _expression = new FakeExpression { Text = "x²" };
            _service = new ControlledService();
            var state = new Mock<ISharedViewState>();
            state.SetupGet(value => value.DefaultActions).Returns(new Dictionary<ViewName, Action<object, EventArgs>>());
            _state = state.Object;
            _presenter = new SymbolicCalculationsPresenter(_view, _state, _expression, _service);
        }

        [TearDown]
        public void DisposePresenter() => _presenter.Dispose();

        [Test]
        public async Task ToolbarActionCapturesTheDisplayedInputAndPublishesTheResultOnTheViewDispatcher()
        {
            _expression.Text = "sin²(x) + cos²(x)";
            _state.DefaultActions[ViewName.SymbolicCalculations](this, EventArgs.Empty);
            NUnit.Framework.Assert.That(_service.Request.Expression, Is.EqualTo(_expression.Text));
            NUnit.Framework.Assert.That(_view.Busy, Is.True);
            _service.Completion.SetResult(new SymbolicResult { Result = "1", Latex = "1", Status = "exact" });
            await _presenter.CurrentCalculation;
            NUnit.Framework.Assert.That(_view.Result, Is.Null, "Worker completion must not touch native controls directly.");
            _view.Drain();
            NUnit.Framework.Assert.That(_view.Busy, Is.False);
            NUnit.Framework.Assert.That(_view.Result.Result, Is.EqualTo("1"));
            NUnit.Framework.Assert.That(_view.Result.Latex, Is.EqualTo("1"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task EditingAnExpressionOrParameterCancelsWorkAndRejectsItsLateAnswer(bool parameter)
        {
            _view.Compute();
            if (parameter) _view.ChangeInput();
            else _expression.Text = "x³";
            NUnit.Framework.Assert.That(_service.Token.IsCancellationRequested, Is.True);
            NUnit.Framework.Assert.That(_view.Busy, Is.True, "Cancellation must unwind before another worker starts.");
            _service.Completion.SetResult(new SymbolicResult { Result = "stale result" });
            await _presenter.CurrentCalculation;
            _view.Drain();
            NUnit.Framework.Assert.That(_view.Busy, Is.False);
            NUnit.Framework.Assert.That(_view.Result, Is.Null);
            NUnit.Framework.Assert.That(_view.Status, Does.Contain("Inputs changed"));
        }

        [Test]
        public async Task InputChangesAfterWorkerCompletionStillInvalidateItsQueuedUiCallback()
        {
            _view.Compute();
            _service.Completion.SetResult(new SymbolicResult { Result = "old" });
            await _presenter.CurrentCalculation;
            _expression.Text = "y + 1";
            _view.Drain();
            NUnit.Framework.Assert.That(_view.Result, Is.Null);
            NUnit.Framework.Assert.That(_view.Busy, Is.False);
        }

        [Test]
        public async Task RepeatedRunAndCancelDoNotCreateOverlappingWorkers()
        {
            _view.Compute();
            _view.Cancel();
            _view.Compute();
            _state.DefaultActions[ViewName.SymbolicCalculations](this, EventArgs.Empty);
            NUnit.Framework.Assert.That(_service.Calls, Is.EqualTo(1));
            NUnit.Framework.Assert.That(_service.Token.IsCancellationRequested, Is.True);
            _service.Completion.SetResult(new SymbolicResult { Result = "must be ignored" });
            await _presenter.CurrentCalculation;
            _view.Drain();
            NUnit.Framework.Assert.That(_view.Result, Is.Null);
            NUnit.Framework.Assert.That(_view.Busy, Is.False);
        }

        [Test]
        public async Task CancellationAndErrorsRecoverToAWorkingNextCalculation()
        {
            _view.Compute();
            _service.Completion.SetCanceled();
            await _presenter.CurrentCalculation;
            _view.Drain();
            NUnit.Framework.Assert.That(_view.Status, Does.Contain("time limit"));
            NUnit.Framework.Assert.That(_view.Busy, Is.False);

            _service.Reset();
            _view.Compute();
            _service.Completion.SetException(new ArgumentException("Invalid symbolic expression."));
            await _presenter.CurrentCalculation;
            _view.Drain();
            NUnit.Framework.Assert.That(_view.Error, Does.Contain("Invalid symbolic expression"));
            NUnit.Framework.Assert.That(_view.Busy, Is.False);

            _service.Reset();
            _expression.Text = "2 + 2";
            _view.Compute();
            _service.Completion.SetResult(new SymbolicResult { Result = "4" });
            await _presenter.CurrentCalculation;
            _view.Drain();
            NUnit.Framework.Assert.That(_view.Result.Result, Is.EqualTo("4"));
            NUnit.Framework.Assert.That(_view.Error, Is.Null);
        }

        [Test]
        public async Task AnErrorFromAnInvalidatedRequestDoesNotReplaceTheNewInputsStatus()
        {
            _view.Compute();
            _expression.Text = "corrected";
            _service.Completion.SetException(new ArgumentException("obsolete diagnostic"));
            await _presenter.CurrentCalculation;
            _view.Drain();
            NUnit.Framework.Assert.That(_view.Error, Is.Null);
            NUnit.Framework.Assert.That(_view.Status, Does.Contain("Inputs changed"));
        }

        [Test]
        public void InvalidStructuredInputIsReportedWithoutStartingAWorker()
        {
            _view.RequestError = new ArgumentException("Duplicate replacement for x.");
            _view.Compute();
            NUnit.Framework.Assert.That(_service.Calls, Is.Zero);
            NUnit.Framework.Assert.That(_view.Busy, Is.False);
            NUnit.Framework.Assert.That(_view.Error, Does.Contain("Duplicate replacement"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task DisposalCancelsDetachesAndPreventsLateUiUpdates(bool disposeView)
        {
            _view.Compute();
            if (disposeView) _view.Close();
            else _presenter.Dispose();
            NUnit.Framework.Assert.That(_service.Token.IsCancellationRequested, Is.True);
            NUnit.Framework.Assert.That(_state.DefaultActions.ContainsKey(ViewName.SymbolicCalculations), Is.False);
            var changes = _view.Changes;
            _expression.Text = "later";
            _view.Compute();
            _view.ChangeInput();
            _view.Example("must not replace expression");
            _service.Completion.SetResult(new SymbolicResult { Result = "obsolete" });
            await _presenter.CurrentCalculation;
            _view.Drain();
            NUnit.Framework.Assert.That(_view.Changes, Is.EqualTo(changes));
            NUnit.Framework.Assert.That(_expression.Text, Is.EqualTo("later"));
        }

        [Test]
        public void DisposalPreservesAnActionInstalledByANewerPresenter()
        {
            Action<object, EventArgs> replacement = (_, _) => { };
            _state.DefaultActions[ViewName.SymbolicCalculations] = replacement;
            _presenter.Dispose();
            NUnit.Framework.Assert.That(_state.DefaultActions[ViewName.SymbolicCalculations], Is.SameAs(replacement));
        }

        [Test]
        public void LoadingAnExampleUpdatesTheSharedExpressionEditor()
        {
            _view.Example("sin²(x)+2·x");
            NUnit.Framework.Assert.That(_expression.Text, Is.EqualTo("sin²(x)+2·x"));
        }

        private sealed class ControlledService : ISymbolicCalculationService
        {
            public TaskCompletionSource<SymbolicResult> Completion;
            public SymbolicRequest Request;
            public CancellationToken Token;
            public int Calls;
            public ControlledService() { Reset(); }
            public void Reset() => Completion = new TaskCompletionSource<SymbolicResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            public Task<SymbolicResult> ExecuteAsync(SymbolicRequest request, CancellationToken cancellationToken)
            { Calls++; Request = request; Token = cancellationToken; return Completion.Task; }
        }

        private sealed class FakeExpression : IExpressionTextBox
        {
            private string _text;
            public string Text { get => _text; set { _text = value; TextChanged?.Invoke(this, EventArgs.Empty); } }
            public event EventHandler TextChanged;
            public event KeyPressEventHandler KeyPress { add { } remove { } }
        }

        private sealed class FakeView : ISymbolicCalculationsView
        {
            private readonly Queue<Action> _callbacks = new Queue<Action>();
            public bool Busy;
            public int Changes;
            public SymbolicResult Result;
            public string Error;
            public string Status;
            public ArgumentException RequestError;
            public SymbolicRequest CreateRequest(string expression)
            {
                if (RequestError != null) throw RequestError;
                return new SymbolicRequest { Expression = expression };
            }
            public void SetBusy(bool busy) { Busy = busy; Changes++; }
            public void ShowResult(SymbolicResult result) { Result = result; Changes++; }
            public void ShowError(string message) { Error = message; Changes++; }
            public void ShowStatus(string message) { Status = message; Changes++; }
            public void ClearResult() { Result = null; Error = null; Changes++; }
            public void Dispatch(Action action) { lock (_callbacks) _callbacks.Enqueue(action); }
            public void Drain() { lock (_callbacks) while (_callbacks.Count != 0) _callbacks.Dequeue()(); }
            public event EventHandler ComputeClicked;
            public event EventHandler CancelClicked;
            public event EventHandler InputChanged;
            public event EventHandler<SymbolicExampleEventArgs> ExampleRequested;
            public event EventHandler Disposed;
            public void Compute() => ComputeClicked?.Invoke(this, EventArgs.Empty);
            public void Cancel() => CancelClicked?.Invoke(this, EventArgs.Empty);
            public void ChangeInput() => InputChanged?.Invoke(this, EventArgs.Empty);
            public void Example(string expression) => ExampleRequested?.Invoke(this, new SymbolicExampleEventArgs(expression));
            public void Close() => Disposed?.Invoke(this, EventArgs.Empty);
        }
    }
}
