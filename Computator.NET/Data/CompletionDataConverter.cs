using System.Collections.Generic;
using Computator.NET.UI.Controls.AutocompleteMenu;
using Computator.NET.UI.Controls.CodeEditors.AvalonEdit;
using System.Linq;

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