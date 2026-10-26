# Reduced Mono desktop

This opt-in target restores the historical reduced desktop alongside the default
.NET 10 Windows application. It builds the **current sources**, including the
shared Engine extracted in #146, with Framework `net48` references and runs them
on Framework Mono. It does not run the Windows `.NET 10` binary through Mono.

The implementation is related to [issue #209](https://github.com/PawelTroka/Computator.NET/issues/209).
No default solution target, Windows installer, release package or automatic
publication is switched to Mono.

## Build and run on Linux

Use the .NET SDK pinned in the repository's `global.json`. The SDK supplies the
current C# compiler for the build; Mono supplies the Framework runtime when the
application runs. The application also ships its NuGet Roslyn assemblies so
interactive TSL compilation does not invoke an installed SDK or `csc` process.

On Ubuntu 24.04, install the GUI and native runtime prerequisites:

```sh
sudo apt-get update
sudo apt-get install -y --no-install-recommends \
  mono-devel libgdiplus libgsl-dev libxtst6 fonts-dejavu-core xvfb xauth
```

`libgsl-dev` supplies the unversioned library name used by Mono's P/Invoke loader.
`libgdiplus` is used by Framework Mono's System.Drawing implementation. `libxtst6` supplies the XTEST input extension client used by the real-pointer 3D GUI acceptance. Xvfb and
xauth are needed for headless acceptance; normal interactive use requires an X11
display, or an XWayland display on a Wayland desktop.

From the repository root:

```sh
# Canonical Cake entry point: build and run the acceptance probes.
./build.sh --target=Mono-Validate

# Build only, through the same Cake graph.
./build.sh --target=Mono-Build

# The minimal direct MSBuild equivalent for the application alone.
dotnet build Computator.NET/Computator.NET.csproj \
  -c Release -p:ComputatorMono=true

# Start the built application on your desktop's display.
mono Computator.NET/bin/mono/Release/net48/Computator.NET.exe
```

Pass `--configuration=Debug` to Cake or `-c Debug` to `dotnet build` for a Debug
build. The compatibility leaf scripts also accept `Release` or `Debug`:

```sh
bash build/mono/build.sh Release
bash build/mono/validate.sh Release --no-build
```

`COMPUTATOR_DOTNET=/absolute/path/to/dotnet` selects an SDK executable for these
leaf scripts. Standard `DOTNET_CLI_HOME` and `NUGET_PACKAGES` overrides can be
used on restricted build hosts; they are not application runtime requirements.

The output directory must be kept intact. Besides the executable, it contains
the shared application assemblies, runtime compiler and numerical dependencies,
binding redirects, `DataVisualizationX.dll`, localization satellites and the
existing `Static` and `TSL Examples` content. Launch the executable with `mono`,
not `dotnet`.

For a separate trial profile, use the existing profile override:

```sh
COMPUTATOR_DATA_DIRECTORY="$PWD/artifacts/mono-trial-profile" \
  mono Computator.NET/bin/mono/Release/net48/Computator.NET.exe
```

The old `build-mono/` scripts remain archival. `Install-Linux` does not install
this build machine-wide. There is no Mono installer or signed application bundle
in this change.

## Shared source and dependency boundaries

`ComputatorMono=true` is a global MSBuild property and propagates through the
application's project references. `Directory.Build.props` selects separate
`obj/mono/` and `bin/mono/` directories before NuGet/MSBuild derive their paths.
Switching between Windows and Mono builds therefore does not reuse the other
target's restore assets, generated resources or output files.

The existing solution links the Mono probes and scripts as developer files.
Its default build project inventory remains unchanged: the two Framework-only
acceptance projects are built explicitly by `Mono-Validate`, and the solution
inventory guard checks that they remain discoverable without adding them to the
ordinary Windows build.

| Boundary | Mono implementation | Default implementation |
| --- | --- | --- |
| Runtime target | `net48`, selected explicitly | `net10.0-windows` for desktop; `net10.0` for Engine/CLI |
| TSL and scientific algorithms | Same Engine project and linked TSL, matrix, integral, derivative and root sources | Existing shared Engine |
| Runtime compiler | Current pinned Roslyn, Framework runtime metadata/facades, `Assembly.Load(byte[])` | Existing Roslyn and collectible `AssemblyLoadContext` |
| UI | Framework Mono WinForms/System.Drawing | Microsoft WinForms and WPF |
| 2D charts | Existing `libraries/net46/DataVisualizationX.dll` | Existing WinForms.DataVisualization package |
| Complex charts | Existing managed sampling, domain coloring and System.Drawing renderer | Same chart source |
| 3D charts | Bounded WinForms/System.Drawing software surface and point renderer with camera input, axes and PNG export | Existing WPF implementation |
| Code editor | Existing TROKA/ICSharpCode TextEditor | Existing Scintilla, AvalonEdit and TextEditor options |
| Function reference | Readable text from the existing reference, including equation alternative text | Existing HTML reference rendering |
| Native GSL | Host's `libgsl` via Mono P/Invoke | Existing validated Windows loader and package/fallback policy |
| Settings | Existing bounded JSON and data-only NRBF importer, with compatible packages | Existing .NET 10 importer |

The principal application package versions remain centralized and unchanged.
The opt-in graph adds Framework reference assemblies and compatible packages for
resource reading, runtime identification, JSON and NRBF records. It excludes
AvalonEdit, Scintilla5.NET, the modern chart package and WPF source/reference
items. Compatibility code adapts unavailable BCL APIs; it does not introduce a
second mathematical catalogue or a new scripting language implementation.

The shared compiler supports the repository's current C# 14 syntax, but scripts
and plugins in this target bind against Framework 4.8 and the application's
Framework-compatible assemblies. A newer compiler does not supply .NET 10-only
runtime APIs or WPF. External code that depends on those APIs or runtime-only
language features must be adapted before it can run on Mono.

The few mathematical BCL gaps have explicit narrow adapters. Adjacent-double
operations and sign copying preserve IEEE-754 bit semantics. Framework inverse
hyperbolic sine uses GSL's existing implementation, including very small and very
large real inputs. Modern builds continue to call their original BCL methods.

## Runtime limitations

### Generated assembly lifetime

Framework Mono cannot unload an individual compiled assembly from the application's
default domain. Expressions and scripts retain the existing unique assembly
identities and shared numerical types, but their generated assemblies remain
loaded until the application exits. Disposing an evaluation does not promise
modern collectible-load-context behavior. Every compilation receives a fresh
assembly identity, so repeated compilation can consume additional memory even
when the source is identical. Restart the compatibility application to release
it. The .NET 10 lifetime implementation is retained.

TSL keeps the application's existing trusted-code execution model. This target
adds neither sandboxing nor a cancellation/process-isolation guarantee.

### Native libraries

`GslConfig.GslDllName` becomes the logical name `gsl` in the Mono build. Both the
host wrapper and emitted expression/script P/Invokes use it. A mapping attached
only to `Computator.NET.Core.dll` would not cover generated assemblies with
unique names, which is why this target does not rely on such a `dllmap`.

The system GSL installation must match the Mono process architecture and be
visible to its native loader. `COMPUTATOR_GSL_DIRECTORY` selects the Windows
distribution format and is rejected explicitly on Mono. The Mono build does not
select the embedded historical Windows native fallback or stage a Windows GSL
package. Its native numerical results are validated against the actual host
library, separately from the existing pinned Windows GSL 2.8 distribution gate.

### Numerical acceptance

Acceptance covers the actual numerical-method delegates exposed by the desktop.
The original Mono acceptance discovered that Accord 3.8's scalar Monte Carlo
overload returned `1` for the constant function `f(x) = 2` over `[0, 1]`, instead
of the exact integral `2`. Its scalar loop increments the counter twice and uses
an integer random value for each sample coordinate.

The shared `Integral.monteCarloMethod` now adapts the scalar function to Accord's
multidimensional overload, which uses uniform coordinates and counts each sample
once. The same fix applies to generated TSL, with no package change. Acceptance
requires this case to pass alongside the other seven integral, five derivative
and five root methods, and additionally checks the generated expression. It no
longer treats the incorrect result as a known limitation. Nonconstant Monte Carlo
estimates still have stochastic sampling error; these checks do not certify every
numerical method or input as correct.

### Desktop behavior

Native Mono 3D uses the existing explicit real f(x,y) evaluator and presents a
WinForms software-rendered surface. It does not load WPF or require a GPU or
embedded browser. 3D charts are interactive on X11: **left-drag** rotates,
**mouse wheel** zooms, **right-drag** pans, **arrow keys** rotate, **+/-** zoom
and **Home** resets the camera. The plot includes labeled x/y/z numeric axes.
Camera changes and resize render cached samples without recompilation.

Sampling is synchronous and capped at **17–97 vertices per axis** (Quality 0–100),
**eight functions per chart**, **20,000 points per series** and **40,000 points total**.
The chart supports finite explicit real height surfaces, including compiled
user-defined two-argument functions, color-coded depth-sorted triangulation,
missing-value holes, point series, image export and normal edit/plot/print menus.
Invalid bounds and oversized exports fail explicitly rather than allocating
unbounded images. All-nonfinite surfaces display a diagnostic rather than a
misleading zero-height mesh. Sampling executes trusted local TSL without process
isolation; slow user functions can still block the UI while being sampled.

Limitations: this renderer uses a painter's algorithm, not a hardware z-buffer.
Overlapping/intersecting surfaces can have incorrect triangle ordering; performance
is limited compared with WebGL/WPF for dense meshes. Implicit F(x,y,z)=0 surfaces,
lighting, surface mouse picking and true hardware acceleration are not supported.
This is **native Linux/Mono** only; the separately developed WebGL browser viewer
in [PR #294](https://github.com/PawelTroka/Computator.NET/pull/294) is not reused
or bundled. The complex-chart defect [#295](https://github.com/PawelTroka/Computator.NET/issues/295)
remains a separate workstream.

Only TextEditor is offered on Mono. Its settings value remains `2`, matching the
Windows edition. Known historical/default editor values are normalized to the
available editor on Mono; invalid settings are still rejected. JSON persistence
and the safe record-based legacy importer remain in use.

Mono's WebBrowser did not populate a document on the validated Linux GUI stack.
The compatibility reference therefore uses a read-only text control. It preserves
the existing description and readable equation alternative text, without claiming
the Windows HTML/SVG reference presentation. Printing, clipboard integration,
monitor/DPI transitions, accessibility and the full editor interaction matrix
require additional manual acceptance on each advertised desktop environment.

Document loading/saving retains byte comparisons, flushed staging, atomic
replacement and backups. Mono rejects case-only path aliases that its
case-insensitive tab keys cannot distinguish. Symbolic-link overwrites are
rejected. Unix physical-file identity and hard-link counts do not currently have
the Windows native file-identity implementation; these builds do not claim that
stronger alias-detection guarantee.

## macOS status

No macOS GUI execution is certified by a Linux cross-build. The Mono framework
target is available for investigation on macOS, but this change's automated GUI
acceptance is Linux/X11. A maintained Mono runtime alone is insufficient evidence
that this WinForms application's macOS UI works.

Framework Mono 6.14.1's released driver source still selects Carbon for Darwin
unless an override is supplied, and its Carbon source explicitly identifies a
64-bit limitation. Its `MONO_MWF_MAC_FORCE_X11` override provides an X11 route to
investigate with XQuartz, matching-architecture Mono/libgdiplus/GSL and a working
`DISPLAY`. A future macOS acceptance run must test the exact machine architecture
and package combination, startup, editor workflows, native calculations and both
supported chart renderers. No Apple Silicon/Intel Mac parity, native Cocoa UI,
signing or notarization is claimed here.

## Reproducible acceptance and evidence

`Mono-Validate` builds the production application and two small Framework probes.
The startup probe invokes the real entry point with the application's own base
directory/configuration, waits for a visible initialized main window, captures
it and closes it. The separate acceptance executable composes production
services/controls and asserts independent numerical, document and image results.
It also rejects accidentally running on modern .NET or referencing the excluded
Windows UI assemblies.

The scripts use fresh temporary application profiles, time-limit both processes,
return a nonzero status for failures and write logs, screenshots and machine-readable
results under `artifacts/mono/Release/` (or `Debug/`). The dedicated Linux workflow
retains those results even when a check fails. Existing .NET 10 workflows remain
separate regression gates.

### Verified development environment

The restoration was built and exercised on Ubuntu 24.04.3 x64 with .NET SDK
10.0.402, Mono 6.8.0.105, libgdiplus 6.1, system GSL 2.7.1 and Xvfb.

| Development verification, 19 October 2026 | Observed result |
| --- | --- |
| `./build.sh --target=Mono-Validate` | Completed successfully; Framework build had zero errors and six existing warnings |
| Production entry point, own application directory and binding redirects | Main window rendered with visible inputs/menus, then closed normally |
| Mono acceptance report | 41 recorded cases: 40 passed, zero failed, one explicit inherited Monte Carlo limitation |
| Numerical method catalogue | Seven integral, five derivative and five root delegates passed independent known-answer checks; Monte Carlo recorded separately |
| Reference fallback | All 1,220 checked-in descriptions rendered; visible text, signature and equation alternatives checked |
| Direct executable with actual X11 keyboard/pointer events | Entered `x*x`, clicked Add to chart, observed a parabola and closed with exit code 0 |
| Default .NET 10 Engine regression suite | 605 passed, zero failed, zero skipped |
| Default .NET 10 Desktop.Tests and production dependency cross-build | Succeeded on Linux with Windows targeting; zero errors |

The suite also exercises expressions, scripts, custom functions, current C#
syntax, native GSL calls and recovery, compatibility math edge cases, JSON and
trusted NRBF migration, Unix document safety, visible editor/numerical workflows,
2D and complex PNG rendering, plus numerical 3D surfaces, custom functions,
missing samples, mode switching, clearing, exports and an actual visible X11
WinForms chart before/after a real XTEST pointer drag. The 3D acceptance retains
`mono-3d-visible-before.png`, `mono-3d-visible-rotated.png`,
`mono-3d-paraboloid.png`, `mono-3d-export.png` and `mono-3d-points.png`.
Its on-screen captures use actual X11 pixels; a successful offscreen render or blank `DrawToBitmap` capture is
not used as evidence of usable UI.

A clean hosted run also exposed an assembly-discovery ordering bug: filename
hints could match debug-symbol or configuration sidecars before the actual
assembly. Mono factory discovery now accepts `.dll` and `.exe` candidates only.
The regression creates matching `.pdb` and `.dll.config` files deliberately and
checks both the fallback factory and the ordinary production assembly hint.

Consult the pull request's exact-commit checks and retained workflow evidence for
subsequent CI results. The local Windows-targeted build is a cross-build, not a
Windows GUI execution. These results record one concrete Mono combination, not
every Linux distribution or newer runtime version.

## Source references

- [Historical Unix chart target before #139](https://github.com/PawelTroka/Computator.NET/blob/9fab48a5dc8e1b01366efb286c7b99acb8e93931/Computator.NET.Charting/Computator.NET.Charting.csproj)
- [Framework modernization #139](https://github.com/PawelTroka/Computator.NET/pull/139)
- [.NET 10 migration #140](https://github.com/PawelTroka/Computator.NET/pull/140)
- [Shared Engine and CLI extraction #146](https://github.com/PawelTroka/Computator.NET/pull/146)
- [Mono compatibility, including the WPF boundary](https://www.mono-project.com/docs/about-mono/compatibility/)
- [Framework Mono 6.14.1 driver selection](https://github.com/wine-mono/mono/blob/mono-6.14.1/mcs/class/System.Windows.Forms/System.Windows.Forms/XplatUI.cs)
- [Framework Mono 6.14.1 Carbon limitation](https://github.com/wine-mono/mono/blob/mono-6.14.1/mcs/class/System.Windows.Forms/System.Windows.Forms/XplatUICarbon.cs)
