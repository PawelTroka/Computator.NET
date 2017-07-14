using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Computator.NET.DataTypes;

// ReSharper disable LocalizableElement

namespace Computator.NET.Controls
{
    internal class NumericTextBox : TextBox
    {
        public NumericTextBox()
        {
            TextChanged += NumericTextBox_TextChanged;
        }

        private void NumericTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!Text.Contains('E') && !Text.Contains('e')) return;

            var chunks = Text.Split('E', 'e', 'i');
            if (!Text.Contains('i'))
                Text = chunks[0] + SpecialSymbols.DotSymbol + "10" +
                       SpecialSymbols.AsciiToSuperscript(chunks[1]);
            else
            {
                //1. -1E-11 + 5E-11i
                if (Text.Count(c => c == 'E') >= 2)
                {
                    var midChunk = chunks[1].Insert(chunks[1].LastIndexOfAny(new[] {'+', '-'}) + 1, "(");

                    var midChunks = midChunk.Split(new[] {"+(", "-(", "+ (", "- ("},
                        StringSplitOptions.RemoveEmptyEntries);

                    Text = chunks[0] + SpecialSymbols.DotSymbol + "10" +
                           SpecialSymbols.AsciiToSuperscript(midChunks[0]) +
                           midChunk.Substring(midChunk.LastIndexOfAny(new[] {'+', '-'})) +
                           SpecialSymbols.DotSymbol + "10" +
                           SpecialSymbols.AsciiToSuperscript(chunks[2]) + ")" +
                           SpecialSymbols.DotSymbol + "i";
                }
                else
                {
                    if (chunks[0].Count(c => c == '+') == 0 &&
                        Regex.IsMatch(chunks[1], @"^[+\-]?(\d+)$") &&
                        Regex.IsMatch(chunks[0], @"^-?(\d+\.?\d*)$"))
                        //2.      5E-11i//-5·16²²·i
                        Text = "(" + chunks[0] + SpecialSymbols.DotSymbol + "10" +
                               SpecialSymbols.AsciiToSuperscript(chunks[1]) + ")" +
                               SpecialSymbols.DotSymbol + "i";


                    //3. -1 + 5E-11i//-5·16²²·i+22
                    else if (Regex.IsMatch(chunks[0], @"^-?(\d+\.?\d*)[\+\-](\d+\.?\d*)$"))
                        Text = chunks[0].Insert(chunks[0].LastIndexOfAny(new[] {'+', '-'}) + 1, "(") +
                               SpecialSymbols.DotSymbol + "10" +
                               SpecialSymbols.AsciiToSuperscript(chunks[1]) + ")" +
                               SpecialSymbols.DotSymbol + "i";


                    //4. -1E-11 + 5i
                    else if (Regex.IsMatch(chunks[1],
                        @"^[+\-]?(\d+)[\+\-](\d+\.?\d*)$"))
                    {
                        var chunk11 = chunks[1].Substring(0, chunks[1].LastIndexOfAny(new[] {'+', '-'}));

                        var chunk12 = chunks[1].Substring(chunks[1].LastIndexOfAny(new[] {'+', '-'}));


                        Text = chunks[0] +
                               SpecialSymbols.DotSymbol + "10" +
                               SpecialSymbols.AsciiToSuperscript(chunk11) +
                               chunk12 + SpecialSymbols.DotSymbol + "i";
                    }
                }
            }
        }
    }
}