using System;
using Computator.NET.Core.Autocompletion;

namespace Computator.NET.Controls.AutocompleteMenu
{
    public class FuzzyAutoCompleteItem : AutocompleteItem
    {
        private IFunctionsDetails _functionsDetails;
        public FuzzyAutoCompleteItem(string text, IFunctionsDetails functionsDetails) : base(text,functionsDetails)
        {
            _functionsDetails = functionsDetails;
        }

        public FuzzyAutoCompleteItem(string name, string addition, string additionWithTypes, string returnTypeName,
            int imageIndex, IFunctionsDetails functionsDetails) : base(name, addition, additionWithTypes, returnTypeName, imageIndex, functionsDetails)
        {
            _functionsDetails = functionsDetails;
        }

        public override CompareResult Compare(string fragmentText)
        {
            var lev = Levenshtein(Text, fragmentText);
            if (lev > 0.9)
                return CompareResult.VisibleAndSelected;
            if (lev > 0.5)
                return CompareResult.Visible;
            return CompareResult.Hidden;
        }
    }


    /// <summary>
    ///     This autocomplete item appears after dot
    /// </summary>
    public class MethodAutocompleteItem : AutocompleteItem
    {
        private readonly string lowercaseText;
        private string firstPart;

        public MethodAutocompleteItem(string text, IFunctionsDetails functionsDetails)
            : base(text,functionsDetails)
        {
            lowercaseText = Text.ToLower();
        }

        public override CompareResult Compare(string fragmentText)
        {
            var i = fragmentText.LastIndexOf('.');
            if (i < 0)
                return CompareResult.Hidden;
            var lastPart = fragmentText.Substring(i + 1);
            firstPart = fragmentText.Substring(0, i);

            if (lastPart == "") return CompareResult.Visible;
            if (Text.StartsWith(lastPart, StringComparison.InvariantCultureIgnoreCase))
                return CompareResult.VisibleAndSelected;
            if (lowercaseText.Contains(lastPart.ToLower()))
                return CompareResult.Visible;

            return CompareResult.Hidden;
        }

        public override string GetTextForReplace()
        {
            return firstPart + "." + Text;
        }
    }

    /// <summary>
    ///     This class finds items by substring
    /// </summary>
    public class SubstringAutocompleteItem : AutocompleteItem
    {
        protected readonly bool ignoreCase;
        protected readonly string lowercaseText;

        public SubstringAutocompleteItem(string text, IFunctionsDetails functionsDetails, bool ignoreCase = true)
            : base(text,functionsDetails)
        {
            this.ignoreCase = ignoreCase;
            if (ignoreCase)
                lowercaseText = text.ToLower();
        }

        public override CompareResult Compare(string fragmentText)
        {
            if (ignoreCase)
            {
                if (lowercaseText.Contains(fragmentText.ToLower()))
                    return CompareResult.Visible;
            }
            else
            {
                if (Text.Contains(fragmentText))
                    return CompareResult.Visible;
            }

            return CompareResult.Hidden;
        }
    }
}