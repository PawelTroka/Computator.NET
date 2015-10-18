using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Computator.NET.Config;

namespace Computator.NET.Evaluation
{
    class ModeDeterminer
    {
        //^(?:(?:\n|\r|\r\n|.)*[\+\-·\/\,\(\s])?(x)(?:[\+\-·\/\,\)\s](?:\n|\r|\r\n|.)*)?$
        private readonly string pre = @"^(?:(?:\n|\r|\r\n|.)*[\+\-" + SpecialSymbols.DotSymbol + @"\/\,\(\s])?(";
        private readonly string post = @")(?:[\+\-" + SpecialSymbols.DotSymbol + @"\/\,\)\s](?:\n|\r|\r\n|.)*)?$";

        public ModeDeterminer()
        {
          //  FindX = new Regex(pre + "x" + post, RegexOptions.Compiled);
            FindY = new Regex(pre + "y" + post, RegexOptions.Compiled);
            FindZ = new Regex(pre + "z|i" + post, RegexOptions.Compiled);
        }

        public CalculationsMode DetermineMode(string input)
        {
            if (FindZ.IsMatch(input))
                return CalculationsMode.Complex;
            if (FindY.IsMatch(input))
                return CalculationsMode.Fxy;
            //return FindX.IsMatch(input) ? CalculationsMode.Real : CalculationsMode.Error;
            return CalculationsMode.Real;
        }

        private Regex FindX, FindY, FindZ;
    }
}
