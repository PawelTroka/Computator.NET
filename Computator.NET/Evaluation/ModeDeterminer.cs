namespace Computator.NET.Evaluation
{
    internal class ModeDeterminer
    {
        private readonly System.Text.RegularExpressions.Regex FindY;
        private readonly System.Text.RegularExpressions.Regex FindZ;
        private readonly string post = $@")(?:[\+\-{DataTypes.SpecialSymbols.DotSymbol}\/\,\)\s](?:\n|\r|\r\n|.)*)?$";
        private readonly string postExponent = $@")(?:(?:[^{DataTypes.SpecialSymbols.SuperscriptAlphabet}]+)|$)";
        //^(?:(?:\n|\r|\r\n|.)*[\+\-·\/\,\(\s])?(x)(?:[\+\-·\/\,\)\s](?:\n|\r|\r\n|.)*)?$
        private readonly string pre = $@"^(?:(?:\n|\r|\r\n|.)*[\+\-{DataTypes.SpecialSymbols.DotSymbol}\/\,\(\s])?(";
        private readonly string preExponent = $@"[^{DataTypes.SpecialSymbols.SuperscriptAlphabet}]+(";
        private System.Text.RegularExpressions.Regex FindI;
        private System.Text.RegularExpressions.Regex FindX;

        public ModeDeterminer()
        {
            //  FindX = new Regex(pre + "x" + post, RegexOptions.Compiled);
            FindY =
                new System.Text.RegularExpressions.Regex(
                    mergePatterns(pre + "y" + post,
                        preExponent + DataTypes.SpecialSymbols.AsciiToSuperscript("y") + postExponent),
                    System.Text.RegularExpressions.RegexOptions.Compiled);

            FindZ =
                new System.Text.RegularExpressions.Regex(
                    mergePatterns(pre + "z" + post,
                        preExponent + DataTypes.SpecialSymbols.AsciiToSuperscript("z") + postExponent),
                    System.Text.RegularExpressions.RegexOptions.Compiled);

            FindI =
                new System.Text.RegularExpressions.Regex(
                    mergePatterns(pre + "i" + post,
                        preExponent + DataTypes.SpecialSymbols.AsciiToSuperscript("i") + postExponent),
                    System.Text.RegularExpressions.RegexOptions.Compiled);
        }

        //[^⁰¹²³⁴⁵⁶⁷⁸⁹ᴬᴮᴰᴱᴳᴴᴵᴶᴷᴸᴹᴺᴼᴾᴿᵀᵁᵂᵃᵇᶜᵈᵉᶠᵍʰⁱʲᵏˡᵐⁿᵒᵖʳˢᵗᵘᵛʷʸᶻᵅᵝᵞᵟᵋᶿᶥᶲᵠᵡ](ˣ)(?:(?:[^⁰¹²³⁴⁵⁶⁷⁸⁹ᴬᴮᴰᴱᴳᴴᴵᴶᴷᴸᴹᴺᴼᴾᴿᵀᵁᵂᵃᵇᶜᵈᵉᶠᵍʰⁱʲᵏˡᵐⁿᵒᵖʳˢᵗᵘᵛʷʸᶻᵅᵝᵞᵟᵋᶿᶥᶲᵠᵡ]+)|$)


        private string mergePatterns(params string[] patterns)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var pattern in patterns)
            {
                sb.AppendFormat(@"(?:{0})|", pattern);
            }
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        public CalculationsMode DetermineMode(string input)
        {
            var isImplicit = input.Contains("=");
            var isZ = FindZ.IsMatch(input);
            var isI = FindZ.IsMatch(input);
            var isY = FindY.IsMatch(input);


            if (!isImplicit)
            {
                if (isZ || isI)
                    return CalculationsMode.Complex;
                return isY ? CalculationsMode.Fxy : CalculationsMode.Real;
                //return FindX.IsMatch(input) ? CalculationsMode.Real : CalculationsMode.Error;
            }

            if (isI)
                return CalculationsMode.Error; //TODO: we dont have implicit mode for complex function yet

            if (isZ && isY)
                return CalculationsMode.Fxy;

            return CalculationsMode.Real;
        }
    }
}