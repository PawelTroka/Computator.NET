using System.Collections.Generic;
using System.Linq;
using Computator.NET.Controls.CodeEditors.AvalonEdit;
using Computator.NET.Core.Autocompletion;

namespace Computator.NET.Data
{
    public static class CompletionDataConverter
    {
        public static List<CompletionData>
            ConvertAutocompleteItemsToCompletionDatas(
                AutocompleteItem[] autocompleteItems)
        {
            return
                autocompleteItems.Select(autocompleteItem => ToCompletionData(autocompleteItem)).ToList();
        }

        public static CompletionData ToCompletionData(AutocompleteItem autocompleteItem)
        {
            return new CompletionData(autocompleteItem.Text, autocompleteItem.MenuText, autocompleteItem.Info, autocompleteItem.ImageIndex, autocompleteItem.sharedViewState);
        }
    }
}