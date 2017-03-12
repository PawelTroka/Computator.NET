using System.ComponentModel;

namespace Computator.NET.DataTypes.SettingsTypes
{
    public enum CodeEditorType
    {
#if !__MonoCS__
        Scintilla,
        AvalonEdit,
#endif
        TextEditor,
    }
}