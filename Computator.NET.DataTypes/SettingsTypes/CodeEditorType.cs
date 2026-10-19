namespace Computator.NET.DataTypes.SettingsTypes
{
    public enum CodeEditorType
    {
#if !__MonoCS__
        Scintilla,
        AvalonEdit,
#endif
        // Preserve the modern settings identity in the reduced Mono enum too.
        TextEditor = 2,
    }
}
