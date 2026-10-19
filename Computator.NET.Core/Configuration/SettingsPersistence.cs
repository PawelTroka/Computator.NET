using System;
using System.Drawing;
using System.Formats.Nrbf;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Computator.NET.DataTypes.SettingsTypes;

namespace Computator.NET.Core.Configuration
{
    /// <summary>Explicit data-only schema. No runtime type names or arbitrary object deserialization.</summary>
    public sealed class SettingsDocument
    {
        [JsonRequired] public int SchemaVersion { get; set; } = 1;
        public string Language { get; set; } = "en";
#if __MonoCS__
        public CodeEditorType CodeEditor { get; set; } = CodeEditorType.TextEditor;
#else
        public CodeEditorType CodeEditor { get; set; }
#endif
        public FunctionsOrder FunctionsOrder { get; set; }
        public TooltipType TooltipType { get; set; }
        public FontDocument ExpressionFont { get; set; } = new FontDocument { Name = "Cambria", Size = 15.75f };
        public FontDocument ScriptingFont { get; set; } = new FontDocument { Name = "Consolas", Size = 12 };
        public bool ShowReturnTypeInExpression { get; set; }
        public bool ShowParametersTypeInExpression { get; set; } = true;
        public bool ShowReturnTypeInScripting { get; set; } = true;
        public bool ShowParametersTypeInScripting { get; set; } = true;
        public NumericalOutputNotationType NumericalOutputNotation { get; set; }
        public string ScriptingDirectory { get; set; }
        public string CustomFunctionsDirectory { get; set; }
        public CalculationsErrors CalculationsErrors { get; set; }
        public string WorkingDirectory { get; set; }

        public void Validate()
        {
            if (SchemaVersion != 1) throw new InvalidDataException("Unsupported settings schema; refusing to overwrite it.");
            if (Language == null || Language.Length > 85) throw new InvalidDataException("Invalid settings language.");
            CultureInfo.GetCultureInfo(Language);
#if __MonoCS__
            // Existing Windows profiles may select Scintilla (0) or AvalonEdit
            // (1); historical Mono profiles also used 0 for TextEditor. Mono now
            // provides the established TextEditor only, whose persisted
            // value remains 2 so a profile can return to the Windows application.
            if ((int)CodeEditor == 0 || (int)CodeEditor == 1) CodeEditor = CodeEditorType.TextEditor;
#endif
            if (!Enum.IsDefined(typeof(CodeEditorType), CodeEditor) || !Enum.IsDefined(typeof(FunctionsOrder), FunctionsOrder) || !Enum.IsDefined(typeof(TooltipType), TooltipType) ||
                !Enum.IsDefined(typeof(NumericalOutputNotationType), NumericalOutputNotation) || !Enum.IsDefined(typeof(CalculationsErrors), CalculationsErrors))
                throw new InvalidDataException("Unknown settings enum value.");
            if (ExpressionFont == null || ScriptingFont == null) throw new InvalidDataException("Missing font settings.");
            ExpressionFont.Validate();
            ScriptingFont.Validate();
            foreach (var path in new[] { ScriptingDirectory, CustomFunctionsDirectory, WorkingDirectory })
            {
                if (string.IsNullOrWhiteSpace(path) || path.Length > 32760)
                    throw new InvalidDataException("Invalid settings directory.");
                Path.GetFullPath(path);
            }
        }
    }

    public sealed class FontDocument
    {
        public string Name { get; set; }
        public float Size { get; set; }
        public FontStyle Style { get; set; }
        public GraphicsUnit Unit { get; set; } = GraphicsUnit.Point;
        public byte GdiCharSet { get; set; } = 1;
        public bool GdiVerticalFont { get; set; }

        public static FontDocument FromFont(Font font) => new FontDocument
        {
            Name = font.OriginalFontName ?? font.Name, Size = font.Size, Style = font.Style,
            Unit = font.Unit, GdiCharSet = font.GdiCharSet, GdiVerticalFont = font.GdiVerticalFont
        };
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name) || Name.Length > 256 || float.IsNaN(Size) || float.IsInfinity(Size) || Size <= 0 || Size > 512 ||
                ((int)Style & ~15) != 0 || !Enum.IsDefined(typeof(GraphicsUnit), Unit) || Unit == GraphicsUnit.World)
                throw new InvalidDataException("Invalid font settings.");
        }
        public Font ToFont()
        {
            Validate();
            return new Font(Name, Size, Style, Unit, GdiCharSet, GdiVerticalFont);
        }
    }

    public static class SettingsPersistence
    {
        public const int MaximumBytes = 1024 * 1024;
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { WriteIndented = true, MaxDepth = 16 };
        public static string JsonPath(string legacyPath) => Path.ChangeExtension(legacyPath, ".json");

        public static SettingsDocument Load(string legacyPath)
        {
            var jsonPath = JsonPath(legacyPath);
            var path = File.Exists(jsonPath) ? jsonPath : legacyPath;
            if (!File.Exists(path)) return null;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length == 0 || stream.Length > MaximumBytes) throw new InvalidDataException("Settings file size is invalid.");
                var document = path == jsonPath ? JsonSerializer.Deserialize<SettingsDocument>(stream, Options) : ReadLegacy(stream);
                if (document == null) throw new InvalidDataException("Settings document is empty.");
                document.Validate();
                return document;
            }
        }

        public static void Save(string legacyPath, SettingsDocument document) => Write(legacyPath, document, true);

        /// <summary>
        /// Use only after an explicit user reset. The rejected JSON is preserved in
        /// an atomic replacement backup; the original binary file is never changed.
        /// </summary>
        public static void SaveAfterExplicitReset(string legacyPath, SettingsDocument document) => Write(legacyPath, document, false);

        private static void Write(string legacyPath, SettingsDocument document, bool validateExisting)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            document.Validate();
            var path = Path.GetFullPath(JsonPath(legacyPath));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            // Normal saves must not overwrite unreadable data or a newer schema.
            if (validateExisting && File.Exists(path)) Load(legacyPath);
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(stream, document, Options);
                    if (stream.Length > MaximumBytes) throw new InvalidDataException("Settings output is too large.");
                    stream.Flush(true);
                }
                if (File.Exists(path))
                    File.Replace(temporary, path, path + ".backup-" + Guid.NewGuid().ToString("N"));
                else
                    File.Move(temporary, path);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        public static SettingsDocument ReadLegacy(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanSeek || stream.Length - stream.Position > MaximumBytes)
                throw new InvalidDataException("Legacy settings must be a bounded seekable stream.");
            // NrbfDecoder reads records only: it never loads payload types, invokes
            // constructors, callbacks, ISerializable, or BinaryFormatter.Deserialize.
            var root = NrbfDecoder.DecodeClassRecord(stream);
            if (root.TypeName.FullName != "Computator.NET.Core.Properties.Settings")
                throw new InvalidDataException("Unexpected legacy settings root type.");
            var language = root.GetClassRecord("_language");
            if (language == null || language.TypeName.FullName != "System.Globalization.CultureInfo")
                throw new InvalidDataException("Unexpected legacy language type.");
            var result = new SettingsDocument
            {
                Language = language.GetString("m_name"),
                CodeEditor = (CodeEditorType)ReadEnum(root, "_codeEditor", typeof(CodeEditorType)),
                FunctionsOrder = (FunctionsOrder)ReadEnum(root, "_functionsOrder", typeof(FunctionsOrder)),
                TooltipType = (TooltipType)ReadEnum(root, "_tooltipType", typeof(TooltipType)),
                NumericalOutputNotation = (NumericalOutputNotationType)ReadEnum(root, "_numericalOutputNotation", typeof(NumericalOutputNotationType)),
                CalculationsErrors = (CalculationsErrors)ReadEnum(root, "_calculationsErrors", typeof(CalculationsErrors)),
                ExpressionFont = ReadFont(root.GetClassRecord("_expressionFont")),
                ScriptingFont = ReadFont(root.GetClassRecord("_scriptingFont")),
                ShowReturnTypeInExpression = root.GetBoolean("_showReturnTypeInExpression"),
                ShowParametersTypeInExpression = root.GetBoolean("_showParametersTypeInExpression"),
                ShowReturnTypeInScripting = root.GetBoolean("_showReturnTypeInScripting"),
                ShowParametersTypeInScripting = root.GetBoolean("_showParametersTypeInScripting"),
                ScriptingDirectory = root.GetString("_scriptingDirectory"),
                CustomFunctionsDirectory = root.GetString("_customFunctionsDirectory"),
                WorkingDirectory = root.GetString("_workingDirectory")
            };
            result.Validate();
            return result;
        }

        private static int ReadEnum(ClassRecord parent, string name, Type expected)
        {
            var record = parent.GetClassRecord(name);
            if (record == null || !record.TypeNameMatches(expected)) throw new InvalidDataException("Unexpected legacy enum type: " + name);
            return record.GetInt32("value__");
        }

        private static int FontNumber(ClassRecord record, string name, Type expected)
        {
            var value = record.GetRawValue(name);
            if (value is int number) return number;
            if (value is ClassRecord enumRecord && enumRecord.TypeNameMatches(expected)) return enumRecord.GetInt32("value__");
            throw new InvalidDataException("Unexpected legacy font value: " + name);
        }

        private static FontDocument ReadFont(ClassRecord font)
        {
            if (font == null || font.TypeName.FullName != "System.Drawing.Font")
                throw new InvalidDataException("Unexpected legacy font type.");
            return new FontDocument
            {
                Name = font.GetString("Name"), Size = font.GetSingle("Size"),
                Style = (FontStyle)FontNumber(font, "Style", typeof(FontStyle)),
                Unit = (GraphicsUnit)FontNumber(font, "Unit", typeof(GraphicsUnit)),
                GdiCharSet = font.HasMember("GdiCharSet") ? font.GetByte("GdiCharSet") : (byte)1,
                GdiVerticalFont = font.HasMember("GdiVerticalFont") && font.GetBoolean("GdiVerticalFont")
            };
        }
    }
}
