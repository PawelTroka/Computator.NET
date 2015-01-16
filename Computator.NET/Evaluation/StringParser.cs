using System.Text.RegularExpressions;
using Computator.NET.Config;

namespace Computator.NET.Evaluation
{
    public static class StringParser
    {
        public static string ReplaceMultipling(this string input, params char[] variables) //OK
        {
            string result = input;
            foreach (char c in variables)
                result = Regex.Replace(result, @"(\d+\.?\d*)(" + c + @")", @"$1*$2");
            return result;
        }

//string result = input.ReplacePow(@"(\d*x)\^(\d+\.?\d*)")
        /* public static string ReplaceComplexNumbers(this string input)//OK
        {
            return Regex.Replace(input, @"(\d+\.?\d*)i", "$1*i");
        }*/

        /// (\d+)\^(\d+)/
        public static string ReplacePow(this string input, params char[] variables) //OK
        {
            //string result = input.ReplacePow(@"(\d*x)\^(\d+\.?\d*)");
            //return result.ReplacePow(@"\(([^\^]+)\)\^(\d+\.?\d*)");
            string result = input;

            //case 1 - x^ANY_EXPONENT, 9.5y^ANY_EXPONENT, etc. 
            foreach (char c in variables)
                result = result.ReplacePow(@"(\d*" + c + @")([" + GlobalConfig.Superscripts + @"]+)");

            //case 2 - (anything in parenthesis)^ANY_EXPONENT
            // result = result.ReplacePow(@"\(([^\^]+)\)([" + GlobalConfig.Superscripts + @"]+)");
            result =
                result.ReplacePow(@"(\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))([" +
                                  GlobalConfig.Superscripts + @"]+)");
            //# First '(' # Match all non-braces # Match '(', and capture into 'open' # Match ')', and delete the 'open' capture # Fails if 'open' stack isn't empty!

            //case 3 - (any ordinary number)^ANY_EXPONENT
            result = result.ReplacePow(@"(\d+\.?\d*)([" + GlobalConfig.Superscripts + @"]+)");

            //replace superscripts with normal characters:
            result = GlobalConfig.SuperscriptsToAscii(result);

            return result.ReplaceMultipling(variables);
        }

        private static string ReplacePow(this string input, string toReplace)
        {
            return Regex.Replace(input, toReplace, "pow($1,$2)");
        }

        public static string ReplaceToDoubles(this string input)
        {
            //return Regex.Replace(input, @"(\d+)(?:[^\.]\d+)", "$1.0");

            //1. X/5 => X/5.0
            string ret = Regex.Replace(input, @"[\/](\d+)([^\.\d]|$)", @"/$1.0$2");
            //System.Windows.Forms.MessageBox.Show(ret);
            //2. X/(4+9+...+i) => X/(1.0*(4+9+...+i)) 
            ret = Regex.Replace(ret, @"[\/](\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))", "/(1.0*($1))");
            //System.Windows.Forms.MessageBox.Show(ret);
            return ret;
        }
    }
}