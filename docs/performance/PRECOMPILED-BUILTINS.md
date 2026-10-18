# Precompile the mathematical surface, not every user's program

This change targets the large generated `ExpressionsEvaluator.Begin` body identified by issue #191. The build compiles the exact current mathematical `ToCode` fragments into facade surfaces. A normal generated program inherits the appropriate surface through a marker type and emits only its own function/custom definitions. Scripts keep scalar and matrix overloads together in a precompiled math surface; I/O, callbacks, profile/notation and additional-object code stay program-local.

There is no numerical rewrite, relaxed tolerance, delegate/result cache, shared user assembly or background prewarming. Every compilation still creates a uniquely named collectible assembly. The only cached metadata contains application-owned types and strings, never collectible user types.

## Native declarations and state

The initial generic-body prototype failed compilation with CS7042: native imports cannot be declared inside a generic type. The corrected surfaces are nongeneric and preserve every original import, numerical body and native error guard unchanged. Their tiny generic marker layers contain no native declarations or state.

Shared mathematical storage is admitted only after a runtime structural check: constants and readonly primitive/enum/decimal/Complex/string values are allowed; the existing private readonly `m_random` is allowed only if it is exactly `Random.Shared`, which the old generated code already uses. Other static storage automatically disables the precompiled path. The check recurses through authored nested types; compiler-generated call-site/delegate caches are runtime implementation details, not authored program state. User-defined fields and callbacks remain in each distinct generated program.

Native tests compare original emitted and precompiled import entry points, signatures, calling conventions and attributes, exercise incomplete gamma against an independent identity, and retain both native error policies and subsequent recovery.

## Compatibility

Moving an overload group to a base type can change C# name lookup. Custom class-member declarations colliding with any built-in name keep the original full-source compiler. Ordinary local variables and loop indices do not affect that class-member lookup. Private helper access, explicit dynamic declarations, type declarations and recognized generated-type/reflection inspection also use the compatibility path. Invalid or directive-bearing policy input falls back rather than suppressing old diagnostics. The same selection is used by automatic symbolic-power mode analysis and evaluation.

This is an internal representation change, not a guarantee that arbitrary reflection into implementation details is indistinguishable. Explicit generated-class inspection uses the original representation. Fallback cases intentionally do not receive the speedup. General desktop startup, the portable engine and API worker process startup are not optimized by this change.

## Source fidelity

`build/PrecompiledTslBuiltins.targets` extracts the existing single-verbatim-literal ToCode initializers, fails on an unsupported initializer shape, and generates code in the intermediate build directory. It has no downloaded build-time executable or new package. Source hashes are verified against the actual runtime ToCode constants by the regression suite. The generator compiles the same numerical bodies, including native error boundaries and overload signatures; it does not substitute the separately maintained host versions. Compilation requires the existing unchecked arithmetic policy.

## Acceptance

The focused fixtures compare original and precompiled execution, check independent real/complex/surface/native oracles, custom overload compatibility, duplicate-signature errors, static state and callbacks, stale-definition rejection, native ABI fidelity and collectible lifetime. The Windows workflow gates measurement on correctness, screens every historical workload once, and then runs 20 sequential AB/BA process pairs against the authenticated official 2.3.2 binary using the #235 harness. Failed samples are retained. The original 2.4 package is still not authenticated, so 2.3.2 measurements must not be labeled 2.4.

A build or a green test run is not evidence of latency parity. Report absolute first-compilation latency, bounded-warm recompilation, cached evaluation and startup separately, with exact source IDs and paired uncertainty intervals. Do not compare absolute timings from different runners. No merge is authorized.
