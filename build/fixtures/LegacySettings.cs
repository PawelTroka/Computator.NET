// Test-only Framework writer for known data; never included in a shipped project.
// It never deserializes payloads or executes data supplied by a settings file.
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Computator.NET.DataTypes.SettingsTypes;
namespace Computator.NET.DataTypes.SettingsTypes
{
    public enum CalculationsErrors { ReturnNAN, ShowError }
    public enum CodeEditorType { Scintilla, AvalonEdit, TextEditor }
    public enum FunctionsOrder { Default, Alphabetical }
    public enum NumericalOutputNotationType { MathematicalNotation, EngineeringNotation }
    public enum TooltipType { Default, Form, None }
}
namespace Computator.NET.Core.Properties
{
    [Serializable]
    public sealed class Settings
    {
        private CalculationsErrors _calculationsErrors = CalculationsErrors.ShowError;
        private string _customFunctionsDirectory = @"C:\fixture\custom functions";
        private string _scriptingDirectory = @"C:\fixture\scripts";
        private NumericalOutputNotationType _numericalOutputNotation = NumericalOutputNotationType.EngineeringNotation;
        private bool _showParametersTypeInScripting = false;
        private bool _showReturnTypeInScripting = true;
        private bool _showParametersTypeInExpression = true;
        private bool _showReturnTypeInExpression = false;
        private Font _scriptingFont = new Font("Consolas", 14.5f, FontStyle.Italic, GraphicsUnit.Point);
        private Font _expressionFont = new Font("Cambria", 18.25f, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point);
        private TooltipType _tooltipType = TooltipType.Form;
        private FunctionsOrder _functionsOrder = FunctionsOrder.Alphabetical;
        private CodeEditorType _codeEditor = CodeEditorType.AvalonEdit;
        private CultureInfo _language = new CultureInfo("pl-PL");
        private string _workingDirectory = @"C:\fixture\workspace";
        public static void Write(string path)
        {
            using (var stream = File.Create(path)) new BinaryFormatter().Serialize(stream, new Settings());
        }
    }
}
