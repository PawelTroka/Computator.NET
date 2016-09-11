using System.Text;
using System.Text.RegularExpressions;
using Computator.NET.Compilation;
using Computator.NET.DataTypes;

namespace Computator.NET.Evaluation
{
    public interface IModeDeterminer
    {
        CalculationsMode DetermineMode(string input);
    }

    public class ModeDeterminer : IModeDeterminer
    {
        private readonly Regex FindI;
        private readonly Regex FindY;
        private readonly Regex FindZ;

        private readonly string post =
            $@")(?:[\+\-\*{SpecialSymbols.DotSymbol}\/\,\)\s{SpecialSymbols.Superscripts}](?:\n|\r|\r\n|.)*)?$";

        private readonly string postExponent = $@")(?:(?:[^{SpecialSymbols.SuperscriptAlphabet}]+)|$)";
        //^(?:(?:\n|\r|\r\n|.)*[\+\-·\/\,\(\s])?(x)(?:[\+\-·\/\,\)\s](?:\n|\r|\r\n|.)*)?$
        private readonly string pre = $@"^(?:(?:\n|\r|\r\n|.)*[\+\-\*{SpecialSymbols.DotSymbol}\/\,\(\s])?(";
        private readonly string preExponent = $@"[^{SpecialSymbols.SuperscriptAlphabet}]+(";
        //private Regex FindX;
        private readonly ITslCompiler _tslCompiler;

        public ModeDeterminer(ITslCompiler tslCompiler)
        {
            _tslCompiler = tslCompiler;
            //  FindX = new Regex(pre + "x" + post, RegexOptions.Compiled);
            FindY =
                new Regex(
                    mergePatterns(pre + "y" + post,
                        preExponent + SpecialSymbols.AsciiToSuperscript("y") + postExponent),
                    RegexOptions.Compiled);

            FindZ =
                new Regex(
                    mergePatterns(pre + "z" + post,
                        preExponent + SpecialSymbols.AsciiToSuperscript("z") + postExponent),
                    RegexOptions.Compiled);

            FindI =
                new Regex(
                    mergePatterns(pre + "i" + post,
                        preExponent + SpecialSymbols.AsciiToSuperscript("i") + postExponent),
                    RegexOptions.Compiled);
        }

        //[^⁰¹²³⁴⁵⁶⁷⁸⁹ᴬᴮᴰᴱᴳᴴᴵᴶᴷᴸᴹᴺᴼᴾᴿᵀᵁᵂᵃᵇᶜᵈᵉᶠᵍʰⁱʲᵏˡᵐⁿᵒᵖʳˢᵗᵘᵛʷʸᶻᵅᵝᵞᵟᵋᶿᶥᶲᵠᵡ](ˣ)(?:(?:[^⁰¹²³⁴⁵⁶⁷⁸⁹ᴬᴮᴰᴱᴳᴴᴵᴶᴷᴸᴹᴺᴼᴾᴿᵀᵁᵂᵃᵇᶜᵈᵉᶠᵍʰⁱʲᵏˡᵐⁿᵒᵖʳˢᵗᵘᵛʷʸᶻᵅᵝᵞᵟᵋᶿᶥᶲᵠᵡ]+)|$)


        private static string mergePatterns(params string[] patterns)
        {
            var sb = new StringBuilder();
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
            input = _tslCompiler.TransformToCSharp(input);
            var isZ = FindZ.IsMatch(input);
            var isI = FindI.IsMatch(input);
            var isY = FindY.IsMatch(input);


            if (!isImplicit)
            {
                if (isZ || isI)
                    return CalculationsMode.Complex;
                return isY ? CalculationsMode.Fxy : CalculationsMode.Real;
                //return FindX.IsMatch(input) ? CalculationsMode.Real : CalculationsMode.Error;
            }

            if (isI)
                return CalculationsMode.Error; //TODO: we don't have implicit mode for complex function yet

            if (isZ && isY)
                return CalculationsMode.Fxy;

            return CalculationsMode.Real;
        }
    }
}