using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Compilation.TslHandlers;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Compilation
{
    /// <summary>
    /// A deliberately bounded numeric subset, not a replacement TSL parser.
    /// Emit ordinary IL and use the same collectible loader and shipped helpers.
    /// Unsupported syntax returns to the complete compiler BEFORE any execution.
    /// </summary>
    internal static class DirectExpressionCompiler
    {
        private const int MaximumLength = 4096;
        private const int MaximumNodes = 256;
        private const int MaximumNesting = 64;
        private static readonly Type Real = typeof(double);
        private static readonly Type ComplexType = typeof(Complex);
        private static readonly Type Surface = typeof(PrecompiledExpressionSurface);
        private static readonly HashSet<string> UnaryFunctions = new HashSet<string>(StringComparer.Ordinal)
        { "sin", "cos", "tan", "sinh", "cosh", "tanh", "exp", "ln", "log10", "abs", "Re", "Im" };

        // Automatic GUI routing must not load the full translator before the
        // direct compiler gets a chance. Classification ONLY parses; it never
        // emits an image, constructs a delegate, or executes a numerical call.
        internal static bool TryDetermineMode(string source, out CalculationsMode mode)
        {
            foreach (var candidate in new[] { CalculationsMode.Real, CalculationsMode.Fxy, CalculationsMode.Complex })
                if (TryParse(source, candidate, out _)) { mode = candidate; return true; }
            mode = CalculationsMode.Error;
            return false;
        }

        private static bool TryParse(string source, CalculationsMode mode, out Node root)
        {
            root = null;
            if (source == null || source.Length > MaximumLength ||
                (mode != CalculationsMode.Real && mode != CalculationsMode.Fxy && mode != CalculationsMode.Complex)) return false;
            if (AppContext.TryGetSwitch("Computator.Tsl.DisableDirectExpressions", out bool disabled) && disabled) return false;
            // No strings, comments-as-tokens, directives, member access, mutation,
            // powers, implicit multiplication or alternative Unicode notation.
            foreach (char c in source)
                if (!(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z') && !(c >= '0' && c <= '9') &&
                    c != '_' && c != ' ' && c != '\t' && c != '\r' && c != '\n' &&
                    c != '+' && c != '-' && c != '*' && c != '/' && c != '.' && c != '(' && c != ')') return false;
            if (source.Contains("++") || source.Contains("--")) return false;
            // Preserve the ACTUAL TSL promotion before parsing signs: x/-0
            // becomes x/-0.0, while x/(-0) promotes an already evaluated integer
            // zero. Reusing the handler also retains parenthesized denominators.
            // This handler uses regexes, not Roslyn's parser/binder/emitter.
            if (source.IndexOf('/') >= 0) source = new DivisionHandler().Replace(source);
            try { root = new Parser(source, mode).Parse(); return true; }
            catch (UnsupportedSyntax) { return false; }
        }

        internal static bool TryCompile(string source, CalculationsMode mode, out Delegate function, out string csharp)
        {
            function = null; csharp = null;
            if (!TryParse(source, mode, out var root)) return false;
            Type result = mode == CalculationsMode.Complex ? ComplexType : Real;
            if (root.Type != result)
            {
                if (root.Type != Real || result != ComplexType) return false;
                root = ConvertToComplex(root);
            }
            var parameters = mode == CalculationsMode.Complex ? new[] { ComplexType } :
                mode == CalculationsMode.Fxy ? new[] { Real, Real } : new[] { Real };
            // Emit/load failures are implementation defects, not reasons to hide a
            // broken fast path by silently succeeding through Roslyn.
            var assembly = new ImageWriter().Compile(root, result, parameters);
            Type delegateType = mode == CalculationsMode.Complex ? typeof(Func<Complex, Complex>) :
                mode == CalculationsMode.Fxy ? typeof(Func<double, double, double>) : typeof(Func<double, double>);
            function = Delegate.CreateDelegate(delegateType,
                assembly.GetType("FunctionsCreatorNS.FunctionsCreator", true).GetMethod("CustomFunction"));
            csharp = root.Code;
            return true;
        }

        private sealed class UnsupportedSyntax : Exception { }
        private static Node ConvertToComplex(Node node) => new Node
        {
            Type = ComplexType, Left = node,
            Method = ComplexType.GetMethod("op_Implicit", new[] { Real }), Code = node.Code
        };

        private sealed class Node
        {
            internal Type Type = Real;
            // Integral leaves are promoted only when C#/TSL would promote them.
            // Integer-only arithmetic is deliberately handed back to Roslyn.
            internal bool Integral;
            internal double? Constant;
            internal int Argument = -1;
            internal Node Left, Right;
            internal MethodInfo Method;
            internal char Operator;
            internal string Code;
        }

        private sealed class Parser
        {
            private readonly string _source;
            private readonly CalculationsMode _mode;
            private int _position, _nodes, _nesting;
            internal Parser(string source, CalculationsMode mode) { _source = source; _mode = mode; }
            private static bool Digit(char c) => c >= '0' && c <= '9';
            private static bool Letter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
            private char Peek()
            {
                while (_position < _source.Length && (_source[_position] == ' ' || _source[_position] == '\t' ||
                    _source[_position] == '\r' || _source[_position] == '\n')) _position++;
                return _position == _source.Length ? '\0' : _source[_position];
            }
            private Node Count(Node node) { if (++_nodes > MaximumNodes) throw new UnsupportedSyntax(); return node; }
            internal Node Parse()
            {
                Node node = Sum();
                if (Peek() != '\0') throw new UnsupportedSyntax();
                return node;
            }
            private Node Sum()
            {
                var left = Product();
                while (Peek() == '+' || Peek() == '-') { char op = _source[_position++]; left = Binary(op, left, Product()); }
                return left;
            }
            private Node Product()
            {
                var left = Unary();
                while (Peek() == '*' || Peek() == '/') { char op = _source[_position++]; left = Binary(op, left, Unary()); }
                return left;
            }
            private Node Unary()
            {
                if (++_nesting > MaximumNesting) throw new UnsupportedSyntax();
                try
                {
                    char c = Peek();
                    if (c != '+' && c != '-') return Primary();
                    _position++;
                    Node value = Unary();
                    if (value.Integral)
                    {
                        // C# -0 (integer) converts to positive double zero, unlike
                        // -0.0. Do not introduce negative zero by premature promotion.
                        int signed = checked((int)value.Constant.Value);
                        if (c == '-') signed = -signed;
                        return Number(signed, true);
                    }
                    if (value.Type == ComplexType)
                        return Count(new Node { Type = ComplexType, Left = value, Code = "(" + c + value.Code + ")",
                            Method = ComplexType.GetMethod(c == '-' ? "op_UnaryNegation" : "op_UnaryPlus", new[] { ComplexType }) });
                    return Count(new Node { Left = value, Operator = c, Code = "(" + c + value.Code + ")" });
                }
                finally { _nesting--; }
            }
            private Node Primary()
            {
                char c = Peek();
                if (c == '(')
                {
                    _position++; var node = Sum();
                    if (Peek() != ')') throw new UnsupportedSyntax();
                    _position++; return node;
                }
                if (Digit(c) || c == '.') return NumericLiteral();
                if (!Letter(c)) throw new UnsupportedSyntax();
                int start = _position++;
                while (_position < _source.Length && (Letter(_source[_position]) || Digit(_source[_position]))) _position++;
                string name = _source.Substring(start, _position - start);
                if (Peek() == '(')
                {
                    if (!UnaryFunctions.Contains(name)) throw new UnsupportedSyntax();
                    _position++; Node argument = Sum();
                    if (Peek() != ')') throw new UnsupportedSyntax();
                    _position++;
                    MethodInfo method = Surface.GetMethod(name, BindingFlags.Public | BindingFlags.Static, null, new[] { argument.Type }, null);
                    if (method == null || (method.ReturnType != Real && method.ReturnType != ComplexType)) throw new UnsupportedSyntax();
                    return Count(new Node { Type = method.ReturnType, Method = method, Left = argument, Code = name + "(" + argument.Code + ")" });
                }
                if (name == "x" || (name == "y" && _mode == CalculationsMode.Fxy))
                {
                    if (_mode != CalculationsMode.Complex) return Count(new Node { Argument = name == "x" ? 0 : 1, Code = name });
                }
                if (_mode == CalculationsMode.Complex)
                {
                    if (name == "z") return Count(new Node { Type = ComplexType, Argument = 0, Code = name });
                    if (name == "x" || name == "y")
                    {
                        var z = Count(new Node { Type = ComplexType, Argument = 0, Code = "z" });
                        return Count(new Node { Left = z, Method = Surface.GetMethod(name == "x" ? "Re" : "Im", new[] { ComplexType }),
                            Code = name == "x" ? "Re(z)" : "Im(z)" });
                    }
                }
                // Even constants are a closed set: no fields, static state,
                // reflection, custom definitions or native/random calls enter here.
                if (name == "pi" || name == "e")
                {
                    FieldInfo field = Surface.GetField(name, BindingFlags.Public | BindingFlags.Static);
                    if (field != null && field.IsLiteral && field.FieldType == Real) return Number((double)field.GetRawConstantValue(), false);
                }
                throw new UnsupportedSyntax();
            }
            private Node NumericLiteral()
            {
                int start = _position;
                bool digits = false, floating = false;
                while (_position < _source.Length && Digit(_source[_position])) { _position++; digits = true; }
                if (_position < _source.Length && _source[_position] == '.')
                {
                    floating = true; _position++;
                    int fractional = _position;
                    while (_position < _source.Length && Digit(_source[_position])) { _position++; digits = true; }
                    if (fractional == _position) throw new UnsupportedSyntax();
                }
                if (!digits) throw new UnsupportedSyntax();
                if (_position < _source.Length && (_source[_position] == 'e' || _source[_position] == 'E'))
                {
                    floating = true; _position++;
                    if (_position < _source.Length && (_source[_position] == '+' || _source[_position] == '-')) _position++;
                    int exponent = _position;
                    while (_position < _source.Length && Digit(_source[_position])) _position++;
                    if (exponent == _position) throw new UnsupportedSyntax();
                }
                if (_position - start > 64) throw new UnsupportedSyntax();
                string text = _source.Substring(start, _position - start);
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number) ||
                    double.IsNaN(number) || double.IsInfinity(number) || (!floating && number > int.MaxValue)) throw new UnsupportedSyntax();
                return Number(number, !floating);
            }
            private Node Number(double value, bool integral)
            {
                string code = value.ToString("R", CultureInfo.InvariantCulture);
                if (code.IndexOf('.') < 0 && code.IndexOf('E') < 0 && code.IndexOf('e') < 0) code += ".0";
                return Count(new Node { Constant = value, Integral = integral, Code = code });
            }
            private Node Binary(char op, Node left, Node right)
            {
                // Integer-only +/-/* has overflow and constant-folding rules
                // outside this subset. Numeric TSL division promotes to real.
                if (op != '/' && left.Integral && right.Integral) throw new UnsupportedSyntax();
                var node = new Node { Left = left, Right = right, Operator = op, Code = "(" + left.Code + op + right.Code + ")" };
                if (left.Type == ComplexType || right.Type == ComplexType)
                {
                    // Parenthesized complex division has an additional TSL
                    // promotion step with nonfinite edge cases: preserve it via Roslyn.
                    if (op == '/') throw new UnsupportedSyntax();
                    string name = op == '+' ? "op_Addition" : op == '-' ? "op_Subtraction" : "op_Multiply";
                    node.Method = ComplexType.GetMethod(name, new[] { left.Type, right.Type });
                    if (node.Method == null)
                    {
                        if (left.Type != ComplexType) node.Left = ConvertToComplex(left);
                        if (right.Type != ComplexType) node.Right = ConvertToComplex(right);
                        node.Method = ComplexType.GetMethod(name, new[] { ComplexType, ComplexType });
                    }
                    node.Type = ComplexType;
                }
                return Count(node);
            }
        }

        private sealed class ImageWriter
        {
            private readonly MetadataBuilder _metadata = new MetadataBuilder();
            private readonly Dictionary<Assembly, AssemblyReferenceHandle> _assemblies = new Dictionary<Assembly, AssemblyReferenceHandle>();
            private readonly Dictionary<Type, TypeReferenceHandle> _types = new Dictionary<Type, TypeReferenceHandle>();
            private readonly Dictionary<MethodInfo, MemberReferenceHandle> _methods = new Dictionary<MethodInfo, MemberReferenceHandle>();

            private TypeReferenceHandle Reference(Type type)
            {
                if (_types.TryGetValue(type, out var known)) return known;
                if (!_assemblies.TryGetValue(type.Assembly, out var assembly))
                {
                    var name = type.Assembly.GetName();
                    assembly = _metadata.AddAssemblyReference(_metadata.GetOrAddString(name.Name), name.Version,
                        string.IsNullOrEmpty(name.CultureName) ? default : _metadata.GetOrAddString(name.CultureName),
                        _metadata.GetOrAddBlob(name.GetPublicKeyToken() ?? Array.Empty<byte>()), (AssemblyFlags)0, default);
                    _assemblies.Add(type.Assembly, assembly);
                }
                var reference = _metadata.AddTypeReference(assembly, _metadata.GetOrAddString(type.Namespace), _metadata.GetOrAddString(type.Name));
                _types.Add(type, reference); return reference;
            }
            private void EncodeType(SignatureTypeEncoder encoder, Type type)
            {
                if (type == Real) encoder.Double();
                else if (type == ComplexType) encoder.Type(Reference(type), true);
                else throw new InvalidOperationException("Unsupported direct compiler type: " + type);
            }
            private BlobHandle Signature(Type result, Type[] parameters)
            {
                var blob = new BlobBuilder();
                new BlobEncoder(blob).MethodSignature().Parameters(parameters.Length,
                    r => EncodeType(r.Type(), result), p => { foreach (var type in parameters) EncodeType(p.AddParameter().Type(), type); });
                return _metadata.GetOrAddBlob(blob);
            }
            private MemberReferenceHandle Reference(MethodInfo method)
            {
                if (method == null || !method.IsStatic) throw new InvalidOperationException("A reviewed static numeric method is required.");
                if (_methods.TryGetValue(method, out var known)) return known;
                var reference = _metadata.AddMemberReference(Reference(method.DeclaringType), _metadata.GetOrAddString(method.Name),
                    Signature(method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray()));
                _methods.Add(method, reference); return reference;
            }
            private void Emit(Node node, InstructionEncoder il)
            {
                if (node.Constant.HasValue) { il.LoadConstantR8(node.Constant.Value); return; }
                if (node.Argument >= 0) { il.LoadArgument(node.Argument); return; }
                Emit(node.Left, il);
                if (node.Right != null) Emit(node.Right, il);
                if (node.Method != null) { il.Call(Reference(node.Method)); return; }
                if (node.Right == null) { if (node.Operator == '-') il.OpCode(ILOpCode.Neg); return; }
                il.OpCode(node.Operator == '+' ? ILOpCode.Add : node.Operator == '-' ? ILOpCode.Sub :
                    node.Operator == '*' ? ILOpCode.Mul : ILOpCode.Div);
            }
            internal Assembly Compile(Node root, Type result, Type[] parameters)
            {
                string identity = "Computator.Direct." + Guid.NewGuid().ToString("N");
                _metadata.AddModule(0, _metadata.GetOrAddString(identity + ".dll"), _metadata.GetOrAddGuid(Guid.NewGuid()), default, default);
                _metadata.AddAssembly(_metadata.GetOrAddString(identity), new Version(1, 0, 0, 0), default, default,
                    (AssemblyFlags)0, AssemblyHashAlgorithm.Sha256);
                Reference(typeof(object));
                // Keep the existing generated type and per-program generic facade
                // shape; load into a NEW collectible context exactly as Roslyn does.
                var baseSignature = new BlobBuilder();
                new BlobEncoder(baseSignature).TypeSpecificationSignature()
                    .GenericInstantiation(Reference(typeof(PrecompiledExpressionBuiltins<>)), 1, false)
                    .AddArgument().Type(MetadataTokens.TypeDefinitionHandle(2), false);
                var baseType = _metadata.AddTypeSpecification(_metadata.GetOrAddBlob(baseSignature));
                _metadata.AddTypeDefinition(TypeAttributes.NotPublic, default, _metadata.GetOrAddString("<Module>"), default,
                    MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));
                _metadata.AddTypeDefinition(TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
                    _metadata.GetOrAddString("FunctionsCreatorNS"), _metadata.GetOrAddString("FunctionsCreator"), baseType,
                    MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));
                var il = new InstructionEncoder(new BlobBuilder());
                Emit(root, il); il.OpCode(ILOpCode.Ret);
                var bodies = new BlobBuilder();
                int body = new MethodBodyStreamEncoder(bodies).AddMethodBody(il, maxStack: MaximumNodes * 2);
                for (int index = 0; index < parameters.Length; index++)
                    _metadata.AddParameter(ParameterAttributes.None, _metadata.GetOrAddString(parameters[index] == ComplexType ? "z" : index == 0 ? "x" : "y"), index + 1);
                _metadata.AddMethodDefinition(MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                    MethodImplAttributes.IL | MethodImplAttributes.Managed, _metadata.GetOrAddString("CustomFunction"),
                    Signature(result, parameters), body, MetadataTokens.ParameterHandle(1));
                var image = new BlobBuilder();
                new ManagedPEBuilder(new PEHeaderBuilder(imageCharacteristics: Characteristics.ExecutableImage | Characteristics.Dll),
                    new MetadataRootBuilder(_metadata), bodies, flags: CorFlags.ILOnly).Serialize(image);
                using (var stream = new MemoryStream(image.ToArray(), false)) return CollectibleProgramLoader.LoadProgram(stream);
            }
        }
    }
}
