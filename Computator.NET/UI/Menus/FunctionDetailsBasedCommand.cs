using System.Collections.Generic;
using System.Linq;
using Computator.NET.Data;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.DummyCommands;

namespace Computator.NET.UI.Menus
{
    public class FunctionDetailsBasedCommand : DummyCommand
    {
        public FunctionDetailsBasedCommand(string name, Dictionary<string, string> KeyAndNameOfCommandCollection, ITextProvider expressionTextProvider, ITextProvider scriptingTextProvider, ITextProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IFunctionsDetails functionsDetails) : base(name)
        {
            var childrenCommands = new List<IToolbarCommand>();

            foreach (var keyValue in KeyAndNameOfCommandCollection)
            {
                childrenCommands.Add(new DummyCommand(keyValue.Value)
                {
                    ChildrenCommands = BuildFunctionsOrConstantsCommands(keyValue.Key, expressionTextProvider,
                        scriptingTextProvider, customFunctionsTextProvider, sharedViewState, functionsDetails)
                });
            }

            ChildrenCommands = childrenCommands;

        }


        private static List<IToolbarCommand> BuildFunctionsOrConstantsCommands(string key, ITextProvider expressionTextProvider, ITextProvider scriptingTextProvider, ITextProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IFunctionsDetails functionsDetails)
        {
            var dict = new Dictionary<string, IToolbarCommand>();

            var functions = functionsDetails.ToArray();


            foreach (var f in functions)
            {
                if (f.Value.Type != key)
                    continue;


                if (f.Value.Category == "")
                    f.Value.Category = "_empty_";

                if (!dict.ContainsKey(f.Value.Category))
                {
                    //var cat = new ToolStripMenuItem(f.Value.Category) { Name = f.Value.Category };
                    var command = new DummyCommand(f.Value.Category) { ChildrenCommands = new List<IToolbarCommand>() };
                    dict.Add(f.Value.Category, command);
                }
                (dict[f.Value.Category].ChildrenCommands as List<IToolbarCommand>).Add(
                    new FunctionOrConstantCommand(f.Value.Signature, f.Value.Title, expressionTextProvider,
                        scriptingTextProvider, customFunctionsTextProvider, sharedViewState, functionsDetails));
            }

            return dict.Values.ToList();
        }
    }
}