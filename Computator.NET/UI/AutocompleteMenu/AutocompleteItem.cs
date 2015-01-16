using System;
using System.Drawing;
using Computator.NET.Functions;

namespace AutocompleteMenuNS
{
    /// <summary>
    ///     Item of autocomplete menu
    /// </summary>
    public class AutocompleteItem
    {
        public object Tag;
        public FunctionInfo functionInfo;
        private string menuText;
        private string toolTipText;
        private string toolTipTitle;

        public AutocompleteItem()
        {
            functionInfo = new FunctionInfo();
            ImageIndex = -1;
        }

        public AutocompleteItem(string text) : this()
        {
            functionInfo = new FunctionInfo();
            Text = text;
        }

        public AutocompleteItem(string text, int imageIndex)
            : this(text)
        {
            functionInfo = new FunctionInfo();
            ImageIndex = imageIndex;
        }

        public AutocompleteItem(string text, int imageIndex, string menuText)
            : this(text, imageIndex)
        {
            functionInfo = new FunctionInfo();
            this.menuText = menuText;
        }

        public AutocompleteItem(string text, int imageIndex, string menuText, string toolTipTitle, string toolTipText)
            : this(text, imageIndex, menuText)
        {
            functionInfo = new FunctionInfo();
            this.toolTipTitle = toolTipTitle;
            this.toolTipText = toolTipText;
        }

        /// <summary>
        ///     Parent AutocompleteMenu
        /// </summary>
        public AutocompleteMenu Parent { get; internal set; }

        /// <summary>
        ///     Text for inserting into textbox
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///     Image index for this item
        /// </summary>
        public int ImageIndex { get; set; }

        /// <summary>
        ///     Title for tooltip.
        /// </summary>
        /// <remarks>Return null for disable tooltip for this item</remarks>
        public virtual string ToolTipTitle
        {
            get { return toolTipTitle; }
            set { toolTipTitle = value; }
        }

        /// <summary>
        ///     Tooltip text.
        /// </summary>
        /// <remarks>For display tooltip text, ToolTipTitle must be not null</remarks>
        public virtual string ToolTipText
        {
            get { return toolTipText; }
            set { toolTipText = value; }
        }

        /// <summary>
        ///     Menu text. This text is displayed in the drop-down menu.
        /// </summary>
        public virtual string MenuText
        {
            get { return menuText; }
            set { menuText = value; }
        }


        /// <summary>
        ///     Returns text for inserting into Textbox
        /// </summary>
        public virtual string GetTextForReplace()
        {
            return Text;
        }

        /// <summary>
        ///     Compares fragment text with this item
        /// </summary>
        public virtual CompareResult Compare(string fragmentText)
        {
            if (Text.StartsWith(fragmentText, StringComparison.InvariantCultureIgnoreCase) &&
                Text != fragmentText)
                return CompareResult.VisibleAndSelected;

            return CompareResult.Hidden;
        }

        /// <summary>
        ///     Returns text for display into popup menu
        /// </summary>
        public override string ToString()
        {
            return menuText ?? Text;
        }

        /// <summary>
        ///     This method is called after item was inserted into text
        /// </summary>
        public virtual void OnSelected(SelectedEventArgs e)
        {
        }

        public virtual void OnPaint(PaintItemEventArgs e)
        {
            e.Graphics.DrawString(ToString(), e.Font, Brushes.Black, e.TextRect, e.StringFormat);
        }
    }

    public enum CompareResult
    {
        /// <summary>
        ///     Item do not appears
        /// </summary>
        Hidden,

        /// <summary>
        ///     Item appears
        /// </summary>
        Visible,

        /// <summary>
        ///     Item appears and will selected
        /// </summary>
        VisibleAndSelected
    }
}