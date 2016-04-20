#define PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Computator.NET.Config;
using Computator.NET.DataTypes;
using Computator.NET.Evaluation;
using Computator.NET.Localization;
using Computator.NET.Properties;
using Computator.NET.UI.MVP;
using Computator.NET.UI.MVP.Views;

namespace Computator.NET
{
    public class MainFormPresenter
    {
        private readonly CultureInfo[] _allCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);


        private readonly IMainForm _view;


        private CalculationsMode _calculationsMode;

        public MainFormPresenter(IMainForm view)
        {


            _view = view;
            _view.EnterClicked += (o, e) => SharedViewState.Instance.CurrentAction?.Invoke(o, e);

            var expressionViewPresenter = new ExpressionViewPresenter(_view.ExpressionView);


            _view.PrintClicked += _view_PrintClicked;
            _view.PrintPreviewClicked += _view_PrintPreviewClicked;


            _view.ModeForcedToReal += (sender, args) =>
            {
                //   SetMode(CalculationsMode.Real);
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Real));
            };
            _view.ModeForcedToComplex += (sender, args) =>
            {
                //  SetMode(CalculationsMode.Complex);
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Complex));
            };
            _view.ModeForcedToFxy += (sender, args) =>
            {
                //  SetMode(CalculationsMode.Fxy);
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Fxy));
            };

            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(mode => SetMode(mode.CalculationsMode));

            _view.SetLanguages(new object[]
            {
                new CultureInfo("en").NativeName,
                new CultureInfo("pl").NativeName,
                new CultureInfo("de").NativeName,
                new CultureInfo("cs").NativeName
            });
            _view.SelectedLanguageChanged += _view_SelectedLanguageChanged;


            _view.SelectedLanguage =
                _allCultures.First(c =>
                    c.TwoLetterISOLanguageName ==
                    Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName)
                    .NativeName;

            Settings.Default.PropertyChanged += (o, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Settings.Default.Language):
                        Thread.CurrentThread.CurrentCulture = Settings.Default.Language;
                        LocalizationManager.GlobalUICulture = Settings.Default.Language;
                        break;
                }
            };

            ///////EventAggregator.Instance.Subscribe<ChangeViewEvent>(cv => { _view.SelectedViewIndex = (int) cv.View; });

            SharedViewState.Instance.PropertyChanged += (o, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(SharedViewState.Instance.CurrentView):
                        _view.SelectedViewIndex = (int)SharedViewState.Instance.CurrentView;
                        break;
                }
            };

            _view.SelectedViewChanged += _view_SelectedViewChanged;

            _view.StatusText = GlobalConfig.version;
        }

        private void _view_SelectedViewChanged(object sender, EventArgs e)
        {
            SharedViewState.Instance.CurrentView = (ViewName) _view.SelectedViewIndex;
        }

        private void _view_SelectedLanguageChanged(object sender, EventArgs e)
        {
            var selectedCulture = _allCultures.First(c => c.NativeName == _view.SelectedLanguage);
            Thread.CurrentThread.CurrentCulture = selectedCulture;
            LocalizationManager.GlobalUICulture = selectedCulture;
            Settings.Default.Language = selectedCulture;
            Settings.Default.Save();
        }


        private void _view_PrintPreviewClicked(object sender, EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (_view.SelectedViewIndex)
            {
                case 0:
                    //TODO: MVP//      currentChart.PrintPreview();
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
                    //TODO: MVP//      currentChart.Print();
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


        private void SetMode(CalculationsMode mode)
        {
            if (mode == _calculationsMode)
                return;


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
        }
    }
}