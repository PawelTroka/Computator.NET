using System.Collections.Generic;
using Computator.NET.Core.Abstract;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Model;

namespace Computator.NET.Core.Menu.Commands.TopMenuCommands
{
    public class FunctionsCommand : FunctionDetailsBasedCommand
    {
        public FunctionsCommand(ITextProvider expressionTextProvider, IScriptProvider scriptingTextProvider, IScriptProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IFunctionsDetails functionsDetails, IClickedMouseButtonsProvider clickedMouseButtonsProvider, IShowFunctionDetails showFunctionDetails) : base(MenuStrings.functionsToolStripMenuItem_Text,
            new Dictionary<string, string>()
            {
                {"ElementaryFunctions",MenuStrings.elementaryFunctionsToolStripMenuItem_Text },
                {"SpecialFunctions", MenuStrings.specialFunctionsToolStripMenuItem_Text}
            },
            expressionTextProvider, scriptingTextProvider, customFunctionsTextProvider, sharedViewState, functionsDetails, clickedMouseButtonsProvider, showFunctionDetails)
        {
        }
    }
}