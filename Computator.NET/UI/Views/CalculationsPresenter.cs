// Computator.NET Copyright © 2016 - 2016 Pawel Troka

using Computator.NET.DataTypes;
using Computator.NET.Evaluation;

namespace Computator.NET.UI.Views
{
    public class CalculationsPresenter
    {
        private readonly ICalculationsView _view;
        private CalculationsMode calculationsMode;

        public CalculationsPresenter(ICalculationsView view)
        {
            _view = view;
            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(_ModeChanged);
        }

        private void _ModeChanged(CalculationsModeChangedEvent calculationsModeChangedEvent)
        {
            if (calculationsModeChangedEvent.CalculationsMode == calculationsMode) return;

            _view.YVisible = calculationsModeChangedEvent.CalculationsMode == CalculationsMode.Complex ||
                             calculationsModeChangedEvent.CalculationsMode == CalculationsMode.Fxy;

            switch (calculationsModeChangedEvent.CalculationsMode)
            {
                case CalculationsMode.Complex:
                    _view.XLabel = "Re(z) =";
                    _view.YLabel = "Im(z) =";
                    break;
                case CalculationsMode.Fxy:
                    _view.XLabel = "       x =";
                    _view.YLabel = "       y =";
                    break;
                case CalculationsMode.Real:
                    _view.XLabel = "       x =";
                    break;
            }

            calculationsMode = calculationsModeChangedEvent.CalculationsMode;
        }
    }
}