using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Budgets;
using AngouriMath.Core.Exceptions;
using PeterO.Numbers;

namespace Computator.NET.Engine.Symbolic;

/// <summary>
/// Shared computer algebra. This parses TSL mathematical notation as data,
/// without compiling or executing TSL/C# programs.
/// Cancellation is cooperative; a host that requires a hard deadline must isolate a worker.
/// </summary>
public sealed class SymbolicEngine
{
    public const int MaximumExpressionCharacters = 8192;
    public const int MaximumOrder = 20;
    public const int MaximumPrecision = 100;
    public const int MaximumResultCharacters = 65536;
    public const int MaximumMatrixDimension = 8;
    public static IReadOnlyList<string> Operations { get; } = Array.AsReadOnly(new[]
    {
        "simplify", "expand", "factor", "differentiate", "integrate", "definite-integrate",
        "limit", "solve", "substitute", "evaluate", "taylor", "solve-system",
        "determinant", "inverse", "transpose", "trace"
    });

    /// <summary>Cheap request checks, suitable before allocating an execution worker.</summary>
    public static void Validate(SymbolicRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (!Operations.Contains(request.Operation)) throw new ArgumentException("Unknown symbolic operation.", nameof(request.Operation));
        if (request.Domain != "real" && request.Domain != "complex") throw new ArgumentException("Domain must be real or complex.", nameof(request.Domain));
        if (request.Direction != "both" && request.Direction != "left" && request.Direction != "right")
            throw new ArgumentException("Limit direction must be both, left or right.", nameof(request.Direction));
        if (request.Order < (request.Operation == "taylor" ? 0 : 1) || request.Order > MaximumOrder)
            throw new ArgumentException("Order must be from 1 to 20 (Taylor degree may be zero).", nameof(request.Order));
        if (request.Precision < 1 || request.Precision > MaximumPrecision)
            throw new ArgumentException("Precision must be from 1 to 100 decimal digits.", nameof(request.Precision));
        if (!SymbolicInput.IsVariableName(request.Variable)) throw new ArgumentException("Variable must be a nonconstant symbolic name, such as x, theta or x_1.", nameof(request.Variable));
        SymbolicInput.ValidateText(request.Expression, nameof(request.Expression), request.Operation != "solve-system");
        SymbolicInput.ValidateText(request.Point, nameof(request.Point), request.Operation == "limit" || request.Operation == "taylor");
        SymbolicInput.ValidateText(request.LowerBound, nameof(request.LowerBound), request.Operation == "definite-integrate");
        SymbolicInput.ValidateText(request.UpperBound, nameof(request.UpperBound), request.Operation == "definite-integrate");
        if (request.Substitutions == null || request.Substitutions.Count > 32) throw new ArgumentException("At most 32 substitutions are supported.", nameof(request.Substitutions));
        if (request.Substitutions.Count > 0 && request.Operation != "substitute" && request.Operation != "evaluate")
            throw new ArgumentException("Substitutions apply to substitute and evaluate operations only.", nameof(request.Substitutions));
        var characters = request.Expression.Length + request.Point.Length + request.LowerBound.Length + request.UpperBound.Length;
        foreach (var item in request.Substitutions)
        {
            if (!SymbolicInput.IsVariableName(item.Key)) throw new ArgumentException("Each substitution key must be a nonconstant symbolic variable.", nameof(request.Substitutions));
            SymbolicInput.ValidateText(item.Value, nameof(request.Substitutions));
            characters += item.Key.Length + item.Value.Length;
        }
        if (request.Equations == null || request.Variables == null || request.Equations.Length > 8 || request.Variables.Length > 8)
            throw new ArgumentException("Systems support at most eight equations and eight variables.");
        if (request.Operation == "solve-system" && (request.Equations.Length == 0 || request.Variables.Length == 0))
            throw new ArgumentException("Provide at least one equation and one variable (at most eight of each).");
        if (request.Variables.Any(variable => !SymbolicInput.IsVariableName(variable)) || request.Variables.Distinct(StringComparer.Ordinal).Count() != request.Variables.Length)
            throw new ArgumentException("System variables must be distinct nonconstant symbolic names.", nameof(request.Variables));
        foreach (var equation in request.Equations)
        {
            SymbolicInput.ValidateText(equation, nameof(request.Equations));
            characters += equation.Length;
        }
        if (characters > MaximumExpressionCharacters * 2) throw new ArgumentException("The complete symbolic request exceeds 16,384 characters.");
    }

    public SymbolicResult Execute(SymbolicRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        cancellationToken.ThrowIfCancellationRequested();
        // A caller can reuse its mutable DTO while the request is running; snapshot it
        // before entering the library and never mutate either its text or collections.
        var snapshot = Copy(request);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(10));
        var context = ExecutionContext.Capture();
        if (context == null)
            return Task.Run(() => Run(snapshot, deadline.Token), deadline.Token).GetAwaiter().GetResult();
        SymbolicResult? result = null;
        // Angouri's cancellation token and settings are AsyncLocal. Running in a
        // captured context restores the caller's ambient state even after an error.
        ExecutionContext.Run(context, _ => result = Run(snapshot, deadline.Token), null);
        return result!;
    }

    private static SymbolicResult Run(SymbolicRequest request, CancellationToken token)
    {
        MathS.Multithreading.SetLocalCancellationToken(token);
        using var noNumericalRoots = MathS.Settings.AllowNewton.Set(false);
        using var domain = MathS.Settings.Codomain.Set(request.Domain == "real" ? Domain.Real : Domain.Complex);
        using var precision = MathS.Settings.DecimalPrecisionContext.Set(EContext.ForPrecision(request.Precision));
        using var explicitSyntax = MathS.Settings.ExplicitParsingOnly.Set(true);
        using var expansion = MathS.Settings.MaxExpansionTermCount.Set(2000);
        using var budget = MathS.Settings.Budget.Set(new WorkBudget { Steps = 100000, Time = TimeSpan.FromSeconds(8) });
        try
        {
            token.ThrowIfCancellationRequested();
            if (request.Operation == "solve-system") return SolveSystem(request, token);
            var expression = Parse(request.Expression, token);
            var variable = MathS.Var(request.Variable);
            var warnings = new List<string>();
            var status = "exact";
            Entity result;
            switch (request.Operation)
            {
                case "simplify": result = expression.Simplify(); break;
                case "expand": result = expression.Expand().InnerSimplified; break;
                case "factor": result = expression.Factorize().InnerSimplified; break;
                case "differentiate": result = expression.Differentiate(variable, request.Order).Simplify(); break;
                case "integrate":
                    result = expression.Integrate(variable).InnerSimplified;
                    warnings.Add("An indefinite integral includes an arbitrary integration constant. It may depend on the other variables.");
                    break;
                case "definite-integrate":
                    var lower = Parse(request.LowerBound, token);
                    var upper = Parse(request.UpperBound, token);
                    if (lower.ContainsNode(variable) || upper.ContainsNode(variable))
                        throw new ArgumentException("Integral bounds must not contain the integration variable.");
                    result = expression.Integrate(variable, lower, upper).InnerSimplified;
                    break;
                case "limit":
                    var point = Parse(request.Point, token);
                    if (point.ContainsNode(variable)) throw new ArgumentException("The limit point must not contain the approaching variable.");
                    result = expression.Limit(variable, point, request.Direction == "left" ? ApproachFrom.Left :
                        request.Direction == "right" ? ApproachFrom.Right : ApproachFrom.BothSides);
                    break;
                case "solve":
                    // The analytical solver can give a generic b/a without the a=0
                    // parameter cases. Retain the defining condition in that situation.
                    var equation = expression is Entity.Statement ? expression : expression.Equalizes(0);
                    var parameters = expression.Vars.Where(value => value != variable).ToArray();
                    if (parameters.Length != 0)
                    {
                        result = new Entity.Set.ConditionalSet(variable, equation & RealDomainCondition(expression, request.Domain == "real"));
                        status = "unevaluated";
                        warnings.Add("Parameter-dependent solutions are retained as conditions so degenerate parameter cases are not omitted. Substitute parameter values before solving for an explicit solution set.");
                    }
                    else result = PreserveEquationDomain(equation.Solve(variable), equation, variable, request.Domain == "real");
                    if (request.Domain == "real") result = MathS.Intersection(result, MathS.Sets.R).InnerSimplified;
                    if (result.Vars.Any(value => value.Name.StartsWith("n_", StringComparison.Ordinal)))
                        warnings.Add("Generated n_ parameters range over all integers.");
                    break;
                case "substitute": result = Substitute(expression, request, token).InnerSimplified; break;
                case "evaluate":
                    var substituted = Substitute(expression, request, token);
                    using (MathS.Settings.DowncastingEnabled.Set(false)) result = Approximate(substituted.Evaled);
                    status = "approximate";
                    if (result.Vars.Count != 0)
                    {
                        status = "unevaluated";
                        warnings.Add("Variables remain. Supply substitutions for a numerical value.");
                    }
                    break;
                case "taylor":
                    var center = Parse(request.Point, token);
                    if (center.ContainsNode(variable)) throw new ArgumentException("The Taylor center must not contain the expansion variable.");
                    result = MathS.Series.Taylor(expression, request.Order + 1, (variable, center)).InnerSimplified;
                    warnings.Add("This is the Taylor polynomial through degree " + request.Order.ToString(CultureInfo.InvariantCulture) + ", not the complete function or a certified remainder bound.");
                    break;
                case "determinant": result = (Matrix(expression).Determinant ?? throw new ArgumentException("A determinant requires a square matrix.")).InnerSimplified; break;
                case "inverse": result = (Matrix(expression).Inverse ?? MathS.NaN).InnerSimplified; break;
                case "transpose": result = Matrix(expression).T.InnerSimplified; break;
                case "trace":
                    var matrix = Matrix(expression);
                    if (matrix.RowCount != matrix.ColumnCount) throw new ArgumentException("A trace requires a square matrix.");
                    result = matrix.Trace.InnerSimplified;
                    break;
                default: throw new ArgumentException("Unknown symbolic operation.");
            }
            if (request.Domain == "real" && request.Operation != "solve" && request.Operation != "limit" && request.Operation != "definite-integrate")
            {
                var condition = RealDomainCondition(expression, true);
                if (request.Operation == "substitute" || request.Operation == "evaluate") condition = Substitute(condition, request, token);
                if (request.Operation == "taylor") condition = condition.Substitute(variable, Parse(request.Point, token));
                condition = condition.InnerSimplified;
                if (condition != Entity.Boolean.True) result = result.Provided(condition).InnerSimplified;
            }
            if (request.Domain == "real") result = RejectNonrealNumbers(result);
            if (request.Operation == "definite-integrate" && result is Entity.Number.Real real && !real.IsFinite && !real.IsNaN)
                warnings.Add("The improper integral diverges; this is not a finite area.");
            return Format(result, status, warnings, token);
        }
        catch (ParseException error) { throw new ArgumentException("Invalid symbolic expression: " + ShortMessage(error.Message), error); }
        catch (NotSufficientlySupportedException error)
        {
            // An unsupported algorithm is not an empty solution set or the number zero.
            return new SymbolicResult
            {
                Result = request.Operation == "solve-system" ? string.Join(Environment.NewLine, request.Equations) : request.Expression,
                Status = "unevaluated", Warnings = new[] { ShortMessage(error.Message) }
            };
        }
        catch (MathSException error) { throw new ArgumentException(ShortMessage(error.Message), error); }
    }

    private static Entity Parse(string text, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var parsed = MathS.FromString(SymbolicInput.Normalize(text), useCache: false);
        CheckTree(parsed, 2048);
        token.ThrowIfCancellationRequested();
        return parsed;
    }

    private static Entity Substitute(Entity expression, SymbolicRequest request, CancellationToken token)
    {
        var values = new Dictionary<Entity.Variable, Entity>();
        foreach (var substitution in request.Substitutions) values.Add(MathS.Var(substitution.Key), Parse(substitution.Value, token));
        // AngouriMath 2.6's dictionary overload is sequential. Fresh intermediate
        // symbols retain its binder/capture rules while making this operation
        // simultaneous: x→y,y→x swaps names, and replacements are not reprocessed.
        var reserved = new HashSet<string>(expression.VarsAndConsts.Select(variable => variable.Name), StringComparer.Ordinal);
        foreach (var value in values.Values)
            foreach (var variable in value.VarsAndConsts) reserved.Add(variable.Name);
        var replacements = new List<KeyValuePair<Entity.Variable, Entity>>();
        var result = expression;
        var suffix = 0;
        foreach (var value in values)
        {
            string name;
            do name = "ComputatorSubstitution_" + (++suffix).ToString(CultureInfo.InvariantCulture); while (!reserved.Add(name));
            var temporary = MathS.Var(name);
            result = result.Substitute(value.Key, temporary);
            replacements.Add(new KeyValuePair<Entity.Variable, Entity>(temporary, value.Value));
        }
        foreach (var value in replacements) result = result.Substitute(value.Key, value.Value);
        CheckTree(result, 4096);
        return result;
    }

    private static Entity Approximate(Entity expression) => expression.Replace(node =>
    {
        if (node is not Entity.Number.Complex value || value.IsNaN) return node;
        var context = MathS.Settings.DecimalPrecisionContext.Value;
        var real = Entity.Number.Real.Create(value.RealPart.EDecimal.RoundToPrecision(context));
        if (value.ImaginaryPart.IsZero) return real;
        return Entity.Number.Complex.Create(real, Entity.Number.Real.Create(value.ImaginaryPart.EDecimal.RoundToPrecision(context)));
    });

    private static Entity RejectNonrealNumbers(Entity expression)
    {
        if (expression is Entity.Number.Complex value && !value.ImaginaryPart.IsZero) return MathS.NaN;
        if (expression is Entity.Matrix matrix) return matrix.With((_, _, value) => RejectNonrealNumbers(value));
        return expression;
    }

    private static Entity PreserveEquationDomain(Entity.Set solutions, Entity original, Entity.Variable variable, bool realDomain)
    {
        Entity result = solutions;
        // A solver may cancel a common factor before finding roots. Its zeros are
        // still excluded from the original domain, including an identity x/x=1.
        var denominators = new HashSet<Entity>();
        foreach (var node in original.Nodes)
        {
            if (node is Entity.Divf(_, var denominator)) denominators.Add(denominator);
            if (node is Entity.Powf(var @base, var exponent) && TryNumber(exponent, out var power) && power <= 0) denominators.Add(@base);
            if (node is Entity.Logf(var logarithmBase, var argument))
            { denominators.Add(argument); denominators.Add(logarithmBase); denominators.Add(logarithmBase - 1); }
            if (node is Entity.Tanf(var tangent)) denominators.Add(MathS.Cos(tangent));
            if (node is Entity.Cotanf(var cotangent)) denominators.Add(MathS.Sin(cotangent));
            if (node is Entity.Secantf(var secant)) denominators.Add(MathS.Cos(secant));
            if (node is Entity.Cosecantf(var cosecant)) denominators.Add(MathS.Sin(cosecant));
        }
        foreach (var denominator in denominators)
            result = MathS.SetSubtraction(result, denominator.SolveEquation(variable)).InnerSimplified;
        result = result.Replace(node => node is Entity.Set.ConditionalSet(_, Entity.Boolean truth)
            ? truth.Value ? MathS.Sets.C : Entity.Set.Empty : node);
        var condition = RealDomainCondition(original, realDomain).InnerSimplified;
        if (condition != Entity.Boolean.True)
            result = MathS.Intersection(result, condition.Solve(variable)).InnerSimplified;
        if (result is Entity.Set.FiniteSet finite)
            result = finite.Filter(original, variable).InnerSimplified;
        return result;
    }

    private static Entity RealDomainCondition(Entity expression, bool realDomain)
    {
        Entity condition = Entity.Boolean.True;
        if (!realDomain) return condition;
        foreach (var node in expression.Nodes)
        {
            if (node is Entity.Logf(var @base, var argument))
                condition &= (@base > 0) & !@base.Equalizes(1) & (argument > 0);
            if (node is Entity.Powf(var radicand, var power) && power.InnerSimplified is Entity.Number.Rational exponent && !exponent.ERational.Denominator.Equals(EInteger.One))
            {
                // Real even roots need a nonnegative radicand. A reciprocal root
                // cannot be taken at zero. Odd-denominator powers remain real on
                // both signs; the library may retain them in principal-power form.
                if (exponent.ERational.Denominator.Remainder(2).IsZero)
                    condition &= exponent.ERational.Sign < 0 ? radicand > 0 : radicand >= 0;
            }
            if (node is Entity.Arcsinf(var sine)) condition &= (sine >= -1) & (sine <= 1);
            if (node is Entity.Arccosf(var cosine)) condition &= (cosine >= -1) & (cosine <= 1);
            if (node is Entity.Arcsecantf(var secant)) condition &= (secant <= -1) | (secant >= 1);
            if (node is Entity.Arccosecantf(var cosecant)) condition &= (cosecant <= -1) | (cosecant >= 1);
        }
        return condition;
    }

    private static Entity.Matrix Matrix(Entity expression)
    {
        var matrix = expression as Entity.Matrix ?? expression.InnerSimplified as Entity.Matrix;
        if (matrix == null) throw new ArgumentException("Enter a matrix, for example [[1,2],[3,4]].");
        if (matrix.RowCount > MaximumMatrixDimension || matrix.ColumnCount > MaximumMatrixDimension)
            throw new ArgumentException("Matrices are limited to 8 × 8 entries.");
        return matrix;
    }

    private static SymbolicResult SolveSystem(SymbolicRequest request, CancellationToken token)
    {
        var expressions = request.Equations.Select(text => Parse(text, token)).ToArray();
        var variables = request.Variables.Select(MathS.Var).ToArray();
        var equations = expressions.Select(expression => expression is Entity.Equalsf(var left, var right) ? left - right :
            expression is Entity.Statement ? throw new ArgumentException("A system must contain equations, not inequalities.") : expression).ToArray();
        var system = MathS.Equations(equations);
        var parameters = equations.SelectMany(equation => equation.Vars).Except(variables).Any();
        Entity.Matrix? solution = null;
        if (!parameters)
        {
            try { solution = system.Solve(variables); }
            catch (WrongNumberOfArgumentsException)
            {
                // The public input supports rectangular systems. Some library
                // algorithms only solve square systems; this is an unsupported
                // calculation, not malformed user data and not zero solutions.
            }
        }
        if (solution == null)
            return new SymbolicResult
            {
                Result = string.Join(Environment.NewLine, expressions.Select(SymbolicOutput.Format)), Latex = system.Latexize(), Status = "unevaluated",
                Warnings = new[] { "The solver did not produce a complete explicit solution matrix. This does not prove that no solutions exist; dependent, inconsistent and parameter-dependent systems may require separate analysis." }
            };
        Entity result = solution;
        if (request.Domain == "real")
        {
            var rows = new List<Entity[]>();
            for (var row = 0; row < solution.RowCount; row++)
            {
                var values = Enumerable.Range(0, solution.ColumnCount).Select(column => solution[row, column]).ToArray();
                if (values.All(value => value.Evaled is Entity.Number.Real)) rows.Add(values);
                else if (values.Any(value => value.Evaled is not Entity.Number.Complex))
                    return new SymbolicResult { Result = SymbolicOutput.Format(solution), Latex = solution.Latexize(), Status = "unevaluated",
                        Warnings = new[] { "The reality of one or more system solutions could not be established." } };
            }
            result = rows.Count == 0 ? Entity.Set.Empty : new Entity.Matrix(indices => rows[indices[0]][indices[1]], rows.Count, variables.Length);
        }
        return Format(result, "exact", new List<string>
        {
            "Each row is a solution; columns follow the variable order: " + string.Join(", ", request.Variables) + "."
        }, token);
    }

    private static SymbolicResult Format(Entity result, string status, List<string> warnings, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        CheckTree(result, 16384, enforceInputMatrixLimit: false);
        var nodes = result.Nodes.ToArray();
        if (nodes.Any(node => node.IsNaN))
        {
            status = "undefined";
            warnings.Add("The result is undefined or indeterminate for these inputs; it is not a finite numerical answer.");
        }
        else if (nodes.Any(node => node is Entity.Integralf || node is Entity.Derivativef || node is Entity.Limitf || node is Entity.Set.ConditionalSet))
        {
            status = "unevaluated";
            warnings.Add("Some operations or conditions remain unevaluated. An unevaluated result does not mean zero or no solutions.");
        }
        if (nodes.Any(node => node is Entity.Providedf))
            warnings.Add("The displayed conditions are part of the result and must be retained.");
        var text = SymbolicOutput.Format(result);
        var latex = result.Latexize();
        if (text.Length > MaximumResultCharacters || latex.Length > MaximumResultCharacters)
            throw new ArgumentException("The symbolic result exceeds 65,536 characters; use a smaller expression.");
        token.ThrowIfCancellationRequested();
        return new SymbolicResult { Result = text, Latex = latex, Status = status, Warnings = warnings.Distinct().ToArray() };
    }

    private static void CheckTree(Entity expression, int maximumNodes, bool enforceInputMatrixLimit = true)
    {
        var pending = new Stack<Tuple<Entity, int>>();
        pending.Push(Tuple.Create(expression, 0));
        var count = 0;
        while (pending.Count > 0)
        {
            var item = pending.Pop();
            if (++count > maximumNodes || item.Item2 > 128) throw new ArgumentException("The symbolic expression exceeds its node or depth budget.");
            var node = item.Item1;
            if (enforceInputMatrixLimit && node is Entity.Matrix matrix && (matrix.RowCount > MaximumMatrixDimension || matrix.ColumnCount > MaximumMatrixDimension))
                throw new ArgumentException("Matrices are limited to 8 × 8 entries.");
            if (node is Entity.Powf(_, var exponent) && TryNumber(exponent, out var power) && Math.Abs(power) > 1000)
                throw new ArgumentException("Numerical exponents are limited to an absolute value of 1,000.");
            if (node is Entity.Factorialf(var argument) && TryNumber(argument, out var factorial) && Math.Abs(factorial) > 500)
                throw new ArgumentException("Numerical factorial arguments are limited to an absolute value of 500.");
            if (node is Entity.Summationf(_, _, var sumFrom, var sumTo) && TryNumber(sumFrom, out var sumStart) && TryNumber(sumTo, out var sumEnd) && Math.Abs(sumEnd - sumStart) > 1000 ||
                node is Entity.Productf(_, _, var productFrom, var productTo) && TryNumber(productFrom, out var productStart) && TryNumber(productTo, out var productEnd) && Math.Abs(productEnd - productStart) > 1000)
                throw new ArgumentException("Explicit numeric sums and products are limited to 1,001 terms.");
            if (node is Entity.Primef(var primeIndex) && TryNumber(primeIndex, out var primeNumber) && Math.Abs(primeNumber) > 10000 ||
                node is Entity.Phif(var totientInput) && TryNumber(totientInput, out var totientNumber) && Math.Abs(totientNumber) > 100000000)
                throw new ArgumentException("The number-theory input exceeds the symbolic work budget.");
            if (node is Entity.Set.Powersetf(Entity.Set.FiniteSet elements) && elements.Count > 12)
                throw new ArgumentException("Explicit power sets are limited to twelve elements.");
            foreach (var child in node.DirectChildren) pending.Push(Tuple.Create(child, item.Item2 + 1));
        }
    }

    private static bool TryNumber(Entity value, out double number)
    {
        number = 0;
        if (value == MathS.e) { number = Math.E; return true; }
        if (value == MathS.pi) { number = Math.PI; return true; }
        switch (value)
        {
            case Entity.Number.Real real: number = (double)real; return true;
            case Entity.Sumf(var a, var b) when TryNumber(a, out var left) && TryNumber(b, out var right): number = left + right; return true;
            case Entity.Minusf(var a, var b) when TryNumber(a, out var left) && TryNumber(b, out var right): number = left - right; return true;
            case Entity.Mulf(var a, var b) when TryNumber(a, out var left) && TryNumber(b, out var right): number = left * right; return true;
            case Entity.Divf(var a, var b) when TryNumber(a, out var left) && TryNumber(b, out var right): number = left / right; return true;
            case Entity.Powf(var a, var b) when TryNumber(a, out var left) && TryNumber(b, out var right): number = Math.Pow(left, right); return true;
            default: return false;
        }
    }

    private static string ShortMessage(string message) => message.Length <= 1024 ? message : message.Substring(0, 1024);
    private static SymbolicRequest Copy(SymbolicRequest request) => new SymbolicRequest
    {
        Operation = request.Operation, Expression = request.Expression, Variable = request.Variable,
        Point = request.Point, Direction = request.Direction, Domain = request.Domain, Order = request.Order,
        Precision = request.Precision, LowerBound = request.LowerBound, UpperBound = request.UpperBound,
        Substitutions = new Dictionary<string, string>(request.Substitutions, StringComparer.Ordinal),
        Equations = (string[])request.Equations.Clone(), Variables = (string[])request.Variables.Clone()
    };
}
