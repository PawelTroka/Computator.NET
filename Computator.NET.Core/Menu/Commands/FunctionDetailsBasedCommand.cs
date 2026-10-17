using System;
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
                    ChildrenCommands = BuildFunctionsOrConstantsCommands(keyValue.Key, keyValue.Value, expressionTextProvider,
                        scriptingTextProvider, customFunctionsTextProvider, sharedViewState, autocompleteProvider, mouseButtonsProvider, showFunctionDetails)
                });
            }

            ChildrenCommands = childrenCommands;
        }

        private static List<IToolbarCommand> BuildFunctionsOrConstantsCommands(string key, string parentText, ITextProvider expressionTextProvider, IScriptProvider scriptingTextProvider, IScriptProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IAutocompleteProvider autocompleteProvider, IClickedMouseButtonsProvider mouseButtonsProvider, IShowFunctionDetails showFunctionDetails)
        {
            var commands = new List<IToolbarCommand>();
            var categories = new Dictionary<string, List<IToolbarCommand>>(StringComparer.Ordinal);

            foreach (var f in autocompleteProvider.ExpressionAutocompleteItems)
            {
                if (f.Details.IsNullOrEmpty() || f.Details.Type != key)
                    continue;

                var functionCommand = new FunctionOrConstantCommand(f, expressionTextProvider,
                    scriptingTextProvider, customFunctionsTextProvider, sharedViewState, mouseButtonsProvider, showFunctionDetails);

                // A runtime type or its caption is a fallback, not a subject subgroup.
                // Keep these entries accessible without modifying shared reference data.
                if (IsRedundantCategory(f.Details.Category, key, parentText))
                {
                    commands.Add(functionCommand);
                    continue;
                }

                var category = f.Details.Category.Trim();
                if (!categories.TryGetValue(category, out var categoryCommands))
                {
                    categoryCommands = new List<IToolbarCommand>();
                    categories.Add(category, categoryCommands);
                    commands.Add(new DummyCommand(category) { ChildrenCommands = categoryCommands });
                }

                categoryCommands.Add(functionCommand);
            }

            return commands;
        }

        private static bool IsRedundantCategory(string category, string key, string parentText)
        {
            if (string.IsNullOrWhiteSpace(category))
                return true;

            var normalizedCategory = NormalizeMenuLabel(category);
            return string.Equals(normalizedCategory, NormalizeMenuLabel(key), StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(normalizedCategory, NormalizeMenuLabel(parentText), StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeMenuLabel(string label)
        {
            return new string((label ?? string.Empty).Where(character => !char.IsWhiteSpace(character) && character != '&').ToArray());
        }
    }
}
