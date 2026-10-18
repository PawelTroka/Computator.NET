# Precompile the mathematical surface, not every user's program

This change targets the large generated `ExpressionsEvaluator.Begin` body identified by issue #191. The build compiles the exact current mathematical `ToCode` fragments into generic facade types. A normal generated program inherits the appropriate facade and emits only its own function/custom definitions. Scripts also keep scalar and matrix overloads together in a precompiled math facade; their I/O, callback, profile/notation and additional-object code stays program-local.

There is no numerical rewrite, relaxed tolerance, delegate/result cache, shared user assembly or background prewarming. Every compilation still creates a uniquely named collectible assembly. A distinct program type closes the generic facade so its static fields and nested types are not accidentally shared between programs. The only cached metadata contains application-owned open generic definitions and strings, not collectible user types.

## Compatibility

Moving an overload group to a base type can change C# name lookup. Custom declarations colliding with any built-in name therefore keep the original full-source compiler. Private helper access, explicit dynamic declarations, type declarations and recognized generated-type/reflection inspection also use that compatibility path. Invalid or directive-bearing policy input falls back rather than permitting new code or suppressing old diagnostics. The same selection is used by automatic symbolic-power mode analysis and evaluation.

This is an internal representation change, not a guarantee that arbitrary reflection into implementation details is indistinguishable. Explicit generated-class inspection uses the original representation. Fallback cases intentionally do not receive the speedup. General desktop startup, the portable engine and API worker process startup are not optimized by this change.

## Source fidelity

`build/PrecompiledTslBuiltins.targets` extracts the existing single-verbatim-literal ToCode initializers, fails on an unsupported initializer shape, and generates code in the intermediate build directory. It has no downloaded build-time executable or new package. Source hashes are verified against the actual runtime ToCode constants by the regression suite. The generator compiles the same numerical bodies, including native error boundaries and overload signatures; it does not substitute the separately maintained host versions. Compilation requires the existing unchecked arithmetic policy.

## Acceptance

The focused fixture compares original and precompiled execution, checks independent real/complex/surface oracles, custom overload compatibility, duplicate-signature errors, static state and callbacks, stale-definition rejection, and collectible lifetime. A new Windows workflow gates the measurement on correctness, screens every historical workload once, and then runs 20 sequential AB/BA process pairs against the authenticated official 2.3.2 binary using the #235 harness. Failed samples are retained. The original 2.4 package is still not authenticated, so 2.3.2 measurements must not be labeled 2.4.

A build or a green test run is not evidence of latency parity. Report the absolute first-compilation latency, bounded-warm recompilation, cached evaluation and startup separately, with exact source IDs and the paired uncertainty intervals. Do not compare absolute timings from different runners. No merge is authorized.
