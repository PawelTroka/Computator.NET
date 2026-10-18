# Precompile the mathematical surface, not every user's program

This change targets the large generated `ExpressionsEvaluator.Begin` body identified by issue #191. The build compiles the exact current mathematical `ToCode` fragments into facade types. A normal generated program inherits the appropriate facade and emits only its own function/custom definitions. Scripts keep scalar and matrix overloads together in a precompiled math surface; I/O, callbacks, profile/notation and additional-object code stay program-local.

There is no numerical rewrite, relaxed tolerance, delegate/result cache, shared user assembly or background prewarming. Every compilation still creates a uniquely named collectible assembly. Cached metadata contains only application-owned nongeneric/open-generic definitions and strings, never closed user types.

## Native declarations and state

The initial generic-body prototype failed compilation with CS7042: native imports cannot be declared inside a generic type. The native mathematical surfaces are now nongeneric and preserve the complete original import declarations, numerical bodies and native error guards unchanged.

A runtime structural check admits only constants and readonly primitive/enum/decimal/Complex/string fields in the shared surfaces. Pointers, arrays and other mutable static storage disable the precompiled path. Authored nested types are also checked. Compiler-generated call-site/delegate caches are runtime implementation details, not authored program state.

The state guard then correctly caught `StatisticsFunctions.rnd`, a public replaceable random generator. The entire unchanged statistics fragment is now in the generic facade layer, closed separately with each generated program type. Both replaceable `rnd` and the original private `m_random` retain their per-program fields; the latter still points to `Random.Shared` exactly as before. User-defined fields and scripting callbacks remain in each distinct generated program. Seeded-random tests explicitly verify that replacing or advancing one program's generator does not affect another program.

Native tests compare original emitted and precompiled import attributes, calling conventions, parameters, struct packing/sizes/field offsets and marshalling. Managed owner names differ, but native memory layouts must agree. An independent incomplete-gamma identity and both raw native error policies/recovery paths are tested.

## Compatibility

Moving an overload group to a base type can change C# name lookup. Custom class-member declarations colliding with any built-in name keep the original full-source compiler. Ordinary local variables and loop indices do not affect that class-member lookup. Private helper access, explicit dynamic declarations, type declarations and recognized generated-type/reflection inspection also use the compatibility path. Invalid or directive-bearing policy input falls back rather than suppressing old diagnostics. The same selection is used by automatic symbolic-power mode analysis and evaluation.

This is an internal representation change, not a guarantee that arbitrary reflection into implementation details is indistinguishable. Explicit generated-class inspection uses the original representation. Fallback cases intentionally do not receive the speedup. General desktop startup, the portable engine and API worker process startup are not optimized by this change.

## Source fidelity

`build/PrecompiledTslBuiltins.targets` extracts existing single-verbatim-literal ToCode initializers, fails on an unsupported initializer shape, and generates code in the intermediate build directory. It has no downloaded build-time executable or new package. Source hashes are verified against the actual runtime ToCode constants by the regression suite. Complete fragments are retained; only their containing classes differ. The generator does not substitute separately maintained host methods. Compilation requires the existing unchecked arithmetic policy.

## Acceptance

Focused fixtures compare original and precompiled execution, check independent real/complex/surface/native oracles, custom overload compatibility, duplicate-signature errors, static state and callbacks, seeded random isolation, stale-definition rejection, native ABI fidelity and collectible lifetime. The Windows workflow gates measurement on correctness, screens every historical workload once, and then runs 20 sequential AB/BA process pairs against the authenticated official 2.3.2 binary using the #235 harness. Failed samples are retained. The original 2.4 package is still not authenticated, so 2.3.2 measurements must not be labeled 2.4.

A build or a green test run is not evidence of latency parity. Report absolute first-compilation latency, bounded-warm recompilation, cached evaluation and startup separately, with exact source IDs and paired uncertainty intervals. Do not compare absolute timings from different runners. No merge is authorized.
