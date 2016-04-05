using System;
using System.Linq;
using System.Text;

namespace Computator.NET.DataTypes
{
    public static class SpecialSymbols
    {
        public const char DotSymbol = '·'; //U+00B7 · middle dot (HTML &#183; · &middot;).
        //alternatives:
        //public const char dotSymbol = '⋅';//U+22C5 ⋅ dot operator (HTML &#8901; · &sdot;
        //public const char dotSymbol = '∙';//U+2219 ∙ bullet operator (HTML &#8729;)
        //public const char dotSymbol = '•';//U+2022 • bullet (HTML &#8226; · &bull;

        //ⱽ

        //ॱ 
        //˸

        public const string Infinity = "∞";

        public static readonly string DecimalSeparatorSuperscript = @"ᱸ";//(Environment.OSVersion.Version.Major > 5) ? @"ᱸ" : @"ॱ";

        public static readonly string CommaSuperscript = @"ʹ";//"\u0315";//(Environment.OSVersion.Version.Major>5) ? @"՚" : @"՛";
        //2ᴹᵃᵗʸᵃˢ⁽ˣʹʸ⁾


        public static readonly string SuperscriptsWithoutSpace =
            "⁽⁾⁺⁻˙˸⁼"+DecimalSeparatorSuperscript+SuperscriptAlphabet+CommaSuperscript;


        public static readonly string Superscripts = " " + SuperscriptsWithoutSpace;

        public const string SuperscriptAlphabet = "⁰¹²³⁴⁵⁶⁷⁸⁹ᴬᴮᴰᴱᴳᴴᴵᴶᴷᴸᴹᴺᴼᴾᴿᵀᵁᵂᵃᵇᶜᵈᵉᶠᵍʰⁱʲᵏˡᵐⁿᵒᵖʳˢᵗᵘᵛʷˣʸᶻᵅᵝᵞᵟᵋᶿᶥᶲᵠᵡ";

        public const string AsciiForSuperscripts =
            " ()+-*/=.0123456789ABDEGHIJKLMNOPRTUWabcdefghijklmnoprstuvwxyzαβγδεθιφψχ,"; //ⱽ

        private const char blankCharacter = 'ⱽ';
        public const char ExponentModeSymbol = '^';

        public static string SuperscriptsToAscii(string s)
        {
            var sb = new StringBuilder(s);

            for (var i = 0; i < s.Length; i++)
                if (Superscripts.Contains(sb[i]))
                    sb[i] = SuperscriptToAscii(sb[i]);
            return sb.ToString();
        }

        public static string AsciiToSuperscript(string s)
        {
            var sb = new StringBuilder(s);

            for (var i = 0; i < s.Length; i++)
                if (AsciiForSuperscripts.Contains(sb[i]))
                    sb[i] = AsciiToSuperscript(sb[i]);
            return sb.ToString();
        }

        public static char AsciiToSuperscript(char c)
        {
            if (AsciiForSuperscripts.Contains(c))
                return Superscripts[AsciiForSuperscripts.IndexOf(c)];
            return blankCharacter;
        }

        public static char SuperscriptToAscii(char c)
        {
            if (Superscripts.Contains(c))
                return AsciiForSuperscripts[Superscripts.IndexOf(c)];
            return blankCharacter;
        }

        public static bool IsAscii(char c)
        {
            return (c < 128 && c > 31);
        }
    }
}