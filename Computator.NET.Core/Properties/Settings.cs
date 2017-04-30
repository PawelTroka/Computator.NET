using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using Computator.NET.Core.Annotations;
using Computator.NET.Core.Helpers;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.SettingsTypes;
using System.Linq;
using NLog;

namespace Computator.NET.Core.Properties
{
    [Serializable]
    public class Settings : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private CalculationsErrors _calculationsErrors;
        private string _customFunctionsDirectory;
        private string _scriptingDirectory;
        private NumericalOutputNotationType _numericalOutputNotation;
        private bool _showParametersTypeInScripting;
        private bool _showReturnTypeInScripting;
        private bool _showParametersTypeInExpression;
        private bool _showReturnTypeInExpression;
        private Font _scriptingFont;
        private Font _expressionFont;
        private TooltipType _tooltipType;
        private FunctionsOrder _functionsOrder;
        private CodeEditorType _codeEditor;
        private CultureInfo _language;
        private string _workingDirectory;

        private static readonly string ScriptingRawDir = Path.Combine("TSL Examples", "_Scripts");
        private static readonly string DefaultScriptingDirectory = Path.Combine(AppInformation.DataDirectory, ScriptingRawDir);

        private static readonly string CustomFunctionsRawDir = Path.Combine("TSL Examples", "_CustomFunctions");
        private static readonly string DefaultCustomFunctionsDirectory = Path.Combine(AppInformation.DataDirectory, CustomFunctionsRawDir);

        private static readonly string DefaultWorkingDirectory = Path.Combine(AppInformation.DataDirectory, "Workspace");

        public void Save()
        {
            using (var fs = new FileStream(AppInformation.SettingsPath, FileMode.Create))
            {
                new BinaryFormatter().Serialize(fs, this);
            }

            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }

        private static Settings Load()
        {
            if (!File.Exists(AppInformation.SettingsPath))
                return new Settings();
            try
            {
                using (var fs = new FileStream(AppInformation.SettingsPath, FileMode.Open))
                {
                    var settings = (Settings)new BinaryFormatter().Deserialize(fs);
                    settings.RestoreDirectories();
                    return settings;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Loading settings failed. Will remove corrupted settings file.");
                File.Delete(AppInformation.SettingsPath);
            }

            return new Settings();
        }

        public void Reset()
        {
            Language = new CultureInfo("en");

            CodeEditor = (CodeEditorType)Enum.GetValues(typeof(CodeEditorType)).GetValue(0);

            FunctionsOrder = FunctionsOrder.Default;
            TooltipType = TooltipType.Default;
            
            ExpressionFont = CustomFonts.GetMathFont(15.75F);//new Font("Cambria", 15.75F, GraphicsUnit.Point);
            ScriptingFont = CustomFonts.GetScriptingFont(12);//new Font("Consolas", 12, GraphicsUnit.Point);

            ShowReturnTypeInExpression = false;
            ShowParametersTypeInExpression = true;

            ShowReturnTypeInScripting = true;
            ShowParametersTypeInScripting = true;

            NumericalOutputNotation = NumericalOutputNotationType.MathematicalNotation;

            ScriptingDirectory = DefaultScriptingDirectory;
            CustomFunctionsDirectory = DefaultCustomFunctionsDirectory;

            CalculationsErrors = CalculationsErrors.ReturnNAN;

            WorkingDirectory = DefaultWorkingDirectory;
        }

        private Settings()
        {
            if (!File.Exists(AppInformation.SettingsPath))
                Reset();

            RestoreDirectories();
        }


        private void RestoreDirectories()
        {

            if (!Directory.Exists(DefaultScriptingDirectory) || !Directory.EnumerateFiles(DefaultScriptingDirectory).Any())
            {
                if (Directory.Exists(PathUtility.GetFullPath(ScriptingRawDir)))
                    CopyDirectory.Copy(PathUtility.GetFullPath(ScriptingRawDir), DefaultScriptingDirectory);
                else
                    throw new FileNotFoundException(
                        $"Scripting examples not found in {PathUtility.GetFullPath(ScriptingRawDir)}");
            }


            if (!Directory.Exists(DefaultCustomFunctionsDirectory) || !Directory.EnumerateFiles(DefaultCustomFunctionsDirectory).Any())
            {
                if (Directory.Exists(PathUtility.GetFullPath(CustomFunctionsRawDir)))
                    CopyDirectory.Copy(PathUtility.GetFullPath(CustomFunctionsRawDir), DefaultCustomFunctionsDirectory);
                else
                    throw new FileNotFoundException(
                        $"Custom functions examples not found in {PathUtility.GetFullPath(CustomFunctionsRawDir)}");
            }

            var dirsToRestore = new string[] { WorkingDirectory, ScriptingDirectory, CustomFunctionsDirectory };

            foreach (var dir in dirsToRestore)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }


        public static Settings Default { get; } = Load();


        #region General

        [DisplayName("Language")]
        [Category("General")]
        [Description("Language of the application used in whole app. Requires restart.")]
        public CultureInfo Language
        {
            get { return _language; }
            set
            {
                if (Equals(value, _language)) return;
                _language = value;
                OnPropertyChanged();
            }
        }

        [DisplayName("Numerical output notation")]
        [Category("General")]
        [Description("What notation should be used when formatting numbers as strings (in output)?")]
        public NumericalOutputNotationType NumericalOutputNotation
        {
            get { return _numericalOutputNotation; }
            set
            {
                if (value == _numericalOutputNotation) return;
                _numericalOutputNotation = value;
                OnPropertyChanged();
            }
        }

        [DisplayName("Calculations errors")]
        [Category("General")]
        [Description("How to treat calculations errors? Should it be soft error (return NaN) and continue calculations or hard error (throw exception and end calculations)?")]
        public CalculationsErrors CalculationsErrors
        {
            get { return _calculationsErrors; }
            set
            {
                if (value == _calculationsErrors) return;
                _calculationsErrors = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Autocomplete

        [DisplayName("Functions order")]
        [Category("Autocomplete")]
        [Description("Order of functions in autocomplete.")]
        public FunctionsOrder FunctionsOrder
        {
            get { return _functionsOrder; }
            set
            {
                if (value == _functionsOrder) return;
                _functionsOrder = value;
                OnPropertyChanged();
            }
        }

        [DisplayName("Tooltip type")]
        [Category("Autocomplete")]
        [Description("Type of tooltips with information about the function, shown in autocomplete. Different types have different looks and feel.")]
        public TooltipType TooltipType
        {
            get { return _tooltipType; }
            set
            {
                if (value == _tooltipType) return;
                _tooltipType = value;
                OnPropertyChanged();
            }
        }

        [DisplayName("Show return type in expression?")]
        [Category("Autocomplete")]
        [Description("Should expressions autocomplete show also what type function returns (eg. real or complex) ?")]
        public bool ShowReturnTypeInExpression
        {
            get { return _showReturnTypeInExpression; }
            set
            {
                if (value == _showReturnTypeInExpression) return;
                _showReturnTypeInExpression = value;
                OnPropertyChanged();
            }
        }

        [DisplayName("Show parameters type in expression?")]
        [Category("Autocomplete")]
        [Description("Should expressions autocomplete show also what types are functions arguments (eg. real or complex) ?")]
        public bool ShowParametersTypeInExpression
        {
            get { return _showParametersTypeInExpression; }
            set
            {
                if (value == _showParametersTypeInExpression) return;
                _showParametersTypeInExpression = value;
                OnPropertyChanged();
            }
        }

        [DisplayName("Show return type in scripting?")]
        [Category("Autocomplete")]
        [Description("Should scripting autocomplete show also what type function returns (eg. real or complex) ?")]
        public bool ShowReturnTypeInScripting
        {
            get { return _showReturnTypeInScripting; }
            set
            {
                if (value == _showReturnTypeInScripting) return;
                _showReturnTypeInScripting = value;
                OnPropertyChanged();
            }
        }

        [DisplayName("Show parameters type in scripting?")]
        [Category("Autocomplete")]
        [Description("Should scripting autocomplete show also what types are functions arguments (eg. real or complex) ?")]
        public bool ShowParametersTypeInScripting
        {
            get { return _showParametersTypeInScripting; }
            set
            {
                if (value == _showParametersTypeInScripting) return;
                _showParametersTypeInScripting = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Expression

        [DisplayName("Expression font")]
        [Category("Expression")]
        [Description("Font used in expressions.")]
        public Font ExpressionFont
        {
            get { return _expressionFont; }
            set
            {
                if (Equals(value, _expressionFont)) return;
                _expressionFont = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Scripting

        [DisplayName("Code editor")]
        [Category("Scripting")]
        [Description("Code editor used in scripting and when editing custom functions. Different editors have different functionality, look and feel.")]
        public CodeEditorType CodeEditor
        {
            get { return _codeEditor; }
            set
            {
                if (value == _codeEditor) return;
                _codeEditor = value;
                OnPropertyChanged();
            }
        }

        [DisplayName("Scripting font")]
        [Category("Scripting")]
        [Description("Font used in scripting and when editing custom functions.")]
        public Font ScriptingFont
        {
            get { return _scriptingFont; }
            set
            {
                if (Equals(value, _scriptingFont)) return;
                _scriptingFont = value;
                OnPropertyChanged();
            }
        }


        [DisplayName("Scripting directory")]
        [Category("Scripting")]
        [Description("Path to location where TSL scripts are stored and edited. Requires restart to take effect.")]
        public string ScriptingDirectory
        {
            get { return _scriptingDirectory; }
            set
            {
                if (value == _scriptingDirectory) return;
                _scriptingDirectory = value;
                OnPropertyChanged();
            }
        }

        [DisplayName("Custom functions directory")]
        [Category("Scripting")]
        [Description("Path to location where TSL custom functions are stored and edited. Requires restart to take effect.")]
        public string CustomFunctionsDirectory
        {
            get { return _customFunctionsDirectory; }
            set
            {
                if (value == _customFunctionsDirectory) return;
                _customFunctionsDirectory = value;
                OnPropertyChanged();
            }
        }

        [DisplayName("Working directory")]
        [Category("Scripting")]
        [Description("Path to directory where all relative paths during TSL script execution will point.")]
        public string WorkingDirectory
        {
            get { return _workingDirectory; }
            set
            {
                if (value == _workingDirectory) return;
                _workingDirectory = value;
                OnPropertyChanged();
            }
        }

        #endregion


        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        [field: NonSerialized]
        public event EventHandler SettingsSaved;
    }
}
