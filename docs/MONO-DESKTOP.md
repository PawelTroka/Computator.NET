# Mono desktop and release packages

This opt-in target builds the current Computator.NET desktop and shared Engine
for Framework `net48`, then runs it on Framework Mono. The default Windows
application remains .NET 10 WinForms/WPF. Launching the Windows .NET 10 binary
with Mono is not supported. The portable CLI and local browser preview are
separate applications with their own documented subsets.

## Linux release candidate

The distribution is a runtime-dependent archive for **Ubuntu 22.04 x64** or
**Ubuntu 24.04 x64**. Choose the archive matching the operating system: the
private drawing library links to the baseline's native codecs, including its
specific libtiff ABI. Other Linux distributions and ARM are not certified by
these packages.

The archive includes the complete Mono application, Roslyn runtime compiler,
Framework compatibility facade DLLs, localization, TSL examples, all reference
content, GSL/CBLAS 2.8 and a private Pango-enabled libgdiplus 6.2. The manifest
records payload hashes, source commit/tree, native selections and dependency
provenance. Corresponding source, recipes and license notices accompany the
native libraries and bundled runtime facades.

End users need Mono 6.8 or newer, its listed Framework runtime assemblies
(including Mono.Posix), and the baseline's GUI/native prerequisites. They do
**not** need a .NET SDK, C# compiler, Node, npm, Python or system GSL. The
archive's `README.txt` contains the exact apt package list for that baseline.
Use a working X11 display or XWayland on a Wayland desktop, and run as your
ordinary desktop user.

After extraction into a permanent location:

```sh
./bin/computator --check
./bin/computator
# Optional per-user desktop-menu registration:
./bin/install-desktop-entry
```

The launcher verifies the full inventory and actual native imports before
starting the application. It generates a process-specific Mono configuration
outside the package and records actual selected library paths/hashes in
`${XDG_CACHE_HOME:-$HOME/.cache}/Computator.NET/runtime-check.json`. It does not
edit `/etc/mono/config`, replace system libraries or install files machine-wide.
Keep the extracted directory in place after registering its desktop entry.
The installed application payload can remain read-only; profiles, generated
configuration and acceptance evidence live elsewhere.

TSL/custom functions are trusted local code with the user's account permissions.
These packages do not add a sandbox or guarantee cancellation of arbitrary
desktop scripts.

## Building and validating Linux packages

Use the SDK pinned in `global.json` and the canonical Cake entry point. Package
builds additionally require Mono development/runtime assemblies, Python 3.10+,
the native compiler/build tools and development headers listed in
`.github/workflows/mono-linux-package.yml`. Xvfb, XTEST, xauth and xdotool support
the real headless display/input acceptance; they are not a replacement UI.

```sh
# Current sources, separate net48 outputs:
./build.sh --target=Mono-Build
# Source-tree startup and full shared GUI/numerical acceptance:
./build.sh --target=Mono-Validate
# Source-build the pinned native libraries, then create the matching archive:
./build.sh --target=Mono-Linux-Package
# Also extract and exercise that exact archive using its production DLLs/content:
./build.sh --target=Mono-Linux-Package-Validate
```

Configuration defaults to Release; add `--configuration=Debug` if needed.
Packages appear under `artifacts/mono-package/Release/`. Validation deliberately
requires new unpack/evidence directories; retain or move a previous run before
repeating it. `COMPUTATOR_MONO_PACKAGE_OUTPUT` and
`COMPUTATOR_MONO_PACKAGE_ARCHIVE` select custom package destinations for the
packaging leaf script; call `build/mono/validate-package.sh` with those explicit
paths when using overrides.

The source-built libgdiplus uses full, hash-pinned native test fixtures and a
small Pango private-font-map cache invalidation patch. All 36 native tests must
pass; missing fixtures and font failures are not excluded. GSL 2.8 is tested
against its own candidate prefix, rather than an accidentally discovered system
`gsl-config`. The matching sources, patches, build records and hashes remain in
the archive.

Hosted package acceptance also runs the same extracted payload read-only as a
normal user in a minimal Ubuntu runtime container without development tools.
It requires startup, nonblank actual GUI painting, normal shutdown, all shared
acceptance cases and an unchanged final package inventory. Container evidence is
bounded X11 acceptance; it does not certify a physical printer or every desktop
environment.

## Source builds and runtime boundaries

`ComputatorMono=true` propagates through the application project references.
`Directory.Build.props` separates `obj/mono/` and `bin/mono/` before restore/build
paths are derived. Windows and Mono builds do not reuse each other's assets.
The single SLNX links the scripts, sources and three opt-in probe projects;
those probes remain outside the default Windows project inventory.

```sh
dotnet build Computator.NET/Computator.NET.csproj -c Release -p:ComputatorMono=true
# Development output uses the host drawing/native loader configuration:
mono Computator.NET/bin/mono/Release/net48/Computator.NET.exe
```

For release acceptance use the archive launcher: a raw development executable
does not select the source-pinned private native payload automatically. In
particular, Ubuntu 22.04's libgdiplus 6.0.4 returned zero text widths after emoji
input and poisoned the editor's drawing context. The package avoids that stack;
Unicode checks remain mandatory. See the earlier
[dependency experiment](evidence/mono-drawing-2026-11-02.json).

| Boundary | Mono implementation | Windows implementation |
| --- | --- | --- |
| Runtime | Explicit Framework `net48` graph | .NET 10 desktop |
| Scientific functions/TSL | Current shared Engine, algorithms and catalogue | Same shared sources |
| Runtime compiler | Pinned Roslyn, compatible runtime metadata/facades, `Assembly.Load(byte[])` | Roslyn and collectible `AssemblyLoadContext` |
| UI | Mono WinForms/System.Drawing on X11 | Microsoft WinForms and WPF |
| Editor | Existing TROKA/ICSharpCode TextEditor | TextEditor, Scintilla and AvalonEdit |
| 2D/complex charts | Existing managed sources and Framework chart component | Existing Windows chart controls |
| 3D | WinForms software surface/point renderer using the same evaluator | WPF renderer |
| Reference | Managed rich rendering of the existing authored HTML fragments, shipped equation/diagram PNGs and selectable Text tab | Existing HTML reference presentation |
| Packaged native math | Source-pinned GSL/CBLAS 2.8 selected by process config | Reviewed Windows GSL payload/loader policy |
| Document safety | Shared atomic writes plus Mono.Posix physical file identity | Windows physical file identity |

No second scientific catalogue or scripting language is introduced. The compiler
supports the repository's C# syntax, but Mono scripts/plugins bind to Framework
4.8, not .NET 10-only runtime APIs. WPF and the advanced Windows editors are
excluded from this graph.

The rich reference uses the same descriptions and verified offline assets as
Windows/web: styled prose, signatures, inline and display equations, diagrams,
examples and notes. Image paths are confined to the shipped manifests and their
hashes/dimensions are checked before decode. External/file images are not fetched.
Source links remain inert. This implements the authored reference vocabulary;
it is not a general embedded web browser.

Document saves retain byte comparisons, flushed staging, atomic replacement and
backups. Mono.Posix supplies the runtime's OS-specific `fstat` layout rather than
guessing Linux/macOS native structures. Hard-link aliases are detected and unsafe
hard-link writes or external same-byte file replacements are refused. Case-only
paths referring to the same file on a case-insensitive volume can reopen; distinct
case-only files on a case-sensitive volume are refused because tab keys cannot
distinguish them. Symbolic-link overwrites remain rejected.

## macOS release preparation

The native Intel and Apple Silicon lanes are under active acceptance in
[#321](https://github.com/PawelTroka/Computator.NET/pull/321). A Linux cross-build
or a passing Mac portable-engine test does not certify the Mono GUI. Mac release
readiness requires successful native production startup, actual drawing/input,
the complete shared acceptance suite and the relocated application bundle on
each advertised architecture.

The implementation retains WinForms and selects its X11 driver through XQuartz,
with a matching private drawing/native dependency closure. It does not use the
legacy 32-bit Carbon driver, substitute the browser preview or introduce Cocoa/
another UI framework. Signed/notarized distribution and physical input-method,
clipboard, printing, accessibility and monitor/DPI acceptance remain separate
release gates. Consult the PR's exact-commit reports for current progress.

## Acceptance and remaining limits

The production startup probe invokes the real application entry point with its
own application directory/configuration, waits for the initialized main window,
captures actual native display pixels and closes normally. The separate shared
suite composes production services/controls and checks independent numerical,
document, image and real-input results. The packaged harness resides outside the
installation and verifies that production assemblies and reference content come
from the extracted application's directory.

As of the initial Linux candidate `bb3ee2a3`, Ubuntu 24.04 hosted package checks
passed all 36 native drawing tests, 52 extracted-package cases and 52 read-only
runtime-container cases, with no skips. An earlier Ubuntu 22.04 archive at
`be8b5509` passed the same two 52-case executions locally. These are dated
checkpoints, not evidence for later source changes. Configuration contracts also
execute the actual managed preflight and reject tampering, wrong architecture,
unrecorded files, unsafe mappings and writes back through redirected caches.
Use the final candidate's fresh workflow reports and archive hashes for release.

Coverage includes expressions, scripts, unsaved custom functions, native GSL and
recovery, numerical delegates, JSON/trusted NRBF migration, file identity,
Unicode editing, localized controls, 2D/complex charts, explicit 3D surfaces and
actual XTEST camera interaction. Offscreen `DrawToBitmap` output alone is not
evidence that a visible native window works.

Framework Mono cannot unload individual generated assemblies from the default
domain. Repeated compilation can grow memory until application exit. This target
does not promise .NET 10 collectible-load-context behavior.

The 3D software renderer supports explicit real `f(x,y)`, bounded meshes, point
series, axes, rotation, pan, zoom and PNG export. It uses a painter's algorithm;
intersecting geometry may order incorrectly. Implicit surfaces, hardware
acceleration, lighting and surface picking are unavailable. Slow trusted functions
can still block synchronous sampling. Symbolic calculations remain unavailable.

Before advertising a release, also test backed-up real profiles/documents,
long-running use, printing with a real configured CUPS printer, clipboard/IME,
accessibility, keyboard layout and display/DPI changes on the advertised desktop
systems. Automated evidence does not close those physical-machine gates.

The old `build-mono/` scripts are archival. `Install-Linux` remains an explicitly
disabled machine-wide installation task; the archive's per-user integration is
the supported packaging path. No Cake target implicitly signs, publishes or
notarizes a release.
