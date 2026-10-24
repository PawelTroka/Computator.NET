using System;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.Localization;

namespace Computator.NET.Core.Presenters
{
    public class ChartAreaValuesPresenter : IDisposable
    {
        private readonly IChartAreaValuesView _view;

        public ChartAreaValuesPresenter(IChartAreaValuesView view, ISharedViewState sharedViewState)
        {
            _view = view;
            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(OnCalculationsModeChangedEvent);
            _view.Disposed += OnViewDisposed;

            _view.YMaxChanged += OnValueChanged;
            _view.XMaxChanged += OnValueChanged;
            _view.XMinChanged += OnValueChanged;
            _view.YMinChanged += OnValueChanged;
            _view.YMax = 3;
            _view.XMax = 5;
            _view.YMin = -_view.YMax;
            _view.XMin = -_view.XMax;
            SetMode(sharedViewState.CalculationsMode);
        }

        private void OnValueChanged(object sender, System.EventArgs e)
        {
            if (_view.XMax <= _view.XMin)
            {
                _view.SetError(nameof(_view.XMax), $"{_view.XAxisLabel} max {Strings.NeedsToBeGreaterThan} {_view.XAxisLabel} min");
                _view.SetError(nameof(_view.XMin), $"{_view.XAxisLabel} min {Strings.NeedsToBeLessThan} {_view.XAxisLabel} max");
            }
            else
            {
                _view.SetError(nameof(_view.XMax), null);
                _view.SetError(nameof(_view.XMin), null);
            }
            if (_view.YMin >= _view.YMax)
            {
                _view.SetError(nameof(_view.YMax), $"{_view.YAxisLabel} max {Strings.NeedsToBeGreaterThan} {_view.YAxisLabel} min");
                _view.SetError(nameof(_view.YMin), $"{_view.YAxisLabel} min {Strings.NeedsToBeLessThan} {_view.YAxisLabel} max");
            }
            else
            {
                _view.SetError(nameof(_view.YMax), null);
                _view.SetError(nameof(_view.YMin), null);
            }
        }

        private void OnCalculationsModeChangedEvent(CalculationsModeChangedEvent e)
        {
            SetMode(e.CalculationsMode);
        }

        private void SetMode(CalculationsMode mode)
        {
            _view.XAxisLabel = mode == CalculationsMode.Complex ? "Re(z)" : "x";
            _view.YAxisLabel = mode == CalculationsMode.Complex ? "Im(z)" : "y";
            _view.AddChartLabel = mode == CalculationsMode.Complex
                ? Strings.DrawChart
                : Strings.AddToChart;
            // Existing invalid values keep their errors, now using the visible axis names.
            OnValueChanged(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            EventAggregator.Instance.Unsubscribe<CalculationsModeChangedEvent>(OnCalculationsModeChangedEvent);
            _view.Disposed -= OnViewDisposed;
            _view.YMaxChanged -= OnValueChanged;
            _view.XMaxChanged -= OnValueChanged;
            _view.XMinChanged -= OnValueChanged;
            _view.YMinChanged -= OnValueChanged;
        }

        private void OnViewDisposed(object sender, EventArgs e) => Dispose();
    }
}
