using System.Collections.Generic;
using Computator.NET.Data;
using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI.Menus
{
    public class FunctionsCommand : FunctionDetailsBasedCommand
    {
        public FunctionsCommand(ITextProvider expressionTextProvider, IScriptProvider scriptingTextProvider, IScriptProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IFunctionsDetails functionsDetails) : base(MenuStrings.functionsToolStripMenuItem_Text,
            new Dictionary<string, string>()
            {
                {"ElementaryFunctions",MenuStrings.elementaryFunctionsToolStripMenuItem_Text },
                {"SpecialFunctions", MenuStrings.specialFunctionsToolStripMenuItem_Text}
            },
            expressionTextProvider, scriptingTextProvider, customFunctionsTextProvider, sharedViewState, functionsDetails)
        {
        }
    }
}