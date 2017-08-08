using System.Collections.Generic;
using System.Linq;
using Computator.NET.Core.Abstract;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Model;

namespace Computator.NET.Core.Menu.Commands
{
    public class FunctionDetailsBasedCommand : DummyCommand
    {
        public FunctionDetailsBasedCommand(string name, Dictionary<string, string> KeyAndNameOfCommandCollection, ITextProvider expressionTextProvider, IScriptProvider scriptingTextProvider, IScriptProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IAutocompleteProvider autocompleteProvider, IClickedMouseButtonsProvider mouseButtonsProvider, IShowFunctionDetails showFunctionDetails) : base(name)
        {
            var childrenCommands = new List<IToolbarCommand>();

            foreach (var keyValue in KeyAndNameOfCommandCollection)
            {
                childrenCommands.Add(new DummyCommand(keyValue.Value)
                {
                    ChildrenCommands = BuildFunctionsOrConstantsCommands(keyValue.Key, expressionTextProvider,
                        scriptingTextProvider, customFunctionsTextProvider, sharedViewState, autocompleteProvider, mouseButtonsProvider, showFunctionDetails)
                });
            }

            ChildrenCommands = childrenCommands;

        }


        private static List<IToolbarCommand> BuildFunctionsOrConstantsCommands(string key, ITextProvider expressionTextProvider, IScriptProvider scriptingTextProvider, IScriptProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IAutocompleteProvider autocompleteProvider, IClickedMouseButtonsProvider mouseButtonsProvider, IShowFunctionDetails showFunctionDetails)
        {
            var dict = new Dictionary<string, IToolbarCommand>();
            
            foreach (var f in autocompleteProvider.ExpressionAutocompleteItems)
            {
                if (f.Details.IsNullOrEmpty() || f.Details.Type != key)
                    continue;

                //TODO: remove this code when made sure it does nothing:
                if (f.Details.Category == "")
                    f.Details.Category = "_empty_";


                if (!dict.ContainsKey(f.Details.Category))
                {
                    //var cat = new ToolStripMenuItem(f.Value.Category) { Name = f.Value.Category };
                    var command = new DummyCommand(f.Details.Category) { ChildrenCommands = new List<IToolbarCommand>() };
                    dict.Add(f.Details.Category, command);
                }
                (dict[f.Details.Category].ChildrenCommands as List<IToolbarCommand>).Add(
                    new FunctionOrConstantCommand(f, expressionTextProvider,
                        scriptingTextProvider, customFunctionsTextProvider, sharedViewState, mouseButtonsProvider, showFunctionDetails));
            }

            return dict.Values.ToList();
        }
    }
}