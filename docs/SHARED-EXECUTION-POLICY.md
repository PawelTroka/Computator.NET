# Shared execution validation — roadmap 2

Both JSON endpoints and legacy route adapters now use ExecutionPolicy for finite numbers, bounded source/output, whole iteration counts, derivative order/error limits, numerical-method lookup, chart dimensions/pixel count and ordered bounds. An MVC filter runs before controller execution, while direct JSON/chart controller entry points also use the same policy. Model-binding failures cannot silently become zero-valued calculations.

Legacy signed whitespace such as `-   1` is normalized only for numeric route parameters before binding. Source is still decoded from RawTarget exactly once. The same existing decoder is now applied to chart/numerical source segments. Programs are not globally unescaped or modified, and successful legacy responses remain text/images rather than being silently converted to the JSON response contract.

Both script routes use the same bounded output builder. Unknown methods and impossible chart allocations are rejected before compilation/rendering. The full-trust language itself is unchanged: these limits do not prevent a loop or arbitrary native/managed work inside a script. Process cancellation/isolation remains a separate roadmap change.

The normal Cake API suite adds real Kestrel tests comparing legacy/JSON validation, checking invalid bounds/dimensions/nonfinite values, exact output limits and error recovery. Existing numerical algorithms and tolerances are unchanged. The deliberate API policy no longer accepts fractional iteration counts or chart dimensions below 100; these now match the established JSON contract rather than silently allocating or truncating.

Review/merge after the protected launcher PR. This branch is independently based on master; its new transport tests already use a token and therefore remain compatible with protected-default startup. No additional project, solution, dependency or NuGet lockfile.
