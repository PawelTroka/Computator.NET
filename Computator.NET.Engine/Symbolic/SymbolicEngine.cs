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
                case "simplify": result = TransformAlgebraically(expression, value => value.Simplify()); break;
                case "expand": result = TransformAlgebraically(expression, value => value.Expand().InnerSimplified); break;
                case "factor": result = TransformAlgebraically(expression, value => value.Factorize().InnerSimplified); break;
                case "differentiate": result = TransformAlgebraically(expression.Differentiate(variable, request.Order), value => value.Simplify()); break;
                case "integrate":
                    result = expression.Integrate(variable).InnerSimplified;
                    result = PreserveIntegralParameters(expression, MathS.Integral(expression, variable), result,
                        variable, warnings);
                    warnings.Add("An indefinite integral includes an arbitrary integration constant. It may depend on the other variables.");
                    break;
                case "definite-integrate":
                    var lower = Parse(request.LowerBound, token);
                    var upper = Parse(request.UpperBound, token);
                    if (lower.ContainsNode(variable) || upper.ContainsNode(variable))
                        throw new ArgumentException("Integral bounds must not contain the integration variable.");
                    result = expression.Integrate(variable, lower, upper).InnerSimplified;
                    result = PreserveIntegralParameters(expression, MathS.Integral(expression, variable, lower, upper), result,
                        variable, warnings);
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
                case "substitute": result = TransformAlgebraically(Substitute(expression, request, token), value => value.InnerSimplified); break;
                case "evaluate":
                    var substituted = Substitute(expression, request, token);
                    using (MathS.Settings.DowncastingEnabled.Set(false))
                        result = TransformAlgebraically(substituted, value => Approximate(value.Evaled));
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
                if (condition != Entity.Boolean.True)
                {
                    result = TransformAlgebraically(result.Provided(condition), value => value.InnerSimplified);
                }
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
        return SubstituteSimultaneously(expression, values, token);
    }

    private static Entity SubstituteSimultaneously(Entity expression, IReadOnlyDictionary<Entity.Variable, Entity> values, CancellationToken token)
    {
        // AngouriMath 2.6's dictionary overload is sequential. Fresh intermediate
        // symbols retain its binder/capture rules while making this operation
        // simultaneous: x→y,y→x swaps names, and replacements are not reprocessed.
        var reserved = new HashSet<string>(expression.VarsAndConsts.Select(variable => variable.Name), StringComparer.Ordinal);
        // Even an unused substitution key must not capture an intermediate name:
        // {x:y, ComputatorSubstitution_1:2} applied to x must still yield y.
        foreach (var key in values.Keys) reserved.Add(key.Name);
        foreach (var value in values.Values)
            foreach (var variable in value.VarsAndConsts) reserved.Add(variable.Name);
        var replacements = new List<KeyValuePair<Entity.Variable, Entity>>();
        var result = expression;
        var suffix = 0;
        foreach (var value in values)
        {
            token.ThrowIfCancellationRequested();
            string name;
            do name = "ComputatorSubstitution_" + (++suffix).ToString(CultureInfo.InvariantCulture); while (!reserved.Add(name));
            var temporary = MathS.Var(name);
            result = result.Substitute(value.Key, temporary);
            replacements.Add(new KeyValuePair<Entity.Variable, Entity>(temporary, value.Value));
        }
        foreach (var value in replacements)
        {
            token.ThrowIfCancellationRequested();
            result = result.Substitute(value.Key, value.Value);
        }
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
        foreach (var denominator in NonzeroDomainFactors(original))
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

    private static IEnumerable<Entity> NonzeroDomainFactors(Entity expression)
    {
        var denominators = new HashSet<Entity>();
        foreach (var node in expression.Nodes)
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
        return denominators;
    }

    private static Entity PreserveIntegralParameters(Entity integrand, Entity integral, Entity result,
        Entity.Variable variable, List<string> warnings)
    {
        var parameters = new HashSet<Entity.Variable>(integrand.Vars.Where(value => value != variable));
        Entity condition = Entity.Boolean.True;
        foreach (var factor in NonzeroDomainFactors(result))
            if (!factor.ContainsNode(variable) && factor.Vars.Any(parameters.Contains))
                condition &= !factor.Equalizes(0);
        condition = condition.InnerSimplified;
        if (condition == Entity.Boolean.True) return result;
        warnings.Add("The generic integral has parameter restrictions. Exceptional parameter cases retain the original integral; substitute parameter values before evaluating those cases.");
        // Keep the original *definite* integral and its bounds when supplied,
        // rather than substituting into a primitive singular at this parameter.
        // No simplification here: it would re-integrate the unspecialized branch.
        return new Entity.Piecewise(new[]
        {
            new Entity.Providedf(result, condition),
            new Entity.Providedf(integral, !condition)
        });
    }

    // A CAS simplifier may integrate every piecewise branch without adopting
    // that branch's condition. In the exceptional a=0 branch of exp(a*x), this
    // would recreate division by a and lose the valid value. Treat unresolved
    // conditional integrals as opaque during algebraic/numeric transformations;
    // restore their exact trees immediately afterwards. Once a substitution
    // resolves a branch condition, the ordinary integral evaluation is safe.
    // Do not use this to perform differentiation/integration on placeholders:
    // those operations depend on the variables inside the original integral.
    private static Entity TransformAlgebraically(Entity expression, Func<Entity, Entity> transform)
    {
        var reserved = new HashSet<string>(expression.VarsAndConsts.Select(variable => variable.Name), StringComparer.Ordinal);
        var originals = new Dictionary<Entity.Variable, Entity>();
        var suffix = 0;
        Entity.Providedf Protect(Entity.Providedf branch)
        {
            var (value, condition) = branch;
            if (condition.InnerSimplified is Entity.Boolean) return branch;
            var protectedValue = value.Replace(child =>
            {
                if (child is not Entity.Integralf) return child;
                string name;
                do name = "ComputatorConditionalIntegral_" + (++suffix).ToString(CultureInfo.InvariantCulture); while (!reserved.Add(name));
                var temporary = MathS.Var(name);
                originals.Add(temporary, child);
                return temporary;
            });
            return protectedValue == value ? branch : new Entity.Providedf(protectedValue, condition);
        }
        // Piecewise traverses its values/conditions directly: Providedf wrappers
        // are not in Nodes or visited by Replace, so handle its Cases explicitly.
        var protectedExpression = expression.Replace(node => node switch
        {
            Entity.Piecewise piecewise => new Entity.Piecewise(piecewise.Cases.Select(Protect)),
            Entity.Providedf branch => Protect(branch),
            _ => node
        });
        var result = transform(protectedExpression);
        return originals.Count == 0 ? result : result.Replace(node =>
            node is Entity.Variable variable && originals.TryGetValue(variable, out var original) ? original : node);
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

    private static bool TrySelectSystemBranches(Entity expression, out Entity selected)
    {
        selected = expression.Replace(node =>
        {
            if (node is Entity.Piecewise piecewise)
            {
                foreach (var branch in piecewise.Cases)
                {
                    var (value, condition) = branch;
                    var truth = condition.Simplify();
                    if (truth == Entity.Boolean.True) return value;
                    if (truth != Entity.Boolean.False) return node;
                }
                return MathS.NaN;
            }
            if (node is Entity.Providedf(var provided, var predicate))
            {
                var truth = predicate.Simplify();
                if (truth == Entity.Boolean.True) return provided;
                if (truth == Entity.Boolean.False) return MathS.NaN;
            }
            return node;
        });
        // Test the selected tree only: an unresolved condition in an inactive
        // branch must not prevent validating the active branch.
        return !selected.Nodes.Any(node => node is Entity.Piecewise || node is Entity.Providedf);
    }

    private static SymbolicResult SolveSystem(SymbolicRequest request, CancellationToken token)
    {
        var expressions = request.Equations.Select(text => Parse(text, token)).ToArray();
        var variables = request.Variables.Select(MathS.Var).ToArray();
        var equations = expressions.Select(expression => expression is Entity.Equalsf(var left, var right) ? left - right :
            expression is Entity.Statement ? throw new ArgumentException("A system must contain equations, not inequalities.") : expression).ToArray();
        var system = MathS.Equations(equations);
        SymbolicResult Unresolved(string warning) => new SymbolicResult
        {
            Result = string.Join(Environment.NewLine, expressions.Select(SymbolicOutput.Format)),
            Latex = system.Latexize(), Status = "unevaluated", Warnings = new[] { warning }
        };
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
            return Unresolved("The solver did not produce a complete explicit solution matrix. This does not prove that no solutions exist; dependent, inconsistent and parameter-dependent systems may require separate analysis.");
        if (solution.ColumnCount != variables.Length)
            return Unresolved("The solver returned an incomplete candidate row; the original equations are retained.");

        // Coordinates being real does not imply that the original real-domain
        // equations are defined there: sqrt(x)^2=-1 must not admit x=-1.
        // Likewise a cancelled denominator and a NaN coordinate cannot become
        // an exact solution. Validate against the original trees, not only the
        // transformed equations the algebraic solver worked with.
        var constraints = expressions.Select(expression =>
        {
            // Conjoining constraints from all piecewise branches would wrongly
            // reject valid values because of an inactive sqrt/log branch. If
            // the original branch is symbolic, derive its constraints after
            // substituting the candidate, when its condition may be decidable.
            if (!TrySelectSystemBranches(expression, out var selected)) return (Entity)Entity.Boolean.True;
            var condition = RealDomainCondition(selected, request.Domain == "real");
            foreach (var factor in NonzeroDomainFactors(selected)) condition &= !factor.Equalizes(0);
            return condition;
        }).ToArray();
        var rows = new List<Entity[]>();
        for (var row = 0; row < solution.RowCount; row++)
        {
            token.ThrowIfCancellationRequested();
            var values = Enumerable.Range(0, solution.ColumnCount).Select(column => solution[row, column]).ToArray();
            if (values.Any(value => value.Nodes.Any(node => node.IsNaN) || value.Evaled is Entity.Number.Complex number &&
                (!number.RealPart.IsFinite || !number.ImaginaryPart.IsFinite))) continue;
            var substitutions = variables.Select((variable, column) => new KeyValuePair<Entity.Variable, Entity>(variable, values[column]))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            Entity validity = Entity.Boolean.True;
            for (var index = 0; index < expressions.Length; index++)
            {
                var specialized = SubstituteSimultaneously(expressions[index], substitutions, token);
                if (!TrySelectSystemBranches(specialized, out var selected))
                    return Unresolved("A candidate's conditional branch could not be established. The original equations and their domain restrictions are retained.");
                if (selected.IsNaN) { validity = Entity.Boolean.False; break; }
                var condition = SubstituteSimultaneously(constraints[index], substitutions, token);
                // Substitution may reveal a domain restriction that was not
                // visible originally, e.g. y=1/2 turns x^y into an even root.
                // Inspect this original specialized tree before simplifying
                // its powers or equality, which could erase that restriction.
                condition &= RealDomainCondition(selected, request.Domain == "real");
                foreach (var factor in NonzeroDomainFactors(selected)) condition &= !factor.Equalizes(0);
                condition = condition.Simplify();
                if (condition == Entity.Boolean.False || condition.Nodes.Any(node => node.IsNaN))
                { validity = Entity.Boolean.False; break; }
                var equation = selected is Entity.Statement ? selected : selected.Equalizes(0);
                var verified = equation.Simplify();
                if (verified == Entity.Boolean.False || verified.Nodes.Any(node => node.IsNaN))
                { validity = Entity.Boolean.False; break; }
                if (verified != Entity.Boolean.True)
                {
                    // Equality simplification can leave 2*(2*t+6)=4*t+12.
                    // After validating the original domain, an identically zero
                    // expanded residual also proves a whole parameter family.
                    var residual = (selected is Entity.Equalsf(var left, var right) ? left - right : selected).Expand().Simplify();
                    if (residual == (Entity)0) verified = Entity.Boolean.True;
                }
                validity &= condition & verified;
            }
            validity = validity.InnerSimplified;
            if (validity == Entity.Boolean.False) continue;
            if (validity != Entity.Boolean.True)
                return Unresolved("One or more candidate rows could not be verified against the original equations and their domain restrictions. The original system is retained; this does not prove that no solutions exist.");
            if (request.Domain == "real" && !values.All(value => value.Evaled is Entity.Number.Real))
            {
                if (values.Any(value => value.Evaled is Entity.Number.Complex number && !number.ImaginaryPart.IsZero)) continue;
                return Unresolved("The reality of one or more system solutions could not be established. The original equations and their domain restrictions are retained.");
            }
            rows.Add(values);
        }
        Entity result = rows.Count == 0 ? Entity.Set.Empty : new Entity.Matrix(indices => rows[indices[0]][indices[1]], rows.Count, variables.Length);
        var warnings = new List<string>();
        if (rows.Count != 0) warnings.Add("Each row is a solution; columns follow the variable order: " + string.Join(", ", request.Variables) + ".");
        return Format(result, "exact", warnings, token);
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
