using System.ComponentModel;

namespace Computator.NET.DataTypes.SettingsTypes
{
    [Category("Scripting")]
    public enum CodeEditorType
    {
#if !__MonoCS__
        Scintilla,
        AvalonEdit,
#endif
        TextEditor,
    }
}