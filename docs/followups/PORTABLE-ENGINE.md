# Mac/Linux foundation: extract the language, then the rest of the engine

This PR is the first portable **production-code extraction**, not a claim that WinForms/WPF now run on Mac or that the whole scientific engine is finished. `Computator.NET.Engine` targets plain `net10.0` with no desktop UI, Windows drawing, GSL or old Mono dependencies. The existing TSL compiler, all of its handlers, mathematical notation and constants are compiled once into this assembly. Windows Core/DataTypes reference it and forward the old public type identities. Source files remain linked from their existing locations, avoiding a copied language implementation that would drift from the desktop.

A separate `Computator.NET.Portable.slnx` builds the engine, CLI and portable regression suite without restoring the Windows application graph. The original TSL syntax tests are linked unchanged into the portable suite, alongside actual execution/diagnostic/concurrency tests. The existing Windows suites remain mandatory to detect extraction regressions. `Portable-Validate` runs the tests and launches the published CLI on real Windows/Linux/macOS runners.

Examples:

```sh
./build.sh --target=Portable-Validate
dotnet artifacts/portable/computator.dll calculate --expression '2x+3' --x 7
dotnet artifacts/portable/computator.dll calculate --expression 'Probe(6)' --functions functions.tslf
dotnet artifacts/portable/computator.dll script --file example.tsl --functions functions.tslf
```

The CLI accepts UTF-8 documents, invariant numeric arguments, emits JSON results or document-relative compiler diagnostics, and uses nonzero exit status for invalid code. Generated assemblies use unique identities and collectible load contexts; no process-global cache stores another caller's function definitions. All code remains fully trusted. Collectible assemblies and output/source limits are **not a sandbox** and do not impose a computation timeout or filesystem/network isolation.

### Supported first slice

Real/two-variable and complex arithmetic, existing TSL rewrite syntax, scalar BCL functions (sin/cos/tan/sqrt/exp/ln/log/abs/pow/floor/ceil/round), mathematical constants, custom definitions and managed scripts with a supplied output callback. Scalar spelling adapters delegate to .NET's functions rather than introducing a new numerical algorithm.

### Boolean operators and absolute values

Conditions, scripts and custom functions support short-circuit logical OR chains such as `true || false || false` and `|x| > 0 || |y| > 0 || |x+y| > 0`. Later operands are evaluated only when the earlier operands are false. Single bars still denote absolute values; nested `||x||` and the supported adjacent expression `|x||y|` retain their mathematical meanings.

Double bars can be ambiguous when an absolute value sits between other expressions. Complete Boolean and comparison chains follow C# logical precedence: for Boolean variables `a` and `b`, `a||b||+z>1` means `a || b || (+z > 1)`. A numerical comparison previously written as `x||y||+z>1` must use explicit multiplication or function notation, for example `x*abs(y)+z>1`.

Whole arithmetic operands retain established nested-absolute-value forms, including `x||y||+z`, `x||y||*z` and `x||y||+z+1`. Prefer explicit multiplication and `abs(...)` when combining this notation with Boolean conditions. The parser classifies syntax before compiling types; it does not infer whether an arbitrary variable or function returns a Boolean or a number.

### Explicit remaining extraction

The existing full special-function/GSL catalog, matrix/numerical-method service, interactive `read` dialogs, settings/font/UI dependencies, and renderer are still in Windows Core. An unsupported name is a compiler diagnostic; no fabricated result or silent fallback pretends it exists. A browser opening on Mac while calling a Windows server is still remote access, not an offline Mac engine. Combine accepted GSL runtime assets from #143 with an explicit portable interop service before expanding native functions.

The next architectural steps are to move the existing mathematical services behind headless interfaces, retain common acceptance fixtures on all OSs, and then build an Avalonia/local-web prototype for notation editing, complex/3D plots and document workflows. This PR does not introduce a second full UI or replace the accepted Windows client prematurely. Mac architecture support is whatever actual CI reports; native ARM64 app packaging/signing/notarization are separate work.

The historical standalone Mono release/build sources remain archival; new support targets modern .NET, not an attempt to run the Windows desktop graph under old Mono. Framework-generated fixture writing in Windows CI remains only a legacy-input test utility. No release, package signing or platform support claim is made merely because a project compiles.
