using System;
using Computator.NET.Charting;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.UI.ErrorHandling;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Presenters
{
    public class ChartingViewPresenter
    {
        private readonly IErrorHandler _errorHandler;
        private readonly ExpressionsEvaluator _expressionsEvaluator = new ExpressionsEvaluator();
        private readonly IChartingView _view;
        private ISharedViewState _sharedViewState;
        private CalculationsMode _calculationsMode;

        public ChartingViewPresenter(IChartingView view, IErrorHandler errorHandler, ISharedViewState sharedViewState)
        {
            _view = view;
            _errorHandler = errorHandler;
            _sharedViewState = sharedViewState;

            var chartAreaValuesViewPresenter = new ChartAreaValuesPresenter(_view.ChartAreaValuesView);


            _sharedViewState.DefaultActions[ViewName.Charting] = ChartAreaValuesView1_AddClicked;


            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(mode => SetMode(mode.CalculationsMode));

            _view.ChartAreaValuesView.QualityChanged += ChartAreaValuesView_QualityChanged;

            _view.ChartAreaValuesView.XMinChanged += ChartAreaValuesView_XMinChanged;
            _view.ChartAreaValuesView.XMaxChanged += ChartAreaValuesView_XMaxChanged;
            _view.ChartAreaValuesView.YMinChanged += ChartAreaValuesView_YMinChanged;
            _view.ChartAreaValuesView.YMaxChanged += ChartAreaValuesView_YMaxChanged;

            foreach (var chart in _view.Charts)
            {
                chart.Value.XMinChanged += (o, e) => _view.ChartAreaValuesView.XMin = chart.Value.XMin;
                chart.Value.XMaxChanged += (o, e) => _view.ChartAreaValuesView.XMax = chart.Value.XMax;
                chart.Value.YMinChanged += (o, e) => _view.ChartAreaValuesView.YMin = chart.Value.YMin;
                chart.Value.YMaxChanged += (o, e) => _view.ChartAreaValuesView.YMax = chart.Value.YMax;
            }

            _view.ChartAreaValuesView.AddClicked += ChartAreaValuesView1_AddClicked;
            _view.ChartAreaValuesView.ClearClicked += ChartAreaValuesView1_ClearClicked;

            foreach (var chart in _view.Charts)
            {
                chart.Value.SetChartAreaValues(_view.ChartAreaValuesView.XMin, _view.ChartAreaValuesView.XMax,
                    _view.ChartAreaValuesView.YMin, _view.ChartAreaValuesView.YMax);
            }
        }

        private IChart CurrentChart => _view.Charts[_calculationsMode];

        private void SetMode(CalculationsMode mode)
        {
            if (mode != _calculationsMode)
            {
                _calculationsMode = mode;
                foreach (var chart in _view.Charts)
                    chart.Value.Visible = chart.Key == _calculationsMode;
            }
        }

        private void ChartAreaValuesView1_AddClicked(object sender, EventArgs e)
        {
            if (_sharedViewState.ExpressionText != "")
            {
                try
                {
                    _sharedViewState.CustomFunctionsEditor.ClearHighlightedErrors();
                    CurrentChart.AddFunction(_expressionsEvaluator.Evaluate(_sharedViewState.ExpressionText,
                        _sharedViewState.CustomFunctionsText,
                        _calculationsMode));
                }
                catch (Exception ex)
                {
                    ExceptionsHandler.Instance.HandleException(ex, _errorHandler);
                }
            }
            else
                _errorHandler.DispalyError(Strings.GUI_addToChartButton_Click_Expression_should_not_be_empty_,
                    Strings.GUI_numericalOperationButton_Click_Warning_);
        }


        private void ChartAreaValuesView1_ClearClicked(object sender, EventArgs e)
        {
            foreach (var chart in _view.Charts)
            {
                chart.Value.ClearAll();
            }
        }

        private void ChartAreaValuesView_YMaxChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.Charts)
            {
                chart.Value.YMax = _view.ChartAreaValuesView.YMax;
            }
        }

        private void ChartAreaValuesView_YMinChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.Charts)
            {
                chart.Value.YMin = _view.ChartAreaValuesView.YMin;
            }
        }

        private void ChartAreaValuesView_XMaxChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.Charts)
            {
                chart.Value.XMax = _view.ChartAreaValuesView.XMax;
            }
        }

        private void ChartAreaValuesView_XMinChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.Charts)
            {
                chart.Value.XMin = _view.ChartAreaValuesView.XMin;
            }
        }

        private void ChartAreaValuesView_QualityChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.Charts)
            {
                chart.Value.Quality = _view.ChartAreaValuesView.Quality;
            }
        }
    }
}