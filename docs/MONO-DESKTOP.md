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
Windows/web: styled prose, inline and display equations, diagrams,
examples and notes. Image paths are confined to the shipped manifests and their
hashes/dimensions are checked before decode. External/file images are not fetched.
Source links remain inert. Signatures stay in autocomplete, following the shared
reference's existing rule against repeating them in the article. This implements the authored reference vocabulary;
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

Native builds use the repository's SDK/Cake pins, Python 3.11+, native Homebrew
for that architecture, Xcode command-line tools and the development prerequisites
in `build/macos/prepare.sh`. End users need the matching Mono runtime and XQuartz,
not those build tools or Homebrew drawing libraries. The source-pinned private
libgdiplus, GSL/CBLAS and non-system drawing dylib closure are bundled; the
launcher supplies private font/module configuration. The bundle's README and
Info.plist record its actual minimum macOS version, derived from the copied
Mach-O binaries with a floor of macOS 15.0.

```sh
# Check/build the private native stack without modifying global Mono dllmaps:
./build.sh --target=Mono-Mac-Prepare
# Optional explicit installation of missing build dependencies/XQuartz:
./build.sh --target=Mono-Mac-Prepare --mono-install-dependencies=true
# With an authenticated XQuartz DISPLAY:
./build.sh --target=Mono-Mac-Validate
./build.sh --target=Mono-Mac-Package-Validate
```

The last target requires source-native acceptance, produces
`artifacts/macos/package/Release/Computator.NET-mono-osx-arm64.zip` on Apple
Silicon (`osx-x64` on Intel), then validates a relocated, read-only `.app` in a
path containing spaces. It checks bootstrap files and Resources inventory,
runtime/native imports, actual production startup and the complete shared
suite. Test capture helpers stay outside the installation. On disposable CI,
original drawing candidates and build-host font/module data are temporarily
hidden and restored; process traces must select the packaged drawing closure.
The ordinary launcher never changes display modes or hides system dependencies.

The executable inside the bundle is `Contents/MacOS/Computator.NET`; its
`--preflight` option prints native dependency/architecture evidence without
starting the desktop. The `.app` can be moved as a whole. Rewritten dylibs carry
local ad-hoc integrity signatures; these do not identify an owner or establish
Gatekeeper/notarization acceptance.

### Hands-on acceptance on an Apple Silicon Mac

Use the accepted `osx-arm64` candidate on an M1/M2 or newer Mac. Record the
archive SHA-256, macOS version and `Contents/MacOS/Computator.NET --preflight`
JSON alongside the trial results. Read the bundle's actual OS/runtime
prerequisites first; test copied documents/profiles rather than the only copy
of real work.

- Launch the relocated `.app` from Finder and from a terminal. Quit normally,
  relaunch, and check settings/documents persist without a leftover application.
- Calculate `ProtonMuonMassRatio - 8.88024331`; the result must be exactly zero.
  Exercise ordinary real/complex calculations and one native special function.
- Load a `.tslf`, call its function in calculations and a script, change its
  definition without saving, then save/reopen and repeat. Verify errors highlight
  the displayed editor and recovery works after correcting the definition.
- Open function and constant help, including a long article and a diagram.
  Switch Description/Text, scroll, copy text and return to typing; open/close a
  modal dialog and ensure completion/help do not remain above it.
- Draw/export 2D and complex charts. Run
  `plot(new System.Func<double,double,double>[] { (x,y) => x*x+y*y });`
  and `plot(new double[] {0,1,2}, new double[] {0,1,0}, new double[] {0,1,4});`.
  Rotate/pan/zoom the 3D plot, resize it and compare its export.
- Test native file dialogs, Unicode filenames/text, clipboard, the installed
  input method, keyboard shortcuts, accessibility, Retina/other monitors and a
  real printer. Keep prolonged-use/memory observations with the exact candidate.

These physical checks complement the native hosted reports. They do not sign,
notarize or publish the candidate.

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

The current suite requires at least 54 cases, including restored compiled 3D
plot commands and exact decimal-input rounding. Mono's BCL and UTF8 parser can
round `8.88024331` one ULP high. The application's fast literal path and
`FromMathString` now retain their invariant grammar/exception behavior while
using the already-shipped Roslyn parser for correctly rounded finite values.
Stored/generated constants are unchanged. This adapter does not replace Mono's
BCL: an explicit `System.Double.Parse` call in user code retains that runtime's
behavior. Regression checks cover all 429 authored/emitted constants, signs,
negative zero, midpoint ties, normal/subnormal boundaries and regional cultures.

XQuartz rootless X11 readback can be black, clipped or inconsistent with the
native compositor. Mac acceptance therefore correlates the actual X11 window's
native ID and geometry with CoreGraphics, then crops a single real full-display
capture using measured pixel/point scales. It preserves popup occlusion and
rejects off-display requests. Calibration requires known colors and an actual
XTEST-triggered repaint. Linux retains its existing X11 screen readback. No
paint/input assertion is satisfied with a synthetic offscreen control image.

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
