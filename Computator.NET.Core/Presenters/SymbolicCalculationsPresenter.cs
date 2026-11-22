using System;
using System.Threading;
using System.Threading.Tasks;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Model;
using Computator.NET.Core.SymbolicCalculations;
using Computator.NET.DataTypes.Events;
using Computator.NET.Engine.Symbolic;

namespace Computator.NET.Core.Presenters
{
    /// <summary>One bounded symbolic computation per displayed view.</summary>
    public sealed class SymbolicCalculationsPresenter : IDisposable
    {
        private readonly ISymbolicCalculationsView _view;
        private readonly ISharedViewState _state;
        private readonly IExpressionTextBox _expression;
        private readonly ISymbolicCalculationService _service;
        private readonly Action<object, EventArgs> _runAction;
        private CancellationTokenSource _active;
        private int _generation;
        private volatile bool _disposed;
        private string _pendingStatus;

        public SymbolicCalculationsPresenter(ISymbolicCalculationsView view, ISharedViewState sharedViewState,
            IExpressionTextBox expressionTextBox, ISymbolicCalculationService service)
        {
            _view = view;
            _state = sharedViewState;
            _expression = expressionTextBox;
            _service = service;
            _runAction = OnCompute;
            _state.DefaultActions[ViewName.SymbolicCalculations] = _runAction;
            _view.ComputeClicked += OnCompute;
            _view.CancelClicked += OnCancel;
            _view.InputChanged += OnInputChanged;
            _view.ExampleRequested += OnExampleRequested;
            _view.Disposed += OnDisposed;
            _expression.TextChanged += OnInputChanged;
            _view.ShowStatus("Enter an expression above or load an example, then choose Calculate.");
        }

        public Task CurrentCalculation { get; private set; } = Task.CompletedTask;

        private void OnCompute(object sender, EventArgs args)
        {
            if (_disposed) return;
            // Cancellation is cooperative. Do not accumulate worker threads by
            // starting replacements while the previous CAS operation is unwinding.
            if (_active != null)
            {
                ++_generation;
                _pendingStatus = "Cancelled. You can calculate again.";
                RequestCancellation("Cancellation requested. Wait for the current calculation to stop, then run again.");
                return;
            }
            _view.ClearResult();
            SymbolicRequest request;
            try { request = _view.CreateRequest(_expression.Text); }
            catch (ArgumentException exception)
            {
                _view.ShowError(exception.Message);
                return;
            }
            _pendingStatus = null;
            var generation = ++_generation;
            var cancellation = new CancellationTokenSource();
            _active = cancellation;
            _view.SetBusy(true);
            _view.ShowStatus("Calculating… You can cancel or edit the input. Symbolic work has a time limit.");
            CurrentCalculation = CompleteAsync(request, cancellation, generation);
        }

        private async Task CompleteAsync(SymbolicRequest request, CancellationTokenSource cancellation, int generation)
        {
            SymbolicResult result = null;
            string error = null;
            var cancelled = false;
            try
            {
                result = await _service.ExecuteAsync(request, cancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { cancelled = true; }
            catch (Exception exception) { error = exception.Message; }
            finally { cancellation.Dispose(); }

            if (_disposed) return;
            _view.Dispatch(() =>
            {
                if (_disposed || !ReferenceEquals(_active, cancellation)) return;
                _active = null;
                _view.SetBusy(false);
                if (generation != _generation)
                {
                    _view.ShowStatus(_pendingStatus ?? "Inputs changed. Calculate again to update the result.");
                    return;
                }
                if (cancelled)
                    _view.ShowStatus("The calculation was cancelled or reached its time limit. Try a simpler expression.");
                else if (error != null)
                    _view.ShowError(error);
                else if (result != null)
                    _view.ShowResult(result);
                else
                    _view.ShowError("The symbolic engine returned no result.");
            });
        }

        private void OnInputChanged(object sender, EventArgs args)
        {
            if (_disposed) return;
            ++_generation;
            _view.ClearResult();
            _pendingStatus = "Inputs changed. Calculate again to update the result.";
            if (_active != null)
                RequestCancellation("Inputs changed. Waiting for the previous calculation to stop…");
            else
                _view.ShowStatus(_pendingStatus);
        }

        private void OnCancel(object sender, EventArgs args)
        {
            if (!_disposed && _active != null)
            {
                ++_generation;
                _pendingStatus = "Cancelled. You can edit the input or calculate again.";
                RequestCancellation("Cancellation requested. Waiting for the calculation to stop…");
            }
        }

        private void RequestCancellation(string status)
        {
            // A worker can complete immediately before its UI callback is pumped.
            try { _active?.Cancel(); }
            catch (ObjectDisposedException) { }
            _view.ShowStatus(status);
        }

        private void OnExampleRequested(object sender, SymbolicExampleEventArgs args)
        {
            if (!_disposed) _expression.Text = args.Expression;
        }

        private void OnDisposed(object sender, EventArgs args) => Dispose();

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            ++_generation;
            try { _active?.Cancel(); }
            catch (ObjectDisposedException) { }
            _active = null;
            _view.ComputeClicked -= OnCompute;
            _view.CancelClicked -= OnCancel;
            _view.InputChanged -= OnInputChanged;
            _view.ExampleRequested -= OnExampleRequested;
            _view.Disposed -= OnDisposed;
            _expression.TextChanged -= OnInputChanged;
            if (_state.DefaultActions.TryGetValue(ViewName.SymbolicCalculations, out var action) && action == _runAction)
                _state.DefaultActions.Remove(ViewName.SymbolicCalculations);
        }
    }
}
