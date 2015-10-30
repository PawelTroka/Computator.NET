namespace AutocompleteMenuNS
{
    /// <summary>
    ///     Control for displaying menu items, hosted in AutocompleteMenu.
    /// </summary>
    public interface IAutocompleteListView
    {
        /// <summary>
        ///     Image list
        /// </summary>
        System.Windows.Forms.ImageList ImageList { get; set; }

        /// <summary>
        ///     Index of current selected item
        /// </summary>
        int SelectedItemIndex { get; set; }

        /// <summary>
        ///     List of visible elements
        /// </summary>
        System.Collections.Generic.IList<AutocompleteItem> VisibleItems { get; set; }

        /// <summary>
        ///     Duration (ms) of tooltip showing
        /// </summary>
        int ToolTipDuration { get; set; }

        /// <summary>
        ///     Occurs when user selected item for inserting into text
        /// </summary>
        event System.EventHandler ItemSelected;

        /// <summary>
        ///     Occurs when current hovered item is changing
        /// </summary>
        event System.EventHandler<HoveredEventArgs> ItemHovered;

        /// <summary>
        ///     Shows tooltip
        /// </summary>
        /// <param name="autocompleteItem"></param>
        /// <param name="control"></param>
        void ShowToolTip(AutocompleteItem autocompleteItem, System.Windows.Forms.Control control = null);

        /// <summary>
        ///     Returns rectangle of item
        /// </summary>
        System.Drawing.Rectangle GetItemRectangle(int itemIndex);
    }
}