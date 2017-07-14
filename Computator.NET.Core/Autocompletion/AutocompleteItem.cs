//#define USE_TEXT_WIDTH

using System;
using System.Collections.Generic;
using System.Drawing;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Text;

namespace Computator.NET.Core.Autocompletion
{
    public class AutocompleteItemEqualityComparer : IEqualityComparer<AutocompleteItem>
    {
        public bool Equals(AutocompleteItem x, AutocompleteItem y)
        {
            return x.Text.Equals(y.Text);
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
        private readonly string _addition;
        private readonly string _additionWithTypes;
        private readonly string _name;
        private readonly string _returnTypeName;

        private readonly string _textForComparison;
        public object Tag;
        public IFunctionsDetails sharedViewState;

        protected AutocompleteItem(IFunctionsDetails sharedViewState)
        {
            this.sharedViewState = sharedViewState;
#if USE_TEXT_WIDTH
            TextWidth = 400;
#endif
        }

        public AutocompleteItem(string text, IFunctionsDetails sharedViewState) : this(sharedViewState)
        {
            Text = text;
            _textForComparison = text.ToLowerInvariant();
        }


        public AutocompleteItem(string name, string addition, string additionWithTypes, string returnTypeName,
            int imageIndex, IFunctionsDetails sharedViewState) : this(sharedViewState)
        {
            Text = name + addition;
            _textForComparison = name.ToLowerInvariant();


            ImageIndex = imageIndex;

            _name = name;
            _returnTypeName = returnTypeName;
            _addition = addition;
            _additionWithTypes = additionWithTypes;
        }

        // private readonly string menuText;

        public FunctionInfo Info { get; } = new FunctionInfo();
        public string AssemblyName { get; set; }
        
        /// <summary>
        ///     Text for inserting into textbox
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///     Image index for this item
        /// </summary>
        public int ImageIndex { get; set; } = -1;

        /// <summary>
        ///     Title for tooltip.
        /// </summary>
        /// <remarks>Return null for disable tooltip for this item</remarks>
        public virtual string ToolTipTitle { get; set; }

        /// <summary>
        ///     Tooltip text.
        /// </summary>
        /// <remarks>For display tooltip text, ToolTipTitle must be not null</remarks>
        public virtual string ToolTipText { get; set; }

        /// <summary>
        ///     Menu text. This text is displayed in the drop-down menu.
        /// </summary>
        public virtual string MenuText
        {
            get
            {
                string ret;
                if (IsScripting)
                {
                    ret = (Settings.Default.ShowReturnTypeInScripting
                        ? _returnTypeName + " "
                        : "") + _name +
                          (Settings.Default.ShowParametersTypeInScripting
                              ? _additionWithTypes
                              : _addition);
                }
                else
                {
                    ret = (Settings.Default.ShowReturnTypeInExpression
                        ? _returnTypeName + " "
                        : "") + _name +
                          (Settings.Default.ShowParametersTypeInExpression
                              ? _additionWithTypes
                              : _addition);
                }
                if (string.IsNullOrWhiteSpace(ret))
                    return Text;
                return ret;
            }
            // set { menuText = value; }
        }

        public bool IsScripting { get; set; }


        public int TextWidth { get; private set; }

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
        public virtual CompareResult Compare(string fragmentText) //hybrid compare
        {
            var normalizedFragmentText = NormalizeString(fragmentText);

            var compareExpliciteResult = CompareExplicite(normalizedFragmentText);

            if (normalizedFragmentText.Length < 3 || compareExpliciteResult != CompareResult.Hidden)
                return compareExpliciteResult;
            var substringCompareResult = CompareSubstring(normalizedFragmentText);
            return substringCompareResult == CompareResult.Hidden
                ? CompareFuzzy(normalizedFragmentText)
                : substringCompareResult;
        }

        private static string NormalizeString(string str)
        {
            var index = str.IndexOfAny(SpecialSymbols.SuperscriptsWithoutSpace.ToCharArray());


            var normalizedFragmentText = str;

            if (index != -1)
                normalizedFragmentText = str.Substring(index);


            normalizedFragmentText = SpecialSymbols.SuperscriptsToAscii(normalizedFragmentText).ToLowerInvariant();
            return normalizedFragmentText;
        }


        public CompareResult CompareSubstring(string fragmentText)
        {
            if (_textForComparison.Contains(fragmentText.ToLowerInvariant()))
                return CompareResult.Visible;


            return CompareResult.Hidden;
        }

        private CompareResult CompareExplicite(string fragmentText)
        {
            if (_textForComparison == fragmentText)
                return CompareResult.VisibleAndSelected;

            if (_textForComparison.StartsWith(fragmentText)
                ) //we dont wanna show something if it is the same as typed string
                return CompareResult.Visible;
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
            var d = new int[src.Length + 1, dest.Length + 1];
            int i, j, cost;
            var str1 = src.ToCharArray();
            var str2 = dest.ToCharArray();

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
                            d[i - 1, j] + 1, // Deletion
                            Math.Min(
                                d[i, j - 1] + 1, // Insertion
                                d[i - 1, j - 1] + cost)); // Substitution

                    if ((i > 1) && (j > 1) && (str1[i - 1] ==
                                               str2[j - 2]) && (str1[i - 2] == str2[j - 1]))
                    {
                        d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
                    }
                }
            }

            var dist = (float) d[str1.Length, str2.Length];

            return 1 - dist/Math.Max(str1.Length, str2.Length);
        }


        /// <summary>
        ///     Returns text for display into popup menu
        /// </summary>
        public override string ToString()
        {
            return MenuText;
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