namespace Computator.NET.DataTypes.Events
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