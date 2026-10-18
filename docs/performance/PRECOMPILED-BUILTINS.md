# Precompile the mathematical surface, not every user's program

This change targets the large generated `ExpressionsEvaluator.Begin` body identified by issue #191. The build compiles mathematical `ToCode` bodies into facade types. A normal generated program inherits the appropriate facade and emits only its own function/custom definitions. Scripts keep scalar and matrix overloads together in a precompiled math surface; I/O, callbacks, profile/notation and additional-object code stay program-local.

There is no relaxed numerical tolerance, result/delegate cache, shared user assembly or background prewarming. Every compilation still creates a uniquely named collectible assembly. Cached metadata contains only application-owned nongeneric/open-generic definitions and strings, never closed user types.

## Native declarations and state

Native mathematical surfaces are nongeneric because native imports cannot be declared inside generic types. Their import signatures and error guards are preserved. A structural state check admits only constants and readonly primitive/enum/decimal/Complex/string fields. Pointers, arrays and other mutable static storage disable this path; authored nested types are checked too.

The unchanged statistics fragment is in a generic facade closed separately with each generated program: replaceable `rnd` and private `m_random` preserve their fields, with the latter still pointing to `Random.Shared` as before. User fields and scripting callbacks remain program-local.

The state check also identified private static `sfResult`, written and immediately read by **CoulombC, CoulombGprime and CoulombFprime**. The generator removes that exact field from the precompiled body and declares a local native output buffer at each of its three native calls. It requires all seven original identifier occurrences, all three exact call shapes and three result reads. Native arguments (including the historical derivative order arguments), numerical algorithms and error policies are unchanged. Original ToCode remains untouched; explicit `sfResult` access forces the compatibility path. A separate hash records the transformed source. Sequential legacy comparisons and concurrent l=0 normalization/derivative identities exercise all three localized outputs.

Native ABI tests compare import attributes, calling conventions, parameters, struct packing/sizes/offsets and marshalling, not the managed owner name. Both native error policies and subsequent recovery remain tested.

## Compatibility and source fidelity

Custom class-member names colliding with built-ins use the original full-source compiler, preserving merged overload groups and duplicate-signature errors. Private helper access, dynamic declarations, type declarations and recognized generated-type/reflection inspection also use that path. Invalid or directive-bearing input falls back. Symbolic-power mode analysis uses the same policy as evaluation.

The build extracts existing single-verbatim-literal ToCode initializers, rejects unsupported shapes, and generates intermediate source without a new downloaded executable or package. Input hashes are checked against runtime constants; a separate output hash covers the audited scratch-locality change. Other numerical bodies are unchanged. The existing unchecked arithmetic policy is required.

This internal representation change does not promise transparent arbitrary reflection into implementation details. Explicit generated-class inspection uses the old representation. Fallback cases intentionally do not receive the speedup. General desktop startup, the portable engine and API worker process startup are not optimized here.

## Acceptance

Fixtures cover real/complex/surface/native independent oracles, source fidelity, overload compatibility, duplicate signatures, per-program static state/callbacks, seeded random isolation, concurrent Coulomb results, stale-definition rejection, native ABI fidelity and collectible lifetime. Windows measurement is gated on correctness and complete workload screening, then collects 20 sequential AB/BA process pairs against the authenticated official 2.3.2 binary. Failed samples are retained. The original 2.4 package is not authenticated: never label these 2.4 measurements.

Green builds alone do not establish latency parity. Report first compilation, bounded-warm recompilation, cached evaluation and startup separately, with source IDs and paired uncertainty intervals. Do not compare absolute timings from different runners. No merge is authorized.
