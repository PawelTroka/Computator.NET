using System.Collections.Generic;
using Computator.NET.Core.Abstract;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Model;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.TopMenuCommands
{
    public class ConstantsCommand : FunctionDetailsBasedCommand
    {
        public ConstantsCommand(ITextProvider expressionTextProvider, IScriptProvider scriptingTextProvider, IScriptProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IAutocompleteProvider autocompleteProvider, IClickedMouseButtonsProvider clickedMouseButtonsProvider, IShowFunctionDetails showFunctionDetails) : base(MenuStrings.constantsToolStripMenuItem_Text,
            new Dictionary<string, string>()
            {
                {"MathematicalConstants",MenuStrings.mathematicalConstantsToolStripMenuItem_Text },
                {"PhysicalConstants", MenuStrings.physicalConstantsToolStripMenuItem_Text}
            },
            expressionTextProvider, scriptingTextProvider, customFunctionsTextProvider, sharedViewState, autocompleteProvider, clickedMouseButtonsProvider, showFunctionDetails)
        {
        }
    }
}