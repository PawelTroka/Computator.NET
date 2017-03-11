using System;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Model;
using Computator.NET.Core.Services.ErrorHandling;
using Computator.NET.Core.Validation;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;

namespace Computator.NET.Core.Presenters
{
    public class ChartingViewPresenter
    {
        private readonly IErrorHandler _errorHandler;
        private readonly ExpressionsEvaluator _expressionsEvaluator = new ExpressionsEvaluator();
        private readonly IChartingView _view;
        private readonly ISharedViewState _sharedViewState;
        private CalculationsMode _calculationsMode;
        private readonly ITextProvider _expressionTextProvider;

        public ChartingViewPresenter(IChartingView view, IErrorHandler errorHandler, ISharedViewState sharedViewState,
            IExceptionsHandler exceptionsHandler, ITextProvider expressionTextProvider,
            ICodeEditorView customFunctionsEditor)
        {
            _view = view;
            _errorHandler = errorHandler;
            _sharedViewState = sharedViewState;
            _exceptionsHandler = exceptionsHandler;
            _expressionTextProvider = expressionTextProvider;
            this.customFunctionsEditor = customFunctionsEditor;

            /////////////var chartAreaValuesViewPresenter = new ChartAreaValuesPresenter(_view.ChartAreaValuesView);


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
                chart.Value.SetChartAreaValues(_view.ChartAreaValuesView.XMin, _view.ChartAreaValuesView.XMax,
                    _view.ChartAreaValuesView.YMin, _view.ChartAreaValuesView.YMax);
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

        private readonly ICodeEditorView customFunctionsEditor;

        private void ChartAreaValuesView1_AddClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_expressionTextProvider.Text))
            {
                _errorHandler.DisplayError(Strings.GUI_addToChartButton_Click_Expression_should_not_be_empty_,
                    Strings.GUI_numericalOperationButton_Click_Warning_);
                return;
            }

            if (!AreChartValuesValid)
            {
                _errorHandler.DisplayError(
                    $"{Strings.Min_value_needs_to_be_less_than_Max_value}{Environment.NewLine}{Strings.CheckChartAreaValues}: x0, xN, y0, yN.",
                    Strings.GUI_numericalOperationButton_Click_Warning_);
                return;
            }
            try
            {
                customFunctionsEditor.ClearHighlightedErrors();
                CurrentChart.AddFunction(_expressionsEvaluator.Evaluate(_expressionTextProvider.Text,
                    customFunctionsEditor.Text,
                    _calculationsMode));
            }
            catch (Exception ex)
            {
                _exceptionsHandler.HandleException(ex);
            }
        }

        private bool AreChartValuesValid
            => ChartAreaValuesValidator.IsValid(_view.ChartAreaValuesView.XMin, _view.ChartAreaValuesView.XMax,
                _view.ChartAreaValuesView.YMin, _view.ChartAreaValuesView.YMax);

        private readonly IExceptionsHandler _exceptionsHandler;

        private void ChartAreaValuesView1_ClearClicked(object sender, EventArgs e)
        {
            foreach (var chart in _view.Charts)
                chart.Value.ClearAll();
        }

        private void ChartAreaValuesView_YMaxChanged(object sender, EventArgs e)
        {
            if (!AreChartValuesValid)
                return;
            foreach (var chart in _view.Charts)
                chart.Value.YMax = _view.ChartAreaValuesView.YMax;
        }

        private void ChartAreaValuesView_YMinChanged(object sender, EventArgs e)
        {
            if (!AreChartValuesValid)
                return;
            foreach (var chart in _view.Charts)
                chart.Value.YMin = _view.ChartAreaValuesView.YMin;
        }

        private void ChartAreaValuesView_XMaxChanged(object sender, EventArgs e)
        {
            if (!AreChartValuesValid)
                return;
            foreach (var chart in _view.Charts)
                chart.Value.XMax = _view.ChartAreaValuesView.XMax;
        }

        private void ChartAreaValuesView_XMinChanged(object sender, EventArgs e)
        {
            if (!AreChartValuesValid)
                return;
            foreach (var chart in _view.Charts)
                chart.Value.XMin = _view.ChartAreaValuesView.XMin;
        }

        private void ChartAreaValuesView_QualityChanged(object sender, EventArgs e)
        {
            if (!AreChartValuesValid)
                return;
            foreach (var chart in _view.Charts)
                chart.Value.Quality = _view.ChartAreaValuesView.Quality;
        }
    }
}