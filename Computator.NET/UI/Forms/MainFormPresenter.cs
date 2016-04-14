#define PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Computator.NET.Charting;
using Computator.NET.Compilation;
using Computator.NET.Config;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.Localization;
using Computator.NET.NumericalCalculations;
using Computator.NET.Properties;

namespace Computator.NET
{



    public class MainFormPresenter 
    {
        private readonly ExpressionsEvaluator expressionsEvaluator = new ExpressionsEvaluator();

        private readonly ModeDeterminer modeDeterminer = new ModeDeterminer();

        private CalculationsMode _calculationsMode = CalculationsMode.Fxy;

        private readonly IMainForm _view;

        private IErrorHandler _errorHandler;

        private readonly List<Action<object, EventArgs>> defaultActions;


        public MainFormPresenter(IMainForm view, IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;

               defaultActions = new List<Action<object, EventArgs>>
            {
                ChartAreaValuesView1_AddClicked,
                //CalculationsView_CalculateClicked,
                //  NumericalCalculationsView_ComputeClicked,
                // symbolicOperationButton_Click,
                //  processButton_Click
            };


            _view = view;
            _view.EnterClicked += (o, e) => defaultActions[_view.SelectedViewIndex].Invoke(o, e);

            _view.chartAreaValuesView1.QualityChanged += ChartAreaValuesView_QualityChanged;

            _view.chartAreaValuesView1.XMinChanged += ChartAreaValuesView_XMinChanged;
            _view.chartAreaValuesView1.XMaxChanged += ChartAreaValuesView_XMaxChanged;
            _view.chartAreaValuesView1.YMinChanged += ChartAreaValuesView_YMinChanged;
            _view.chartAreaValuesView1.YMaxChanged += ChartAreaValuesView_YMaxChanged;

            foreach (var chart in _view.charts)
            {
                chart.Value.XMinChanged += (o, e) => _view.chartAreaValuesView1.XMin = chart.Value.XMin;
                chart.Value.XMaxChanged += (o, e) => _view.chartAreaValuesView1.XMax = chart.Value.XMax;
                chart.Value.YMinChanged += (o, e) => _view.chartAreaValuesView1.YMin = chart.Value.YMin;
                chart.Value.YMaxChanged += (o, e) => _view.chartAreaValuesView1.YMax = chart.Value.YMax;
            }

            _view.chartAreaValuesView1.AddClicked += ChartAreaValuesView1_AddClicked;
            _view.chartAreaValuesView1.ClearClicked += ChartAreaValuesView1_ClearClicked;

            foreach (var chart in _view.charts)
            {
                chart.Value.SetChartAreaValues(_view.chartAreaValuesView1.XMin, _view.chartAreaValuesView1.XMax,
                    _view.chartAreaValuesView1.YMin, _view.chartAreaValuesView1.YMax);
            }
            _view.ExpressionView.TextChanged += ExpressionView_TextChanged;


            _view.PrintClicked += _view_PrintClicked;
            _view.PrintPreviewClicked += _view_PrintPreviewClicked;


            _view.ModeForcedToReal += (sender, args) => SetMode(CalculationsMode.Real);
            _view.ModeForcedToComplex += (sender, args) => SetMode(CalculationsMode.Complex);
            _view.ModeForcedToFxy += (sender, args) => SetMode(CalculationsMode.Fxy);


            _view.SetLanguages(new object[]
            {
                new CultureInfo("en").NativeName,

                new CultureInfo("pl").NativeName,
                new CultureInfo("de").NativeName,
                new CultureInfo("cs").NativeName
            });
            _view.SelectedLanguageChanged += _view_SelectedLanguageChanged;


            _view.SelectedLanguage =
                AllCultures.First(c =>
                    c.TwoLetterISOLanguageName ==
                    Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName)
                    .NativeName;

            Settings.Default.PropertyChanged += (o, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Language":
                        Thread.CurrentThread.CurrentCulture = Settings.Default.Language;
                        LocalizationManager.GlobalUICulture = Settings.Default.Language;
                        break;
                }
            };

            EventAggregator.Instance.Subscribe<ChangeViewEvent>((cv) =>
            {
                _view.SelectedViewIndex = (int) cv.View;
            });



            _view.StatusText = GlobalConfig.version;
        }

        private void _view_SelectedLanguageChanged(object sender, EventArgs e)
        {
            var selectedCulture = AllCultures.First(c => c.NativeName == _view.SelectedLanguage);
            Thread.CurrentThread.CurrentCulture = selectedCulture;
            LocalizationManager.GlobalUICulture = selectedCulture;
            Settings.Default.Language = selectedCulture;
            Settings.Default.Save();
        }

        private readonly CultureInfo[] AllCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);


        private IChart currentChart => _view.charts[_calculationsMode];


     

        private void ChartAreaValuesView1_AddClicked(object sender, EventArgs e)
        {
            if (_view.ExpressionView.Text != "")
            {
                try
                {
                    currentChart.AddFunction(expressionsEvaluator.Evaluate(_view.ExpressionView.Text, 
                        _view.CustomFunctionsView.CustomFunctionsEditor.Text,
                        _calculationsMode));
                }
                catch (Exception ex)
                {
                   ExceptionsHandler.Instance.HandleException(ex,_errorHandler);
                }
            }
            else
                _errorHandler.DispalyError(Strings.GUI_addToChartButton_Click_Expression_should_not_be_empty_, Strings.GUI_numericalOperationButton_Click_Warning_);
        }

        private void _view_PrintPreviewClicked(object sender, EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (_view.SelectedViewIndex)
            {
                case 0:
                    currentChart.PrintPreview();
                    /*if (_calculationsMode == CalculationsMode.Real)
                        chart2d.Printing.PrintPreview();
                    else
                        SendStringAsKey("^P");*/
                    break;

                case 4:
                    //scriptingCodeEditor();
                    break;

                case 5:
                    //this.customFunctionsCodeEditor
                    break;

                default:
                    _view.SendStringAsKey("^P"); //this.chart2d.Printing.PrintPreview();
                    break;
            }
#else
            SendStringAsKey("^P");
#endif
        }

        private void _view_PrintClicked(object sender, EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (_view.SelectedViewIndex)
            {
                case 0:
                    //if (_calculationsMode == CalculationsMode.Real)
                    currentChart.Print();
                    // else
                    //  SendStringAsKey("^P");
                    break;

                case 4:
                    //scriptingCodeEditor();

                    break;

                case 5:
                    //this.customFunctionsCodeEditor
                    break;

                default:
                    _view.SendStringAsKey("^P"); //this.chart2d.Printing.PrintPreview();
                    break;
            }
#else
            SendStringAsKey("^P");
#endif
        }

        private void ExpressionView_TextChanged(object sender, EventArgs e)
        {
            var mode = modeDeterminer.DetermineMode(_view.ExpressionView.Text);
            if (mode == _calculationsMode) return;

            SetMode(mode);
        }


        private void SetMode(CalculationsMode mode)
        {
            foreach (var chart in _view.charts)
                chart.Value.Visible = chart.Key == mode;

            switch (mode)
            {
                case CalculationsMode.Complex:
                    _view.ModeText = "Mode[Complex : f(z)]";
                    break;
                case CalculationsMode.Fxy:
                    _view.ModeText = "Mode[3D : f(x,y)]";
                    break;
                case CalculationsMode.Real:
                    _view.ModeText = "Mode[Real : f(x)]";
                    break;
            }

            _calculationsMode = mode;
            //  _view.EditChartMenus.SetMode(_calculationsMode);

            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(mode));

//            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>((mode) => SetMode(mode.CalculationsMode));
        }


        private void ChartAreaValuesView1_ClearClicked(object sender, EventArgs e)
        {
            foreach (var chart in _view.charts)
            {
                chart.Value.ClearAll();
            }
        }

        private void ChartAreaValuesView_YMaxChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.charts)
            {
                chart.Value.YMax = _view.chartAreaValuesView1.YMax;
            }
        }

        private void ChartAreaValuesView_YMinChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.charts)
            {
                chart.Value.YMin = _view.chartAreaValuesView1.YMin;
            }
        }

        private void ChartAreaValuesView_XMaxChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.charts)
            {
                chart.Value.XMax = _view.chartAreaValuesView1.XMax;
            }
        }

        private void ChartAreaValuesView_XMinChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.charts)
            {
                chart.Value.XMin = _view.chartAreaValuesView1.XMin;
            }
        }

        private void ChartAreaValuesView_QualityChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.charts)
            {
                chart.Value.Quality = _view.chartAreaValuesView1.Quality;
            }
        }
    }
}