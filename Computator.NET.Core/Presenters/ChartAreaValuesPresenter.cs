using System;
using System.ComponentModel;
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
        private readonly IComponent _viewComponent;

        public ChartAreaValuesPresenter(IChartAreaValuesView view, ISharedViewState sharedViewState)
        {
            _view = view;
            _viewComponent = view as IComponent;
            if (_viewComponent != null) _viewComponent.Disposed += OnViewDisposed;
            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(OnCalculationsModeChangedEvent);
            SetMode(sharedViewState.CalculationsMode);

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
            var xMin = $"{_view.XAxisLabel} min";
            var xMax = $"{_view.XAxisLabel} max";
            var yMin = $"{_view.YAxisLabel} min";
            var yMax = $"{_view.YAxisLabel} max";
            if (_view.XMax <= _view.XMin)
            {
                _view.SetError(nameof(_view.XMax), $"{xMax} {Strings.NeedsToBeGreaterThan} {xMin}");
                _view.SetError(nameof(_view.XMin), $"{xMin} {Strings.NeedsToBeLessThan} {xMax}");
            }
            else
            {
                _view.SetError(nameof(_view.XMax), null);
                _view.SetError(nameof(_view.XMin), null);
            }
            if (_view.YMin >= _view.YMax)
            {
                _view.SetError(nameof(_view.YMax), $"{yMax} {Strings.NeedsToBeGreaterThan} {yMin}");
                _view.SetError(nameof(_view.YMin), $"{yMin} {Strings.NeedsToBeLessThan} {yMax}");
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
            // Existing errors must describe the newly displayed coordinates,
            // including when a mode button changes them without editing a bound.
            OnValueChanged(this, EventArgs.Empty);
        }

        private void SetMode(CalculationsMode mode)
        {
            var complex = mode == CalculationsMode.Complex;
            _view.XAxisLabel = complex ? "Re(z)" : "x";
            _view.YAxisLabel = complex ? "Im(z)" : "y";
            _view.AddChartLabel = complex
                ? Strings.DrawChart
                : Strings.AddToChart;
        }

        private void OnViewDisposed(object sender, EventArgs e) => Dispose();

        public void Dispose()
        {
            if (_viewComponent != null) _viewComponent.Disposed -= OnViewDisposed;
            EventAggregator.Instance.Unsubscribe<CalculationsModeChangedEvent>(OnCalculationsModeChangedEvent);
            _view.YMaxChanged -= OnValueChanged;
            _view.XMaxChanged -= OnValueChanged;
            _view.XMinChanged -= OnValueChanged;
            _view.YMinChanged -= OnValueChanged;
        }
    }
}
