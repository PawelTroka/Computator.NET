using System.Threading;
using System.Threading.Tasks;
using Computator.NET.Engine.Symbolic;

namespace Computator.NET.Core.SymbolicCalculations
{
    public interface ISymbolicCalculationService
    {
        Task<SymbolicResult> ExecuteAsync(SymbolicRequest request, CancellationToken cancellationToken);
    }

    public sealed class SymbolicCalculationService : ISymbolicCalculationService
    {
        public Task<SymbolicResult> ExecuteAsync(SymbolicRequest request, CancellationToken cancellationToken)
        {
            // The shared engine bounds input/work and installs its cancellation context
            // on this worker. No CAS parsing or computation runs on the native UI thread.
            return Task.Run(() => new SymbolicEngine().Execute(request, cancellationToken), cancellationToken);
        }
    }
}
