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

namespace Computator.NET.Core.Properties
{
    [Serializable]
    public class Settings : INotifyPropertyChanged
    {
        private static readonly SimpleLogger.SimpleLogger Logger = new SimpleLogger.SimpleLogger(AppInformation.Name)
        {
            ClassName = "Settings"
        };
        private static readonly string ScriptingRawDir = Path.Combine("TSL Examples", "_Scripts");
        private static readonly string CustomFunctionsRawDir = Path.Combine("TSL Examples", "_CustomFunctions");

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
            if (File.Exists(AppInformation.SettingsPath))
            {
                var fs = new FileStream(AppInformation.SettingsPath, FileMode.Open);
                try
                {
                    var settings = (Settings)new BinaryFormatter().Deserialize(fs);
                    return settings;
                }
                catch (Exception exception)
                {
                    Logger.Log("Loading settings failed. Will remove corrupted settings file.", ErrorType.General, exception);
                    File.Delete(AppInformation.SettingsPath);
                }
            }

            return new Settings();
        }

        public void Reset()
        {
            Language = new CultureInfo("en");
            CodeEditor = RuntimeInformation.IsUnix
                ? CodeEditorType.TextEditor
                : CodeEditorType.Scintilla;
            FunctionsOrder = FunctionsOrder.Default;
            TooltipType = TooltipType.Default;
            ExpressionFont = new Font("Cambria", 15.75F, GraphicsUnit.Point);
            ScriptingFont = new Font("Consolas", 12, GraphicsUnit.Point);
            ShowParametersTypeInExpression = true;
            ShowReturnTypeInScripting = true;
            ShowParametersTypeInScripting = true;
            NumericalOutputNotation = NumericalOutputNotationType.MathematicalNotation;
            ScriptingDirectory = ScriptingRawDir;
            CustomFunctionsDirectory = CustomFunctionsRawDir;
            CalculationsErrors = CalculationsErrors.ReturnNAN;
        }

        private Settings()
        {
            if (!File.Exists(AppInformation.SettingsPath))
                Reset();

            MakeScriptingDirectoriesInMyDocumentsIfNeeded();
            RestoreScriptingExamplesIfNeeded();
        }



        private void RestoreScriptingExamplesIfNeeded()
        {
            if (ScriptingDirectory.Contains(ScriptingRawDir) && !Directory.Exists(ScriptingDirectory))
                if (Directory.Exists(PathUtility.GetFullPath(ScriptingRawDir)))
                    CopyDirectory.Copy(PathUtility.GetFullPath(ScriptingRawDir), ScriptingDirectory);
                else
                    throw new FileNotFoundException(
                        $"Scripting examples not found in {PathUtility.GetFullPath(ScriptingRawDir)}");

            if (CustomFunctionsDirectory.Contains(CustomFunctionsRawDir) &&
                !Directory.Exists(CustomFunctionsDirectory))
                if (Directory.Exists(PathUtility.GetFullPath(CustomFunctionsRawDir)))
                    CopyDirectory.Copy(PathUtility.GetFullPath(CustomFunctionsRawDir), CustomFunctionsDirectory);
                else
                    throw new FileNotFoundException(
                        $"Custom functions examples not found in {PathUtility.GetFullPath(CustomFunctionsRawDir)}");
        }

        private void MakeScriptingDirectoriesInMyDocumentsIfNeeded()
        {
            if (ScriptingDirectory == ScriptingRawDir)
                ScriptingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    ScriptingRawDir);

            if (CustomFunctionsDirectory == CustomFunctionsRawDir)
                CustomFunctionsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    CustomFunctionsRawDir);
        }

        public static Settings Default { get; } = Load();

        [Category("General")]
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

        [Category("Scripting")]
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

        [Category("Autocomplete")]
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

        [Category("Autocomplete")]
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
        [Category("Expression")]
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
        [Category("Scripting")]
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

        [Category("Expression")]
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
        [Category("Expression")]
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
        [Category("Scripting")]
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
        [Category("Scripting")]
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

        [Category("General")]
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

        [Category("Scripting")]
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

        [Category("Scripting")]
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

        [Category("General")]
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
