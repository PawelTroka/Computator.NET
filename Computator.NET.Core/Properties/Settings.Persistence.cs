using System;
using System.Globalization;
using System.IO;
using Computator.NET.Core.Configuration;
using Computator.NET.DataTypes.Configuration;

namespace Computator.NET.Core.Properties
{
    public partial class Settings
    {
        private bool _canPersist = true;

        public void Save()
        {
            if (!_canPersist)
                throw new InvalidOperationException("Settings could not be read safely. The original files were preserved. Back them up and explicitly reset settings before saving.");
            SettingsPersistence.Save(AppInformation.SettingsPath, Capture());
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }

        private static Settings Load()
        {
            var settings = new Settings();
            try
            {
                var document = SettingsPersistence.Load(AppInformation.SettingsPath);
                if (document != null)
                {
                    settings.Apply(document);
                    settings.RestoreDirectories();
                    // A legacy import writes a separate JSON file. settings.dat is
                    // never overwritten or deleted, including on import failure.
                    if (!File.Exists(SettingsPersistence.JsonPath(AppInformation.SettingsPath)))
                        SettingsPersistence.Save(AppInformation.SettingsPath, document);
                }
            }
            catch (Exception exception)
            {
                settings._canPersist = false;
                Logger.Error(exception, "Settings could not be loaded; original files are preserved and automatic overwriting is disabled.");
            }
            return settings;
        }

        internal SettingsDocument Capture() => new SettingsDocument
        {
            Language = Language.Name,
            CodeEditor = CodeEditor,
            FunctionsOrder = FunctionsOrder,
            TooltipType = TooltipType,
            ExpressionFont = FontDocument.FromFont(ExpressionFont),
            ScriptingFont = FontDocument.FromFont(ScriptingFont),
            ShowReturnTypeInExpression = ShowReturnTypeInExpression,
            ShowParametersTypeInExpression = ShowParametersTypeInExpression,
            ShowReturnTypeInScripting = ShowReturnTypeInScripting,
            ShowParametersTypeInScripting = ShowParametersTypeInScripting,
            NumericalOutputNotation = NumericalOutputNotation,
            ScriptingDirectory = ScriptingDirectory,
            CustomFunctionsDirectory = CustomFunctionsDirectory,
            CalculationsErrors = CalculationsErrors,
            WorkingDirectory = WorkingDirectory
        };

        private void Apply(SettingsDocument document)
        {
            document.Validate();
            // Validate/create both font objects before changing any stored field.
            var expressionFont = document.ExpressionFont.ToFont();
            System.Drawing.Font scriptingFont;
            try { scriptingFont = document.ScriptingFont.ToFont(); }
            catch { expressionFont.Dispose(); throw; }
            _language = CultureInfo.GetCultureInfo(document.Language);
            _codeEditor = document.CodeEditor;
            _functionsOrder = document.FunctionsOrder;
            _tooltipType = document.TooltipType;
            _expressionFont = expressionFont;
            _scriptingFont = scriptingFont;
            _showReturnTypeInExpression = document.ShowReturnTypeInExpression;
            _showParametersTypeInExpression = document.ShowParametersTypeInExpression;
            _showReturnTypeInScripting = document.ShowReturnTypeInScripting;
            _showParametersTypeInScripting = document.ShowParametersTypeInScripting;
            _numericalOutputNotation = document.NumericalOutputNotation;
            _scriptingDirectory = document.ScriptingDirectory;
            _customFunctionsDirectory = document.CustomFunctionsDirectory;
            _calculationsErrors = document.CalculationsErrors;
            _workingDirectory = document.WorkingDirectory;
        }
    }
}
