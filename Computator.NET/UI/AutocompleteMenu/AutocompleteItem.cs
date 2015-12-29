//#define USE_TEXT_WIDTH
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Computator.NET.Functions;
using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;



namespace AutocompleteMenuNS
{

    /// <summary>
    ///     Item of autocomplete menu
    /// </summary>
    public class AutocompleteItem
    {
        private readonly string _addition;
        private readonly string _additionWithTypes;
        private readonly string _name;
        private readonly string _returnTypeName;
        private readonly string menuText;
        public FunctionInfo functionInfo;
        public object Tag;
        private string toolTipText;
        private string toolTipTitle;

        public AutocompleteItem()
        {
            functionInfo = new FunctionInfo();
            ImageIndex = -1;
#if USE_TEXT_WIDTH
            TextWidth = 400;
#endif
        }

        public AutocompleteItem(string text) : this()
        {
            functionInfo = new FunctionInfo();
            Text = text;
            lowercaseText = text.ToLower();
        }

        public AutocompleteItem(string text, int imageIndex)
            : this(text)
        {
            functionInfo = new FunctionInfo();
            ImageIndex = imageIndex;
        }

        public AutocompleteItem(string name, string addition, string additionWithTypes, string returnTypeName,
            int imageIndex)
            : this(name + addition, imageIndex)
        {
            functionInfo = new FunctionInfo();
            // this.menuText = menuText;
            _name = name;
            _returnTypeName = returnTypeName;
            _addition = addition;
            _additionWithTypes = additionWithTypes;
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

        private string lowercaseText;

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
                if (menuText != null)
                    return menuText;


                string ret;
                if (IsScripting)
                {
                    ret = ((Settings.Default.ShowReturnTypeInScripting)
                        ? _returnTypeName + " "
                        : "") + _name +
                          (Settings.Default.ShowParametersTypeInScripting
                              ? _additionWithTypes
                              : _addition);
                }
                else
                {
                    ret = ((Settings.Default.ShowReturnTypeInExpression)
                        ? _returnTypeName + " "
                        : "") + _name +
                          (Settings.Default.ShowParametersTypeInExpression
                              ? _additionWithTypes
                              : _addition);
                }
                if (string.IsNullOrEmpty(ret) || string.IsNullOrWhiteSpace(ret))
                    return Text;
                return ret;
            }
            // set { menuText = value; }
        }

        public bool IsScripting { get; set; }


        public int TextWidth { get; private set; }

        public CompletionData ToCompletionData()
        {
            return new CompletionData(Text, MenuText, functionInfo, ImageIndex);
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
       /* public virtual CompareResult Compare(string fragmentText)
        {
            if (Text.StartsWith(fragmentText, StringComparison.InvariantCultureIgnoreCase) &&
                Text != fragmentText)
                return CompareResult.VisibleAndSelected;

            return CompareResult.Hidden;
        }*/




        public virtual CompareResult Compare(string fragmentText)//hybrid compare
        {
            if (fragmentText.Length < 3)
                return CompareExplicite(fragmentText);
            else
            {
                var substringCompareResult = CompareSubstring(fragmentText);
                return substringCompareResult == CompareResult.Hidden ? CompareFuzzy(fragmentText) : substringCompareResult;
            }
        }


        public CompareResult CompareSubstring(string fragmentText)
        {

            if (lowercaseText.Contains(fragmentText.ToLower()))
                return CompareResult.Visible;



            return CompareResult.Hidden;
        }

        private CompareResult CompareExplicite(string fragmentText)
        {
            if (Text.StartsWith(fragmentText, StringComparison.InvariantCultureIgnoreCase) /*&&
Text != fragmentText*/)
                return CompareResult.VisibleAndSelected;
            return CompareResult.Hidden;
        }


        private CompareResult CompareFuzzy(string fragmentText)
        {
            var lev = Levenshtein(Text, fragmentText);
            if (lev > 0.9)
                return CompareResult.VisibleAndSelected;
            if (lev > 0.5)
                return CompareResult.Visible;
            return CompareResult.Hidden;
        }

        public static float Levenshtein(string src, string dest)
        {
            int[,] d = new int[src.Length + 1, dest.Length + 1];
            int i, j, cost;
            char[] str1 = src.ToCharArray();
            char[] str2 = dest.ToCharArray();

            for (i = 0; i <= str1.Length; i++)
            {
                d[i, 0] = i;
            }
            for (j = 0; j <= str2.Length; j++)
            {
                d[0, j] = j;
            }
            for (i = 1; i <= str1.Length; i++)
            {
                for (j = 1; j <= str2.Length; j++)
                {

                    if (str1[i - 1] == str2[j - 1])
                        cost = 0;
                    else
                        cost = 1;

                    d[i, j] =
                        Math.Min(
                            d[i - 1, j] + 1,              // Deletion
                            Math.Min(
                                d[i, j - 1] + 1,          // Insertion
                                d[i - 1, j - 1] + cost)); // Substitution

                    if ((i > 1) && (j > 1) && (str1[i - 1] ==
                        str2[j - 2]) && (str1[i - 2] == str2[j - 1]))
                    {
                        d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
                    }
                }
            }

            var dist = (float)d[str1.Length, str2.Length];

            return 1 - dist / Math.Max(str1.Length, str2.Length);
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
            using (var brush = new SolidBrush(e.IsSelected ? e.Colors.SelectedForeColor : e.Colors.ForeColor))
                e.Graphics.DrawString(ToString(), e.Font, brush, e.TextRect, e.StringFormat);

#if USE_TEXT_WIDTH
            TextWidth = TextRenderer.MeasureText(ToString(), e.Font).Width;
            //TextWidth = (int)(Graphics.MeasureString(ToString(), e.Font, new SizeF(10000F,10000F), e.StringFormat).Width);
#endif

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