# Precompile the mathematical surface, not every user's program

This change targets the large generated `ExpressionsEvaluator.Begin` body identified by issue #191. The build compiles mathematical `ToCode` bodies into facade types. A normal generated program inherits the appropriate facade and emits only its own function/custom definitions. Scripts keep scalar and matrix overloads together in a precompiled math surface; I/O, callbacks, profile/notation and additional-object code stay program-local.

There is no relaxed numerical tolerance, result/delegate cache, shared user assembly or background prewarming. Every compilation still creates a uniquely named collectible assembly. Cached metadata contains only application-owned nongeneric/open-generic definitions and strings, never closed user types.

## Native declarations and state

The initial generic-body prototype failed compilation with CS7042: native imports cannot be declared inside a generic type. Native mathematical surfaces are nongeneric and retain the original native import signatures and error guards. A structural state check admits only constants and readonly primitive/enum/decimal/Complex/string fields there. Pointers, arrays and other mutable static storage disable the precompiled path; authored nested types are checked too.

The check caught two real state boundaries during validation. The entire unchanged statistics fragment is in a generic facade closed separately with each generated program type: replaceable `rnd` and private `m_random` preserve their fields, with the latter still pointing to `Random.Shared` as before. User-defined fields and scripting callbacks remain in each distinct generated program.

The legacy SpecialFunctions fragment also contains a private static `sfResult` native scratch buffer, written and immediately read by `CoulombC`. Sharing that buffer would introduce cross-program races. The generator therefore performs one explicit, fail-closed locality transformation: remove that exact field and introduce an invocation-local variable immediately before its native out call. It requires the original three identifier occurrences and exact declaration/call shape. Native arguments, results, error handling and the mathematical algorithm do not change. The original ToCode is untouched; explicit access to its private `sfResult` forces the compatibility path. A separate hash records the transformed special-function source. Tests compare sequential legacy values and concurrent calls against the l=0 Coulomb normalization identity (NIST DLMF 33.2.6).

Native tests compare original and precompiled import attributes, calling conventions, parameters, struct packing/sizes/offsets and marshalling. Managed owner names differ, but native memory layouts must agree. Both native error policies and subsequent recovery remain tested.

## Compatibility and source fidelity

Custom class-member declarations colliding with any built-in name use the original full-source compiler, preserving merged overload groups and duplicate-signature errors. Private helper access, explicit dynamic declarations, type declarations and recognized generated-type/reflection inspection also use that path. Invalid or directive-bearing input falls back. The same selection is used by symbolic-power mode analysis and evaluation.

The build extracts existing single-verbatim-literal ToCode initializers, rejects unsupported initializer shapes, and generates intermediate source without a new downloaded executable or package. Input hashes are checked against runtime ToCode constants; a distinct output hash covers the explicitly localized Coulomb scratch. Other numerical bodies are unchanged. The existing unchecked arithmetic policy is required.

This internal representation change does not promise transparent arbitrary reflection into implementation details. Explicit generated-class inspection uses the old representation. Fallback cases intentionally do not receive the speedup. General desktop startup, the portable engine and API worker process startup are not optimized here.

## Acceptance

Correctness fixtures cover source fidelity, real/complex/surface/native independent oracles, custom overload compatibility, duplicate signatures, per-program static state and callbacks, seeded random isolation, concurrent Coulomb output, stale-definition rejection, native ABI fidelity and collectible lifetime. The Windows workflow requires these to pass, screens every historical workload, and then collects 20 sequential AB/BA process pairs against authenticated official 2.3.2 using the #235 harness. Failed samples remain available. The original 2.4 package is not authenticated: never label these 2.4 measurements.

A build or green test run is not proof of latency parity. Report first compilation, bounded-warm recompilation, cached evaluation and startup separately with exact source IDs and paired uncertainty intervals. Do not compare absolute timings from different runners. No merge is authorized.
