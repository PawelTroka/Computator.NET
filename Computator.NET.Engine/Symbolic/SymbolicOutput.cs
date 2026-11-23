using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AngouriMath;
using AngouriMath.Core;
using Computator.NET.DataTypes.Text;

namespace Computator.NET.Engine.Symbolic;

/// <summary>Prints the algebra tree in the same notation as the TSL expression editor.</summary>
internal static class SymbolicOutput
{
    // Only mathematical nodes are translated. The library still owns the syntax
    // of sets, binders, matrices and conditions, with their children already
    // formatted. Unique whole-token placeholders avoid substring replacements
    // in user names and preserve the tree's binding structure.
    internal static string Format(Entity expression)
    {
        var fragments = new Dictionary<string, Fragment>(StringComparer.Ordinal);
        var reserved = new HashSet<string>(expression.VarsAndConsts.Select(variable => variable.Name), StringComparer.Ordinal);
        var suffix = 0;

        string Resolve(string text)
        {
            var result = new StringBuilder();
            for (var index = 0; index < text.Length;)
            {
                if (!char.IsLetter(text[index]) && text[index] != '_') { result.Append(text[index++]); continue; }
                var start = index++;
                while (index < text.Length && (char.IsLetterOrDigit(text[index]) || text[index] == '_')) index++;
                var name = text.Substring(start, index - start);
                result.Append(fragments.TryGetValue(name, out var fragment) ? fragment.Text : name);
            }
            return result.ToString();
        }

        Fragment Read(Entity node)
        {
            if (node is Entity.Variable variable && fragments.TryGetValue(variable.Name, out var fragment)) return fragment;
            var precedence = node is Entity.Providedf ? 0 : node is Entity.Statement ? 5 : 100;
            return new Fragment(Resolve(node.ToString()), precedence, node);
        }

        string Operand(Entity node, int precedence, bool equal = false)
        {
            var value = Read(node);
            return value.Precedence < precedence || equal && value.Precedence == precedence ? "(" + value.Text + ")" : value.Text;
        }

        Entity Save(Entity node, string text, int precedence, Domain? defaultDomain = null)
        {
            if (defaultDomain.HasValue && node.Codomain != defaultDomain.Value)
            {
                var set = node.Codomain == Domain.Any ? "Any" : Entity.Set.SpecialSet.Create(node.Codomain).ToString();
                text = "domain(" + text + ", " + set + ")";
                precedence = 100;
            }
            if (text.Length > SymbolicEngine.MaximumResultCharacters)
                throw new ArgumentException("The symbolic result exceeds 65,536 characters; use a smaller expression.");
            string name;
            do name = "ComputatorNativeOutput_" + (++suffix).ToString(CultureInfo.InvariantCulture); while (!reserved.Add(name));
            fragments.Add(name, new Fragment(text, precedence, node));
            return MathS.Var(name);
        }

        var result = expression.Replace(node =>
        {
            switch (node)
            {
                case Entity.Number.Complex number:
                    var printed = Number(number);
                    return Save(node, printed.Text, printed.Precedence);
                case Entity.Variable variable when variable.Name == "pi":
                    return Save(node, "PI", 100, Domain.Any);
                case Entity.Sumf(var left, var right):
                    return Save(node, Operand(left, 20) + " + " + Operand(right, 20, true), 20, Domain.Complex);
                case Entity.Minusf(var left, var right):
                    return Save(node, Operand(left, 20) + " - " + Operand(right, 20, true), 20, Domain.Complex);
                case Entity.Mulf(var left, var right):
                    return Read(left).Original == -1
                        ? Save(node, "-" + Operand(right, 40, true), 40, Domain.Complex)
                        : Save(node, Operand(left, 30) + " " + SpecialSymbols.DotSymbol + " " + Operand(right, 30, Read(right).Original is Entity.Modf), 30, Domain.Complex);
                case Entity.Divf(var left, var right):
                    return Save(node, Operand(left, 30) + " / " + Operand(right, 30, true), 30, Domain.Complex);
                case Entity.Modf(var left, var right):
                    return Save(node, Operand(left, 30) + " mod " + Operand(right, 30, true), 30, Domain.Real);
                case Entity.Powf(var value, var exponent):
                    var power = Read(exponent);
                    if (power.Original == MathS.Numbers.CreateRational(1, 2))
                        return Save(node, "sqrt(" + Read(value).Text + ")", 100, Domain.Complex);
                    var asciiPower = power.Text.Replace(SpecialSymbols.DotSymbol, '*').Replace(" ", "");
                    if (asciiPower.All(character => SpecialSymbols.AsciiForSuperscripts.IndexOf(character) >= 0))
                    {
                        // A whole compound exponent occupies one superscript run.
                        // A power used as a base must remain grouped: (x²)³.
                        if (power.Precedence < 40) asciiPower = "(" + asciiPower + ")";
                        return Save(node, Operand(value, 50, true) + SpecialSymbols.AsciiToSuperscript(asciiPower), 50, Domain.Complex);
                    }
                    return Save(node, "pow(" + Read(value).Text + ", " + power.Text + ")", 100, Domain.Complex);
                case Entity.Logf(var logarithmBase, var argument):
                    return Save(node, Read(logarithmBase).Original == MathS.e
                        ? "ln(" + Read(argument).Text + ")"
                        : "log(" + Read(argument).Text + ", " + Read(logarithmBase).Text + ")", 100, Domain.Complex);
                case Entity.Factorialf(var argument):
                    // Postfix ! is C#'s null-forgiving operator in TSL, not a factorial.
                    return Save(node, "factorial(" + Read(argument).Text + ")", 100, Domain.Complex);
                case Entity.Cardf(var argument):
                    return Save(node, "card(" + Read(argument).Text + ")", 100, Domain.Integer);
                case Entity.Cotanf(var argument):
                    return Save(node, "cot(" + Read(argument).Text + ")", 100, Domain.Complex);
                case Entity.Arccotanf(var argument):
                    // Backend arccotan(z) is atan(1/z), with pi/2 at zero.
                    // Calling this arccot(z) would silently select TSL's
                    // different pi/2 - atan(z) branch. Keep both the backend
                    // branch and its zero value explicit in symbolic output.
                    var cotangentArgument = "(" + Read(argument).Text + ")";
                    return Save(node, "piecewise(PI / 2 provided " + cotangentArgument + " = 0, arctan(1 / " +
                        cotangentArgument + ") provided " + cotangentArgument + " != 0)", 100, Domain.Complex);
                default: return node;
            }
        });
        return Resolve(result.ToString());
    }

    private static Fragment Number(Entity.Number.Complex number)
    {
        if (number is Entity.Number.Real real)
        {
            var text = real.IsNaN ? "NaN" : !real.IsFinite ? (real.IsNegative ? "-" : "") + SpecialSymbols.Infinity : real.ToString();
            return new Fragment(text, real is Entity.Number.Rational and not Entity.Number.Integer ? 30 : real.IsNegative ? 40 : 100, number);
        }
        var actualReal = Number(number.RealPart);
        var actualImaginary = Number(number.ImaginaryPart);
        var magnitude = actualImaginary.Text.StartsWith("-", StringComparison.Ordinal) ? actualImaginary.Text.Substring(1) : actualImaginary.Text;
        var imaginary = magnitude == "1" ? "i" : magnitude + " " + SpecialSymbols.DotSymbol + " i";
        if (number.RealPart.IsZero)
            return new Fragment((number.ImaginaryPart.IsNegative ? "-" : "") + imaginary, magnitude == "1" ? 40 : 30, number);
        return new Fragment(actualReal.Text + (number.ImaginaryPart.IsNegative ? " - " : " + ") + imaginary, 20, number);
    }

    private sealed class Fragment
    {
        internal Fragment(string text, int precedence, Entity original) { Text = text; Precedence = precedence; Original = original; }
        internal string Text { get; }
        internal int Precedence { get; }
        internal Entity Original { get; }
    }
}
