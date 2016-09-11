using System.Collections.Generic;
using Computator.NET.Data;
using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI.Menus
{
    public class ConstantsCommand : FunctionDetailsBasedCommand
    {
        public ConstantsCommand(ITextProvider expressionTextProvider, ITextProvider scriptingTextProvider, ITextProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IFunctionsDetails functionsDetails) : base(MenuStrings.constantsToolStripMenuItem_Text,
            new Dictionary<string, string>()
            {
                {"MathematicalConstants",MenuStrings.mathematicalConstantsToolStripMenuItem_Text },
                {"PhysicalConstants", MenuStrings.physicalConstantsToolStripMenuItem_Text}
            },
            expressionTextProvider, scriptingTextProvider, customFunctionsTextProvider, sharedViewState, functionsDetails)
        {
        }
    }
}