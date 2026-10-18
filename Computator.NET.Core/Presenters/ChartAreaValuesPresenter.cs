using System;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.Localization;

namespace Computator.NET.Core.Presenters
{
    public class ChartAreaValuesPresenter : IDisposable
    {
        private readonly IChartAreaValuesView _view;

        public ChartAreaValuesPresenter(IChartAreaValuesView view)
        {
            _view = view;
            _view.XAxisName = "x";
            _view.YAxisName = "y";
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

        private void OnValueChanged(object sender, EventArgs e)
        {
            if (_view.XMax <= _view.XMin)
            {
                _view.SetError(nameof(_view.XMax), $"{_view.XAxisName} max {Strings.NeedsToBeGreaterThan} {_view.XAxisName} min");
                _view.SetError(nameof(_view.XMin), $"{_view.XAxisName} min {Strings.NeedsToBeLessThan} {_view.XAxisName} max");
            }
            else
            {
                _view.SetError(nameof(_view.XMax), null);
                _view.SetError(nameof(_view.XMin), null);
            }
            if (_view.YMin >= _view.YMax)
            {
                _view.SetError(nameof(_view.YMax), $"{_view.YAxisName} max {Strings.NeedsToBeGreaterThan} {_view.YAxisName} min");
                _view.SetError(nameof(_view.YMin), $"{_view.YAxisName} min {Strings.NeedsToBeLessThan} {_view.YAxisName} max");
            }
            else
            {
                _view.SetError(nameof(_view.YMax), null);
                _view.SetError(nameof(_view.YMin), null);
            }
        }

        private void OnCalculationsModeChangedEvent(CalculationsModeChangedEvent e)
        {
            // An incomplete expression is not a new coordinate system. Keep the
            // last chart's labels while the user finishes editing it.
            if (e.CalculationsMode == CalculationsMode.Error)
                return;

            _view.XAxisName = e.CalculationsMode == CalculationsMode.Complex ? "Re(z)" : "x";
            _view.YAxisName = e.CalculationsMode == CalculationsMode.Complex ? "Im(z)" : "y";
            _view.AddChartLabel = e.CalculationsMode == CalculationsMode.Complex
                ? Strings.DrawChart
                : Strings.AddToChart;
            OnValueChanged(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            EventAggregator.Instance.Unsubscribe<CalculationsModeChangedEvent>(OnCalculationsModeChangedEvent);
            _view.YMaxChanged -= OnValueChanged;
            _view.XMaxChanged -= OnValueChanged;
            _view.XMinChanged -= OnValueChanged;
            _view.YMinChanged -= OnValueChanged;
        }
    }
}
