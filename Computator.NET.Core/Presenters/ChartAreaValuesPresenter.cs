using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Validation;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;

namespace Computator.NET.Core.Presenters
{
    public class ChartAreaValuesPresenter
    {
        private readonly IChartAreaValuesView _view;

        public ChartAreaValuesPresenter(IChartAreaValuesView view)
        {
            _view = view;
            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(OnCalculationsModeChangedEvent);

            _view.YMaxChanged += OnValueChanged;
            _view.XMaxChanged += OnValueChanged;
            _view.XMinChanged += OnValueChanged;
            _view.YMinChanged += OnValueChanged;
        }

        private void OnValueChanged(object sender, System.EventArgs e)
        {
            if (_view.XMax <= _view.XMin)
            {
                _view.SetError(nameof(_view.XMax),"xN needs to be greater than x0");
                _view.SetError(nameof(_view.XMin), "x0 needs to be less than xN");
            }
            else
            {
                _view.SetError(nameof(_view.XMax), null);
                _view.SetError(nameof(_view.XMin), null);
            }
            if (_view.YMin >= _view.YMax)
            {
                _view.SetError(nameof(_view.YMax), "yN needs to be greater than y0");
                _view.SetError(nameof(_view.YMin), "y0 needs to be less than yN");
            }
            else
            {
                _view.SetError(nameof(_view.YMax), null);
                _view.SetError(nameof(_view.YMin), null);
            }
        }

        private void OnCalculationsModeChangedEvent(CalculationsModeChangedEvent e)
        {
            _view.AddChartLabel = e.CalculationsMode == CalculationsMode.Complex
                ? Strings.DrawChart
                : Strings.AddToChart;
        }
    }
}