# Direct numeric expression compilation

Stacked on precompiled built-ins (#249) and ReadyToRun publication (#250). This change targets the remaining first-expression compiler latency, not a micro-allocation or a compiled-result cache.

## Change and strict scope

Before loading Roslyn for expression translation, a bounded parser recognizes a closed subset: ordinary ASCII real/complex coordinate expressions, finite decimal/scientific literals, parentheses, unary signs, +, -, *, real division, and a small allowlist of single-argument shipped numeric functions. It emits standard CLI metadata and IL through System.Reflection.Metadata and loads each image using the existing CollectibleProgramLoader. Each call still returns a fresh uniquely named collectible assembly with the existing FunctionsCreator/CustomFunction shape and generic per-program built-in base. There is no interpreter, hidden warmup, output/delegate cache or process-global root for generated programs.

The numeric implementation is NOT copied: calls reference the build-generated PrecompiledExpressionSurface, which is derived from the shipped ToCode bodies. Complex operators reference the actual runtime overloads. No algebraic reassociation, fast-math flags or approximate functions are introduced.

Everything outside the accepted grammar goes back to the existing compiler before any user code executes. This includes scripts, custom definitions, superscripts, implicit multiplication, square roots/dynamic results, symbolic powers, native/random calls, directives, member access, mutation, large integer literals and integer-only arithmetic with overflow/constant-folding rules. Complex division retains the original TSL lowering through fallback. Integer -0 and floating -0.0 are distinguished. The existing DivisionHandler performs denominator promotion BEFORE the numeric parse, retaining the actual difference between x/-0 and x/(-0).

Specialized evaluator preambles disable the direct path. Subclasses retain their compiler by default because they may change NativeCompiler or its references without changing a preamble; they may explicitly override UseDirectExpressions to opt in. This is deliberately NOT a claim to make every TSL program as fast as the old product.

The path is bounded to 4,096 original characters, 256 parser nodes and 64 nested unary/parenthesized levels; exceeding a bound invokes the normal compiler, not a truncated parse. Malformed/unsupported programs retain the normal error owner. Emit/load defects are not silently hidden by a fallback.

## Actual automatic routing

The desktop's ModeDeterminer can classify the same subset without invoking the full TSL translator first. Classification parses but never compiles or executes a program. The shortcut is available only for the exact shipped TslCompiler with no custom definitions; injected/replaced language compilers retain their behavior. Tests check both equivalent coordinate classification and the absence of newly emitted assemblies.

This matters for user-visible first results: making Evaluate fast while leaving automatic mode detection slow would merely move the unmeasured bottleneck. Three separate auto-real, auto-complex and auto-surface scenarios include mode/compiler construction, automatic selection, compilation and the first result. They exclude window rendering; native engine initialization is separately recorded.

## Correctness and diagnosis

Differential tests compare against the same current source with the direct compiler disabled. Finite values, infinities and signed zero use exact double bit comparisons; NaN class is checked without requiring its unspecified payload bits. Separate tests cover independent analytic grids, custom definition changes, unsupported/invalid syntax, exact exception diagnostics, assembly uniqueness and collectible lifetime. A deterministic 128-program arithmetic/function corpus broadens coverage beyond the benchmark expressions. The existing precompiled-facade tests continue to execute.

The first Windows prototype passed 83/84 focused cases and 887/888 full Core cases. Its retained x/-0 assertion exposed an actual signed-zero bug. The correction reuses the existing denominator transform instead of relaxing the assertion. The initial failed run is retained at https://github.com/PawelTroka/Computator.NET/actions/runs/39226623323 .

Protected UseDirectExpressions permits specialized hosts/tests to select the appropriate path. AppContext switch Computator.Tsl.DisableDirectExpressions is also an explicit diagnostic opt-out. Function.TslCode retains original input; CsCode on the direct path is a parenthesized equivalent C# numeric expression with promoted literals, not a byte-preserving copy of the full translator's formatting. CsCode is C#, not a second TSL input requiring another division promotion.

## Measurement gate

The dedicated Windows workflow requires at least 232 focused regression cases and screens all declared published workloads before measuring 20 sequential AB/BA process pairs. Two separate experiments are retained:

* Original, authenticated Framework 2.3.2.0 versus the new R2R candidate. This is not a relabeled 2.4.0.0 binary.
* The SAME modern product and driver bytes, with --roslyn-only versus the direct path. The common entry point sets the opt-out before any workload clock; no product evaluation or compilation is performed as preparation. Original Worker.cs operation bodies and correctness oracles remain unchanged.

The modern comparison omits desktop startup because an AppContext switch in the observer is not inherited by a child desktop. The historical comparison retains desktop startup. Source identity, payload hashes, commands, runtime, hardware and cache conditions are recorded. Results are fresh-process, conditioned-profile and unflushed-filesystem-cache, not disk-cold measurements. Failed samples remain invalid. Do not merge or claim historical parity until completed results and regression coverage have been reviewed.

Implementation API references: https://learn.microsoft.com/dotnet/api/system.reflection.metadata.ecma335.instructionencoder and https://learn.microsoft.com/dotnet/api/system.reflection.portableexecutable.managedpebuilder .
