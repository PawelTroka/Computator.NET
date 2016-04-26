namespace Computator.NET.DataTypes.Events
{
    public class ExponentModeChangedEvent : IApplicationEvent
    {
        public ExponentModeChangedEvent(bool isExponentMode)
        {
            IsExponentMode = isExponentMode;
        }

        public bool IsExponentMode { get; private set; }
    }
}