using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Computator.NET.Config;

namespace Computator.NET.UI
{
    internal class NumericTextBox : TextBox
    {
        public NumericTextBox()
        {
            TextChanged += NumericTextBox_TextChanged;
        }

        private void NumericTextBox_TextChanged(object sender, EventArgs e)
        {
            if (Text.Contains('E') || Text.Contains('e'))
            {
                string[] chunks = Text.Split('E', 'e', 'i');
                if (!Text.Contains('i'))
                    Text = chunks[0] + GlobalConfig.dotSymbol + "10" + GlobalConfig.AsciiToSuperscript(chunks[1]);
                else
                {
                    //1. -1E-11 + 5E-11i
                    if (Text.Count(c => c == 'E') >= 2)
                    {
                        string midChunk = chunks[1].Insert(chunks[1].LastIndexOfAny(new[] {'+', '-'}) + 1, "(");

                        string[] midChunks = midChunk.Split(new[] {"+(", "-(", "+ (", "- ("},
                            StringSplitOptions.RemoveEmptyEntries);

                        Text = chunks[0] + GlobalConfig.dotSymbol + "10" + GlobalConfig.AsciiToSuperscript(midChunks[0]) +
                               midChunk.Substring(midChunk.LastIndexOfAny(new[] {'+', '-'})) +
                               GlobalConfig.dotSymbol + "10" + GlobalConfig.AsciiToSuperscript(chunks[2]) + ")" +
                               GlobalConfig.dotSymbol + "i";
                    }
                    else
                    {
                        if (chunks[0].Count(c => c == '+') == 0 && Regex.IsMatch(chunks[1], @"^[+\-]?(\d+)$") &&
                            Regex.IsMatch(chunks[0], @"^-?(\d+\.?\d*)$"))
                            //2.      5E-11i//-5·16²²·i
                            Text = "(" + chunks[0] + GlobalConfig.dotSymbol + "10" +
                                   GlobalConfig.AsciiToSuperscript(chunks[1]) + ")" + GlobalConfig.dotSymbol + "i";


                            //3. -1 + 5E-11i//-5·16²²·i+22
                        else if (Regex.IsMatch(chunks[0], @"^-?(\d+\.?\d*)[\+\-](\d+\.?\d*)$"))
                            Text = chunks[0].Insert(chunks[0].LastIndexOfAny(new[] {'+', '-'}) + 1, "(") +
                                   GlobalConfig.dotSymbol + "10" +
                                   GlobalConfig.AsciiToSuperscript(chunks[1]) + ")" + GlobalConfig.dotSymbol + "i";


                            //4. -1E-11 + 5i
                        else if (Regex.IsMatch(chunks[1], @"^[+\-]?(\d+)[\+\-](\d+\.?\d*)$"))
                        {
                            string chunk11 = chunks[1].Substring(0, chunks[1].LastIndexOfAny(new[] {'+', '-'}));

                            string chunk12 = chunks[1].Substring(chunks[1].LastIndexOfAny(new[] {'+', '-'}));


                            Text = chunks[0] +
                                   GlobalConfig.dotSymbol + "10" + GlobalConfig.AsciiToSuperscript(chunk11) +
                                   chunk12 + GlobalConfig.dotSymbol + "i";
                        }
                    }
                }
            }
        }
    }
}