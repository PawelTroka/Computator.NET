using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Computator.NET.DataTypes.Text;

namespace Computator.NET.Engine.Symbolic;

/// <summary>Normalizes mathematical notation without passing it through the C# compiler.</summary>
internal static class SymbolicInput
{
    private static readonly HashSet<string> Functions = new HashSet<string>((
        "log log10 log2 pow sqrt cbrt sqr root ln exp sin cos tan tg cotan cot ctg sec cosec csc " +
        "arcsin arccos arctan arccotan arcsec arccosec arccsc acsc asin acos atan acotan asec acosec acot arccot " +
        "arctg arcctg sinh sh cosh ch tanh tgh th cotanh coth ctgh cth sech sch cosech csch asinh arsinh arsh arcsinh " +
        "acosh arcosh arch arccosh atanh artanh artgh arth arctanh acoth arcoth arctgh acotanh arcotanh arcth " +
        "asech arsech arsch acosech arcosech arcsch acsch factorial gamma derivative integral limit limitleft limitright " +
        "sum product signum sgn sign abs phi prime valuation card powerset union intersection complement image preimage " +
        "floor ceil ceiling round min max argmax argmin gcd lcm binomial iverson aleph erf erfc erfi Ei li Si Ci Shi Chi " +
        "domain piecewise apply lambda").Split(' '), StringComparer.Ordinal);
    private static readonly HashSet<string> Keywords = new HashSet<string>((
        "and or xor not implies in provided intersect unite setsubtract mod subset").Split(' '), StringComparer.Ordinal);
    private static readonly Regex Identifier = new Regex(
        @"^[a-zA-Z\u0370-\u03ff\u1f00-\u1fff\u0400-\u04ff]+(?:_[a-zA-Z0-9\u0370-\u03ff\u1f00-\u1fff\u0400-\u04ff]+)?$",
        RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    private static readonly Regex Number = new Regex(
        @"\G(?:[0-9]+(?:_[0-9]+)*(?:\.[0-9]*(?:_[0-9]+)*)?|\.[0-9]+(?:_[0-9]+)*)(?:[eE][+-]?[0-9]+)?",
        RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    internal static bool IsVariableName(string name) => name != null && name.Length <= 64 && Identifier.IsMatch(name) &&
        !Functions.Contains(name) && !Keywords.Contains(name) &&
        !new[] { "pi", "PI", "π", "e", "ℯ", "i", "oo", "Infinity", "NaN", "true", "false", "RR", "CC", "QQ", "ZZ", "NN", "BB", "PP", "Any" }.Contains(name);

    internal static void ValidateText(string text, string name, bool required = true)
    {
        if (text == null || text.Length > SymbolicEngine.MaximumExpressionCharacters || required && string.IsNullOrWhiteSpace(text))
            throw new ArgumentException(name + " must contain 1–8,192 characters.", name);
        var depth = 0;
        var operators = 0;
        var run = 0;
        foreach (var character in text)
        {
            if (character == '(' || character == '[' || character == '{' || character == '⁽')
            {
                if (++depth > 64) throw new ArgumentException(name + " exceeds 64 levels of nesting.", name);
            }
            else if (character == ')' || character == ']' || character == '}' || character == '⁾') depth--;
            if ("+-*/^!=<>:,;|&⁺⁻˙˸⁼".IndexOf(character) >= 0 && ++operators > 512)
                throw new ArgumentException(name + " exceeds the 512-operator input budget.", name);
            if (char.IsDigit(character))
            {
                if (++run > 128) throw new ArgumentException(name + " contains a number longer than 128 digits.", name);
            }
            else run = 0;
            if (char.IsControl(character) && !char.IsWhiteSpace(character))
                throw new ArgumentException(name + " contains an unsupported control character.", name);
        }
    }

    internal static string Normalize(string text) => TranslateCalls(NormalizeCore(text, 0), 0);

    private static string NormalizeCore(string text, int recursion)
    {
        if (recursion > 64) throw new ArgumentException("The symbolic expression is nested too deeply.");
        // A function power is a power of the completed call, not of its argument.
        for (var index = 0; index < text.Length; index++)
        {
            if (!IsSuperscript(text[index])) continue;
            var end = index;
            // The editor keeps spaces in exponent mode. Read one complete run,
            // not separate powers for x² ˸ ʸ, and leave trailing normal space out.
            while (end < text.Length && (IsSuperscript(text[end]) || char.IsWhiteSpace(text[end]))) end++;
            while (end > index && char.IsWhiteSpace(text[end - 1])) end--;
            var exponent = SpecialSymbols.SuperscriptsToAscii(text.Substring(index, end - index));
            var nameStart = index;
            while (nameStart > 0 && char.IsWhiteSpace(text[nameStart - 1])) nameStart--;
            var nameEnd = nameStart;
            while (nameStart > 0 && (IsNameCharacter(text[nameStart - 1]) || char.IsDigit(text[nameStart - 1]) || text[nameStart - 1] == '_')) nameStart--;
            var name = text.Substring(nameStart, nameEnd - nameStart);
            // A coefficient is not part of the callable name: 2cosʸ(x),
            // 1e3sin²(x), and 1_000log10²(x) use the same implicit product as TSL.
            if (!Functions.Contains(name) && name.Length > 0 && char.IsDigit(name[0]))
            {
                var coefficient = Number.Match(text, nameStart);
                if (coefficient.Success && nameStart + coefficient.Length < nameEnd)
                {
                    var callable = text.Substring(nameStart + coefficient.Length, nameEnd - nameStart - coefficient.Length);
                    if (Functions.Contains(callable)) { nameStart += coefficient.Length; name = callable; }
                }
            }
            var argument = end;
            while (argument < text.Length && char.IsWhiteSpace(text[argument])) argument++;
            if (Functions.Contains(name) && argument < text.Length && text[argument] == '(')
            {
                var close = MatchingParenthesis(text, argument);
                // Keep the completed prefix power grouped before scanning a
                // following postfix power: cosʸ(x)² is (cos(x)^y)^2, not
                // cos(x)^(y^2), because ordinary ^ is right-associative.
                text = text.Substring(0, nameStart) + "((" + name + "(" +
                    NormalizeCore(text.Substring(argument + 1, close - argument - 1), recursion + 1) + "))^(" +
                    NormalizeCore(exponent, recursion + 1) + "))" + text.Substring(close + 1);
                index = nameStart - 1;
            }
            else
            {
                if (index == 0) throw new ArgumentException("A superscript needs a base.");
                var replacement = "^(" + NormalizeCore(exponent, recursion + 1) + ")";
                text = text.Substring(0, index) + replacement + text.Substring(end);
                index += replacement.Length - 1;
            }
        }

        var tokens = new List<Token>();
        var absoluteBars = 0;
        for (var index = 0; index < text.Length;)
        {
            var character = text[index];
            if (char.IsWhiteSpace(character)) { index++; continue; }
            if (char.IsDigit(character) || character == '.')
            {
                var match = Number.Match(text, index);
                if (!match.Success) throw new ArgumentException("A decimal point must belong to a number; member access is not symbolic syntax.");
                tokens.Add(new Token(ExactNumber(match.Value), Kind.Atom));
                index += match.Length;
                continue;
            }
            if (IsNameCharacter(character))
            {
                var start = index++;
                while (index < text.Length && (IsNameCharacter(text[index]) || text[index] == '_' || char.IsDigit(text[index]))) index++;
                var name = text.Substring(start, index - start);
                if (name == "π" || name == "PI") name = "pi";
                if (name == "ℯ") name = "e";
                if (name == "Infinity") { tokens.Add(new Token("(+oo)", Kind.Atom)); continue; }
                // These are indivisible CAS set names in returned conditions,
                // not a multiplication or addition following the integer set.
                if (name == "ZZ" && index < text.Length && (text[index] == '*' || text[index] == '+'))
                { tokens.Add(new Token(name + text[index++], Kind.Atom)); continue; }
                if (!Identifier.IsMatch(name) && !Functions.Contains(name))
                    throw new ArgumentException("Write powers with superscripts or pow(x,2), and indexed names with an underscore (x² or x_2, not x2). ASCII x^2 is also accepted.");
                tokens.Add(new Token(name, Keywords.Contains(name) ? Kind.Operator : Kind.Name));
                continue;
            }
            if (character == '√')
            {
                var next = index + 1;
                while (next < text.Length && char.IsWhiteSpace(text[next])) next++;
                if (next == text.Length || text[next] != '(') throw new ArgumentException("Write a square root as √(expression) or sqrt(expression).");
                tokens.Add(new Token("sqrt", Kind.Name)); index++; continue;
            }
            if (character == '∞') { tokens.Add(new Token("(+oo)", Kind.Atom)); index++; continue; }
            if (character == '|')
            {
                if (absoluteBars > 0 && tokens.Count > 0 && EndsFactor(tokens[tokens.Count - 1]))
                { tokens.Add(new Token(")", Kind.Close)); absoluteBars--; }
                else
                { tokens.Add(new Token("abs", Kind.Name)); tokens.Add(new Token("(", Kind.Open)); absoluteBars++; }
                index++; continue;
            }
            if ((character == '+' || character == '-') && index + 2 < text.Length && text.Substring(index + 1, 2) == "oo")
            { tokens.Add(new Token("(" + text.Substring(index, 3) + ")", Kind.Atom)); index += 3; continue; }
            var symbol = character switch
            {
                '−' or '–' => "-", '·' or '⋅' or '×' or '∙' => "*", '÷' => "/",
                '≤' => "<=", '≥' => ">=", '≠' => "!=", _ => character.ToString()
            };
            if (symbol == "=" && index + 1 < text.Length && text[index + 1] == '=') index++;
            else if ((symbol == "<" || symbol == ">" || symbol == "!") && index + 1 < text.Length && text[index + 1] == '=')
            { symbol += "="; index++; }
            var kind = symbol == "(" ? Kind.Open : symbol == ")" ? Kind.Close :
                symbol == "]" || symbol == "}" ? Kind.CollectionClose : Kind.Operator;
            if ("+-*/^!=<>:,;|&()[]{}\\".IndexOf(symbol[0]) < 0)
                throw new ArgumentException("Unsupported symbol: " + character);
            tokens.Add(new Token(symbol, kind)); index++;
        }
        if (absoluteBars != 0) throw new ArgumentException("Absolute-value bars must be balanced; abs(expression) is also supported.");
        if (tokens.Count > 1024) throw new ArgumentException("The symbolic expression exceeds the 1,024-token input budget.");
        var output = new StringBuilder();
        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (token.Kind == Kind.Name && Functions.Contains(token.Text) &&
                (index + 1 == tokens.Count || tokens[index + 1].Kind != Kind.Open))
                throw new ArgumentException("The function '" + token.Text + "' needs parenthesized arguments. Write sin²(x) or sin(x)² for a function power.");
            if (index > 0)
            {
                var previous = tokens[index - 1];
                var functionCall = previous.Kind == Kind.Name && token.Kind == Kind.Open && Functions.Contains(previous.Text);
                if (previous.Kind == Kind.Name && token.Kind == Kind.Open && !functionCall)
                    throw new ArgumentException("Unknown symbolic function '" + previous.Text + "'. Use an explicit · if multiplication was intended.");
                if (!functionCall && EndsFactor(previous) && StartsFactor(token)) output.Append(" * ");
                else if (!functionCall) output.Append(' ');
            }
            output.Append(token.Text);
        }
        return BoundedNormalization(output);
    }

    // Work on balanced calls after all superscripts have been decoded. Doing this
    // inside NormalizeCore would swap a nested logarithm twice when it occurs in
    // a function power. Arguments are delimited structurally, including matrices
    // and sets; commas in nested calls are never mistaken for an outer argument.
    private static string TranslateCalls(string text, int recursion)
    {
        if (recursion > 64) throw new ArgumentException("The symbolic expression is nested too deeply.");
        var output = new StringBuilder();
        for (var index = 0; index < text.Length;)
        {
            if (!IsNameCharacter(text[index])) { output.Append(text[index++]); continue; }
            var start = index++;
            while (index < text.Length && (IsNameCharacter(text[index]) || char.IsDigit(text[index]) || text[index] == '_')) index++;
            var name = text.Substring(start, index - start);
            var opening = index;
            while (opening < text.Length && char.IsWhiteSpace(text[opening])) opening++;
            if (opening == text.Length || text[opening] != '(' || !Functions.Contains(name))
            { output.Append(name); continue; }
            var closing = MatchingParenthesis(text, opening);
            var arguments = SplitArguments(text.Substring(opening + 1, closing - opening - 1))
                .Select(argument => TranslateCalls(argument, recursion + 1)).ToArray();
            if (name == "log" && arguments.Length == 2)
                output.Append("log(").Append(arguments[1]).Append(", ").Append(arguments[0]).Append(')');
            else if (name == "root")
            {
                if (arguments.Length != 2) throw new ArgumentException("root(value, index) requires two arguments.");
                var value = "(" + arguments[0] + ")";
                var degree = "(" + arguments[1] + ")";
                // TSL's real root overload preserves the real odd root for a
                // negative radicand; the complex principal power alone does not.
                var oddReal = value + " in RR and " + degree + " in ZZ and (" + degree + " mod 2) != 0";
                output.Append("piecewise(sgn(").Append(value).Append(") * abs(").Append(value)
                    .Append(")^(1/").Append(degree).Append(") provided ").Append(oddReal)
                    .Append(", ").Append(value).Append("^(1/").Append(degree)
                    .Append(") provided not (").Append(oddReal).Append("))");
            }
            else
            {
                name = name switch
                {
                    "tg" => "tan", "ctg" => "cotan", "arctg" => "arctan", "arcctg" => "arccotan",
                    "tgh" => "tanh", "ctgh" => "coth", "artgh" => "atanh", "arctgh" => "acoth", _ => name
                };
                output.Append(name).Append('(').Append(string.Join(", ", arguments)).Append(')');
            }
            index = closing + 1;
            if (output.Length > SymbolicEngine.MaximumResultCharacters)
                throw new ArgumentException("The normalized symbolic expression exceeds its size budget.");
        }
        return BoundedNormalization(output);
    }

    private static string BoundedNormalization(StringBuilder output)
    {
        if (output.Length > SymbolicEngine.MaximumResultCharacters)
            throw new ArgumentException("The normalized symbolic expression exceeds its size budget.");
        return output.ToString();
    }

    private static IEnumerable<string> SplitArguments(string text)
    {
        var depth = 0;
        var start = 0;
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] == '(' || text[index] == '[' || text[index] == '{') depth++;
            else if (text[index] == ')' || text[index] == ']' || text[index] == '}') depth--;
            else if (text[index] == ',' && depth == 0)
            { yield return text.Substring(start, index - start); start = index + 1; }
        }
        if (start != 0 || !string.IsNullOrWhiteSpace(text)) yield return text.Substring(start);
    }

    private static bool StartsFactor(Token token) => token.Kind == Kind.Name || token.Kind == Kind.Atom || token.Kind == Kind.Open;
    private static bool EndsFactor(Token token) => token.Kind == Kind.Name || token.Kind == Kind.Atom || token.Kind == Kind.Close || token.Kind == Kind.CollectionClose || token.Text == "!";
    private static bool IsNameCharacter(char character) => char.IsLetter(character) && !IsSuperscript(character);
    private static bool IsSuperscript(char character) => SpecialSymbols.SuperscriptsWithoutSpace.IndexOf(character) >= 0;
    private static int MatchingParenthesis(string text, int start)
    {
        var depth = 0;
        for (var index = start; index < text.Length; index++)
        {
            if (text[index] == '(') depth++;
            if (text[index] == ')' && --depth == 0) return index;
        }
        throw new ArgumentException("Unclosed function argument parentheses.");
    }

    // Parse decimal literals as decimal rationals, never as binary floating point or
    // a nearby guessed fraction. Scientific notation retains that same exactness.
    private static string ExactNumber(string raw)
    {
        var text = raw.Replace("_", "");
        var exponentIndex = text.IndexOfAny(new[] { 'e', 'E' });
        var exponent = 0;
        if (exponentIndex >= 0)
        {
            if (!int.TryParse(text.Substring(exponentIndex + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out exponent) || Math.Abs((long)exponent) > 1000)
                throw new ArgumentException("Scientific exponents must be between -1000 and 1000.");
            text = text.Substring(0, exponentIndex);
        }
        var point = text.IndexOf('.');
        var fractionalDigits = point >= 0 ? text.Length - point - 1 : 0;
        var digits = text.Replace(".", "").TrimStart('0');
        if (digits.Length == 0) return "0";
        if (digits.Length > 128) throw new ArgumentException("Numbers may contain at most 128 significant input digits.");
        var scale = fractionalDigits - exponent;
        return scale > 0 ? "(" + digits + "/1" + new string('0', scale) + ")" : digits + new string('0', -scale);
    }

    private enum Kind { Atom, Name, Open, Close, CollectionClose, Operator }
    private sealed class Token
    {
        internal Token(string text, Kind kind) { Text = text; Kind = kind; }
        internal string Text { get; }
        internal Kind Kind { get; }
    }
}
