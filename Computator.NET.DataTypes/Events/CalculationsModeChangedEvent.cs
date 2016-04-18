using Computator.NET.Evaluation;

namespace Computator.NET.DataTypes
{
    public class CalculationsModeChangedEvent : IApplicationEvent
    {
        public CalculationsModeChangedEvent(CalculationsMode mode)
        {
            CalculationsMode = mode;
        }

        public CalculationsMode CalculationsMode { get; private set; }
    }
}