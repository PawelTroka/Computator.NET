using System;
using Computator.NET.Engine.Symbolic;

namespace Computator.NET.Core.Abstract.Views
{
    public interface ISymbolicCalculationsView
    {
        SymbolicRequest CreateRequest(string expression);
        void SetBusy(bool busy);
        void ShowResult(SymbolicResult result);
        void ShowError(string message);
        void ShowStatus(string message);
        void ClearResult();
        // The view owns the native UI dispatcher, including its disposed-handle guard.
        void Dispatch(Action action);
        event EventHandler ComputeClicked;
        event EventHandler CancelClicked;
        event EventHandler InputChanged;
        event EventHandler<SymbolicExampleEventArgs> ExampleRequested;
        event EventHandler Disposed;
    }

    public sealed class SymbolicExampleEventArgs : EventArgs
    {
        public SymbolicExampleEventArgs(string expression) { Expression = expression; }
        public string Expression { get; }
    }
}
