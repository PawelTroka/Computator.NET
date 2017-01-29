using System.ComponentModel;

namespace Computator.NET.DataTypes.SettingsTypes
{
    [Category("Scripting")]
    public enum CodeEditorType
    {
        Scintilla,
#if !__MonoCS__
        AvalonEdit,
#endif
        TextEditor,
    }
}