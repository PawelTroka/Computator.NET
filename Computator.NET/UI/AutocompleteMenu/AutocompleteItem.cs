using System;
using System.Collections.Generic;
using System.Drawing;
using Computator.NET;
using Computator.NET.Functions;
using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;

namespace AutocompleteMenuNS
{
    public class AutocompleteItemEqualityComparer : IEqualityComparer<AutocompleteItem>
    {
        public bool Equals(AutocompleteItem x, AutocompleteItem y)
        {
            return x.Text == y.Text;
        }

        public int GetHashCode(AutocompleteItem obj)
        {
            return obj.Text.GetHashCode();
        }
    }
    /// <summary>
    ///     Item of autocomplete menu
    /// </summary>
    public class AutocompleteItem
    {
        public FunctionInfo functionInfo;
        private string menuText;
        public object Tag;
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

        public CompletionData ToCompletionData()
        {
            return new CompletionData(this.Text, this.MenuText, this.functionInfo, this.ImageIndex);
        }

        public AutocompleteItem(string text, int imageIndex)
            : this(text)
        {
            functionInfo = new FunctionInfo();
            ImageIndex = imageIndex;
        }

        public AutocompleteItem(string name, string addition, string additionWithTypes, string returnTypeName, int imageIndex)
            : this(name+ addition, imageIndex)
        {
            functionInfo = new FunctionInfo();
            // this.menuText = menuText;
            _name = name;
            _returnTypeName = returnTypeName;
            _addition = addition;
            _additionWithTypes = additionWithTypes;
        }

        private string _name;
        private string _returnTypeName;
        private string _addition;
        private string _additionWithTypes;
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
            get
            {

                if(menuText!=null)
                return menuText;


                string ret;
                if (IsScripting)
                {
                    ret= ((Settings.Default.ShowReturnTypeInScripting) ? this._returnTypeName + " " : "") +this._name+
                           (Settings.Default.ShowParametersTypeInScripting ? this._additionWithTypes : this._addition);
                }
                else
                {
                    ret= ((Settings.Default.ShowReturnTypeInExpression) ? this._returnTypeName + " " : "") + this._name +
                          (Settings.Default.ShowParametersTypeInExpression ? this._additionWithTypes : this._addition);
                }
                if(string.IsNullOrEmpty(ret)||string.IsNullOrWhiteSpace(ret))
                    return Text;
                else
                {
                    return ret;
                }
            }
           // set { menuText = value; }
        }

        public bool IsScripting { get; set; }


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
            return menuText ?? MenuText;
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