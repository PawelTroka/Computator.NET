using Enumerable = System.Linq.Enumerable;

namespace Computator.NET.UI
{
    internal class NumericTextBox : System.Windows.Forms.TextBox
    {
        public NumericTextBox()
        {
            TextChanged += NumericTextBox_TextChanged;
        }

        private void NumericTextBox_TextChanged(object sender, System.EventArgs e)
        {
            if (!Enumerable.Contains(Text, 'E') && !Enumerable.Contains(Text, 'e')) return;

            var chunks = Text.Split('E', 'e', 'i');
            if (!Enumerable.Contains(Text, 'i'))
                Text = chunks[0] + DataTypes.SpecialSymbols.DotSymbol + "10" +
                       DataTypes.SpecialSymbols.AsciiToSuperscript(chunks[1]);
            else
            {
                //1. -1E-11 + 5E-11i
                if (Enumerable.Count(Text, c => c == 'E') >= 2)
                {
                    var midChunk = chunks[1].Insert(chunks[1].LastIndexOfAny(new[] {'+', '-'}) + 1, "(");

                    var midChunks = midChunk.Split(new[] {"+(", "-(", "+ (", "- ("},
                        System.StringSplitOptions.RemoveEmptyEntries);

                    Text = chunks[0] + DataTypes.SpecialSymbols.DotSymbol + "10" +
                           DataTypes.SpecialSymbols.AsciiToSuperscript(midChunks[0]) +
                           midChunk.Substring(midChunk.LastIndexOfAny(new[] {'+', '-'})) +
                           DataTypes.SpecialSymbols.DotSymbol + "10" +
                           DataTypes.SpecialSymbols.AsciiToSuperscript(chunks[2]) + ")" +
                           DataTypes.SpecialSymbols.DotSymbol + "i";
                }
                else
                {
                    if (Enumerable.Count(chunks[0], c => c == '+') == 0 &&
                        System.Text.RegularExpressions.Regex.IsMatch(chunks[1], @"^[+\-]?(\d+)$") &&
                        System.Text.RegularExpressions.Regex.IsMatch(chunks[0], @"^-?(\d+\.?\d*)$"))
                        //2.      5E-11i//-5·16²²·i
                        Text = "(" + chunks[0] + DataTypes.SpecialSymbols.DotSymbol + "10" +
                               DataTypes.SpecialSymbols.AsciiToSuperscript(chunks[1]) + ")" +
                               DataTypes.SpecialSymbols.DotSymbol + "i";


                    //3. -1 + 5E-11i//-5·16²²·i+22
                    else if (System.Text.RegularExpressions.Regex.IsMatch(chunks[0], @"^-?(\d+\.?\d*)[\+\-](\d+\.?\d*)$"))
                        Text = chunks[0].Insert(chunks[0].LastIndexOfAny(new[] {'+', '-'}) + 1, "(") +
                               DataTypes.SpecialSymbols.DotSymbol + "10" +
                               DataTypes.SpecialSymbols.AsciiToSuperscript(chunks[1]) + ")" +
                               DataTypes.SpecialSymbols.DotSymbol + "i";


                    //4. -1E-11 + 5i
                    else if (System.Text.RegularExpressions.Regex.IsMatch(chunks[1],
                        @"^[+\-]?(\d+)[\+\-](\d+\.?\d*)$"))
                    {
                        var chunk11 = chunks[1].Substring(0, chunks[1].LastIndexOfAny(new[] {'+', '-'}));

                        var chunk12 = chunks[1].Substring(chunks[1].LastIndexOfAny(new[] {'+', '-'}));


                        Text = chunks[0] +
                               DataTypes.SpecialSymbols.DotSymbol + "10" +
                               DataTypes.SpecialSymbols.AsciiToSuperscript(chunk11) +
                               chunk12 + DataTypes.SpecialSymbols.DotSymbol + "i";
                    }
                }
            }
        }
    }
}