using Computator.NET.Core.Abstract.Views;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.Localization;

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
            _view.YMax = 3;
            _view.XMax = 5;
            _view.YMin = -_view.YMax;
            _view.XMin = -_view.XMax;
        }

        private void OnValueChanged(object sender, System.EventArgs e)
        {
            if (_view.XMax <= _view.XMin)
            {
                _view.SetError(nameof(_view.XMax),$"xN {Strings.NeedsToBeGreaterThan} x0");
                _view.SetError(nameof(_view.XMin), $"x0 {Strings.NeedsToBeLessThan} xN");
            }
            else
            {
                _view.SetError(nameof(_view.XMax), null);
                _view.SetError(nameof(_view.XMin), null);
            }
            if (_view.YMin >= _view.YMax)
            {
                _view.SetError(nameof(_view.YMax), $"yN {Strings.NeedsToBeGreaterThan} y0");
                _view.SetError(nameof(_view.YMin), $"y0 {Strings.NeedsToBeLessThan} yN");
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