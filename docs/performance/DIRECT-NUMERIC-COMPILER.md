# Direct numeric expression compilation

Stacked on precompiled built-ins (#249) and ReadyToRun publication (#250). This change targets the remaining first-expression compiler latency, not a micro-allocation or a compiled-result cache.

## Change and strict scope

Before loading Roslyn for expression translation, a bounded parser recognizes a closed subset: ordinary ASCII real/complex coordinate expressions, finite decimal/scientific literals, parentheses, unary signs, +, -, *, real division, and a small allowlist of single-argument shipped numeric functions. It emits standard CLI metadata and IL through System.Reflection.Metadata and loads each image using the existing CollectibleProgramLoader. Each call still returns a fresh uniquely named collectible assembly with the existing FunctionsCreator/CustomFunction shape and generic per-program built-in base. There is no interpreter, hidden warmup, output/delegate cache or process-global root for generated programs.

The numeric implementation is NOT copied: calls reference the build-generated PrecompiledExpressionSurface, which is derived from the shipped ToCode bodies. Complex operators reference the actual runtime overloads. No algebraic reassociation, fast-math flags or approximate functions are introduced.

Everything outside the accepted grammar goes back to the existing compiler before any user code executes. This includes scripts, custom definitions, superscripts, implicit multiplication, square roots/dynamic results, symbolic powers, native/random calls, directives, member access, mutation, large integer literals and integer-only arithmetic with overflow/constant-folding rules. Parenthesized complex division retains the original TSL lowering through fallback. Integer -0 and floating -0.0 are distinguished. Specialized evaluator preambles disable the direct path. This is deliberately NOT a claim to make every TSL program as fast as the old product.

The path is bounded to 4,096 characters, 256 parser nodes and 64 nested unary/parenthesized levels; exceeding a bound invokes the normal compiler, not a truncated parse. Malformed/unsupported programs retain the normal error owner. Emit/load defects are not silently hidden by a fallback.

## Correctness and diagnosis

Differential tests compare against the same current source with the direct compiler disabled. Finite values, infinities and signed zero use exact double bit comparisons; NaN class is checked without requiring its unspecified payload bits. Separate tests cover independent analytic grids, custom definition changes, unsupported/invalid syntax, exact exception diagnostics, assembly uniqueness and collectible lifetime. The existing precompiled-facade tests continue to execute.

Protected `UseDirectExpressions` permits a specialized host or test to retain Roslyn. AppContext switch `Computator.Tsl.DisableDirectExpressions` is also available as an explicit diagnostic opt-out. `Function.TslCode` retains original input; `CsCode` on the direct path is a parenthesized equivalent C# numeric expression with promoted literals, not a byte-preserving copy of the full translator's formatting.

## Measurement gate

The dedicated Windows workflow screens all original benchmark workloads before measuring 20 sequential AB/BA process pairs. Two separate experiments are retained:

* Original, authenticated Framework 2.3.2.0 versus the new R2R candidate. This is not a relabeled 2.4.0.0 binary.
* The SAME modern product and driver bytes, with `--roslyn-only` versus the direct path. The common entry point sets the opt-out before the original worker's clocks; no product evaluation or compilation is performed as preparation. Native initialization, compilation, first invocation, warm recompilation and cached evaluation boundaries and correctness oracles remain unchanged.

The modern comparison omits desktop startup because an AppContext switch in the observer is not inherited by a child desktop. The historical comparison retains desktop startup. Source identity, full payload hashes, commands, runtime, hardware and cache conditions are recorded. Results are fresh-process, conditioned-profile and unflushed-filesystem-cache, not disk-cold measurements. Failed samples remain invalid. Do not merge or claim historical parity until completed results and regression coverage have been reviewed.

Implementation API references: https://learn.microsoft.com/dotnet/api/system.reflection.metadata.ecma335.instructionencoder and https://learn.microsoft.com/dotnet/api/system.reflection.portableexecutable.managedpebuilder .
