# macOS application investigation and portable local preview

Original investigation: **28 October 2026**, with historical baseline `master` at
`fe1aba49b5f9538355ffaaa3d1683f69a23d2d9c`. Related objective:
[issue #292](https://github.com/PawelTroka/Computator.NET/issues/292).

**2 November update:** native Mono desktop release preparation now lives in
[#321](https://github.com/PawelTroka/Computator.NET/pull/321), with a private
X11/Pango drawing stack, GSL 2.8, native Intel/Apple Silicon acceptance and
runtime-dependent application bundles. Follow [MONO-DESKTOP.md](MONO-DESKTOP.md)
for current build/package commands and exact-commit acceptance status. The
browser-route recommendation below records the original investigation; its
portable preview remains a separate supported subset, not a replacement for the
requested Mono desktop. Neither a diagnostic nor an unaccepted bundle is a
certified native Mac release.

## Decision and scope

The portable preview now uses the shared Cake graph. `PortableWeb-Publish` calls
the audited `Web-Restore` task, selects the repository's Node/npm pins, builds the
separate portable frontend and publishes the host. `PortableWeb-Validate` adds
the real HTTP/process smoke checks and Chromium/WebKit acceptance on the native
Linux/macOS runner. The build wrapper below forwards to `PortableWeb-Publish`;
it no longer performs a separate unaudited npm installation.

```sh
bash ./build.sh --target=PortableWeb-Validate --portable-web-rid=osx-arm64
```

Use `osx-x64`, `linux-x64` or `linux-arm64` for the corresponding native host.
An RID cross-publish does not establish execution or GUI acceptance there.

The least disruptive promising route is a **portable local ASP.NET Core host
using the existing Engine and browser interface**. This change implements a
bounded proof of concept for that architecture. It establishes a way to run a
graphical mathematical workspace without a Windows server; it does **not**
establish full application parity or a certified Mac desktop release.

`Computator.NET.PortableWeb` targets `net10.0`, references only the existing
`Computator.NET.Engine` project, and serves the existing React/Vite application
on the same loopback origin as its API. It adds no numerical implementation or
native dependency. The Windows API, desktop, Mono target and release packaging
retain their existing roles. A deployment flag shows the preview's capability
boundary and hides the unused derivative tolerance and 3D quality inputs.

The original proof-of-concept branch was developed independently from
[web 3D #294](https://github.com/PawelTroka/Computator.NET/pull/294),
[native Mono 3D #296](https://github.com/PawelTroka/Computator.NET/pull/296), and their
[integration #300](https://github.com/PawelTroka/Computator.NET/pull/300).
The integrated repository now contains these workstreams together. Real `f(x)`
sampling is now shared with the Windows browser API, and the portable host
returns that numerical data to the existing browser renderer. The separate
Linux/Windows desktop renderers do not establish macOS desktop acceptance.

## Architecture comparison

| Question | Existing Mono desktop through XQuartz | Existing browser interface with portable local backend |
| --- | --- | --- |
| Existing application reuse | Reuses the Framework-compatible WinForms application, editor, documents, native calculations, 2D/complex renderers and integrated native software 3D renderer | Reuses React/Ace, browser document/custom-function workflows, TSL compiler and managed Engine mathematics |
| Current blocking boundary | Framework Mono's macOS driver defaults to Carbon; its source documents a 64-bit limitation. X11 is an alternative, with additional loader/drawing/display requirements | Full WebApi references Core, Charting, WinForms and WPF. Existing local-service lifetime code uses Windows job objects. Those dependencies cannot be removed by changing the target-framework string |
| Proof of concept | A separate diagnostic inspects the actual Mono-selected libraries, required X11 entry point, display and numerical calls; optional small visible-window probe | A separately published `net10.0` host actually serves the UI, executes portable mathematics in workers and returns explicit unsupported responses |
| Native dependencies | Matching Mono, X11-enabled libgdiplus, Cairo with Xlib support, GSL/CBLAS, XQuartz client libraries, fonts and a usable X11 display | Matching .NET 10 ASP.NET Core runtime and browser. No XQuartz, Mono, libgdiplus or native GSL for this subset |
| Graphical coverage | In principle retains more desktop behavior, but needs real Mac validation. Linux acceptance is not Mac evidence | Existing editor workflows and explicit real f(x) charts with overlays, pan/zoom and PNG export. Complex, surface and implicit charts remain outside the subset |
| Intel | Use x64 runtime and compatible native libraries; Carbon's limitation remains relevant | Publish `osx-x64`; execute on an Intel runner/Mac |
| Apple Silicon | XQuartz supports arm64, but every library loaded into Mono must match its process. An x64/Rosetta experiment is a separate stack, not arm64 certification | Publish `osx-arm64`; native .NET process avoids a Rosetta dependency for this POC |
| Maintenance | Requires ongoing ownership of the legacy WinForms/X11/native-library combination, potentially custom libgdiplus packaging, display/clipboard/fonts and Mac GUI regressions | Adds a small HTTP/lifetime adapter around maintained shared code. Full catalog and rendering extraction remain real work; keep contract tests as the Windows API evolves |
| Distribution still needed | Tested dependency bundle, launch/environment handling, application integration and signing/notarization decisions | User-friendly launcher/runtime packaging, supported browser matrix, application integration and signing/notarization decisions |

The browser route is selected because it isolates the known Windows dependency
boundary without replacing the mathematical engine or desktop controls. The
Mono route remains worth an explicitly scoped physical-Mac experiment, particularly
if retaining existing desktop workflows is a priority.

## A concrete XQuartz obstacle: drawing support, not only DISPLAY

Framework Mono 6.14.1's `XplatUI` selects X11 when
`MONO_MWF_MAC_FORCE_X11` is present; otherwise Darwin selects Carbon. Setting
that variable changes the driver, but does not supply its native dependencies.

The Homebrew `mono-libgdiplus` formula inspected on 28 October configures
libgdiplus with **`--without-x11`**. The `GdipCreateFromXDrawable_linux` entry
point used by Mono's X11 drawing path is compiled only with both `HAVE_X11` and
`CAIRO_HAS_XLIB_SURFACE`. Consequently, `brew install mono mono-libgdiplus gsl`
plus XQuartz is not a reproducible guarantee of drawable WinForms windows.
An offscreen bitmap or successful native library load would not establish this
capability either.

This is a source-level incompatibility finding, **not a reproduced failure on a
physical Mac**. Check the actual loaded library: another Mono distribution or a
custom X11-enabled build may behave differently. Do not replace machine-wide
Mono configuration or assume that a Homebrew prefix identifies the library Mono
actually loaded. The diagnostic below is designed to make that distinction.

XQuartz added native Apple Silicon support in 2.8.0. Its 2.8.6 release notes also
document fixes for black X11 surfaces on Apple Silicon, which reinforces the
need to retain exact version and visible-render evidence. On either architecture,
record `mono --version`, process architecture, native library paths/architectures,
XQuartz version, macOS version and display configuration. Mixing arm64 libraries
from `/opt/homebrew` with an x64 Mono process is not a valid combination; likewise
an arm64 process cannot load x64-only libraries. Universal binaries still require
the appropriate slice in every transitive dependency.

### Standalone Mac dependency probe

From an existing configured Mac terminal, with a working XQuartz `DISPLAY`:

```sh
bash build/macos/probe-xquartz.sh
bash build/macos/probe-xquartz.sh --gui
```

Use the script's `--help` for paths and environment overrides. It installs
nothing, uses a temporary compiled probe and does not modify system dllmaps.
The default probe requires macOS, the selected Framework Mono/compiler and the
actual native dependencies; failure is reported with a nonzero exit status.
The optional GUI probe checks a small visible WinForms calibration window.
**Neither result is acceptance of the Computator application itself.**

After resolving the actual native stack, the independent application experiment
starts with the existing opt-in build:

```sh
dotnet build Computator.NET/Computator.NET.csproj -c Release -p:ComputatorMono=true
MONO_MWF_MAC_FORCE_X11=1 \
  COMPUTATOR_DATA_DIRECTORY="$PWD/artifacts/mac-mono-trial-profile" \
  mono Computator.NET/bin/mono/Release/net48/Computator.NET.exe
```

Keep the complete output directory. Use the XQuartz-provided display and
authentication, rather than enabling unrestricted X server access. The probe's
`COMPUTATOR_MAC_LIBRARY_PATH` override applies only to its own process; if it was
needed, supply the same directories through command-level `DYLD_LIBRARY_PATH`
when launching the actual application. The existing
`build/mono/validate.sh` is a Linux/Xvfb suite and deliberately rejects macOS;
removing its platform check would not make its tests Mac acceptance.

## What the portable preview actually implements

| Capability | Implemented boundary |
| --- | --- |
| Local hosting | One numeric-loopback listener, same-origin frontend/API, no Windows host/proxy required |
| Real and two-variable calculation | Existing `PortableEngine.EvaluateReal`, with x and y |
| Complex calculation | Real compilation is attempted first; on compilation failure, existing `EvaluateComplex` uses `z = x + iy`. Results use the Engine's invariant complex representation |
| Scripts | Existing `ExecuteScript` and bounded `write(...)` output; trusted code with the local user's permissions |
| Custom functions | Existing unsaved definitions travel with each request; separate workers prevent definitions leaking into the next request |
| Mathematical tools | Existing managed scalar spellings, mathematical constants and matrix functions available through Engine |
| Numerical screen | Simpson integration, the existing automatically stepped derivative, and Brent root finding; unsupported method names fail explicitly |
| Documents | Existing browser `.tsl`/`.tslf` import/export, tab-local drafts and custom-function save workflows; no arbitrary server filesystem endpoint |
| Autocomplete | Metadata derived from the actual exported portable functions/constants; limited signature help, not the complete rich function reference |
| Charts | Explicit real f(x) curves with overlays/navigation, and f(x,y) height surfaces with bounded grid sampling and the existing interactive WebGL viewer. Complex and implicit expressions remain unsupported |
| Native functions | Full GSL/special-function catalog is unavailable here; pre-existing native build probes do not expose it through Engine |

The initial URL is `/calculate`, because the original browser application's `/`
screen is Chart. The preview notice remains visible on every screen. Charts
use this local managed backend; no Windows API is contacted. Symbolic
calculation is still an existing unfinished product feature.

Supported API contracts are `/api/v2/execute`, `/api/v2/chart`, the three
`/api/numerical-calculations/{operation}/list-methods` routes and the two
`/api/autocomplete/{expression|scripting}` routes. `/api/portable/status` reports
the actual OS/process architecture, operations, worker count and explicit
`chart: true`, `chartKinds: ["curve", "surface"]` and `nativeGsl: false` boundaries. Unknown API paths return 404, not
the SPA HTML.

### Real function charts

Open **Chart**, enter `sin(x)` or `x²`, and choose **Draw chart!**. Unsaved custom
definitions travel with the expression. **Add function** retains each layer's
submitted definitions; **Pan**, **Zoom** and **Reset view** resample those saved
layers. **Download PNG** exports the browser-rendered chart with its labels.
The 3D quality control is available for explicit height surfaces.

`POST /api/v2/chart` accepts `expression`, `customFunctions`, `xMin`, `xMax`,
`yMin`, `yMax`, `curveSamples` (default 1,025; range 33–4,097) and `allowEmpty2D`
(default false). Its JSON response has `kind: "curve"`, ordered `x` coordinates,
nullable `y` values and finite/missing sample counts. Each expression compiles
once in the same isolated worker used for other operations. The host verifies
the bounded response shape before sending it to the browser.

Undefined samples and arithmetic singularities leave gaps. The shared sampler
uses at most eight midpoint probes per interval to avoid drawing across detected
discontinuities; narrow/high-frequency features can still alias. Bounds and spans
must be finite, increasing and sufficiently separated to represent every sample.
A new chart with no finite samples returns 422. Viewport navigation sets
`allowEmpty2D` so an existing curve can remain visible as an empty layer and be
recovered by panning or resetting. Invalid expressions, unsupported functions,
complex variables return 400 with compilation diagnostics. Top-level
implicit equations (such as `x=1`) are rejected before compilation using the
desktop's syntax analysis; they cannot silently become assignment-based curves.
This addition does not expose the full special-function catalog or close the native desktop parity scope of #292.

### Real f(x,y) height surfaces

The local host compiles explicit `f(x,y)` once per chart and returns
`kind: "surface"` with x/y axes and row-major nullable `z[y][x]` samples,
using the same JSON schema and Plotly WebGL viewer as the Windows browser.
Default grid size is 65; valid sizes are 9–129 (at most 16,641 evaluations).
A 3D quality control selects 33/65/97. Non-finite values and arithmetic
singularities produce null holes, and fewer than three finite samples return
422. A worker-output shape check verifies bounded arrays, finite/ordered
coordinates and exact sample counts before the response reaches the browser.

The same loopback origin checks, two-worker limit, per-operation deadline,
worker cancellation, output cap and trusted-code warning apply to both chart
kinds. Without WebGL the browser reports its existing explicit rendering
error. Complex-domain coloring, implicit equations, full native mathematical
catalogs and native macOS/XQuartz desktop support remain separate work.

### Execution and local-access boundaries

Each request runs in a fresh process using the published application's own
worker entry point. At most two workers run; further simultaneous requests get
429. The default wall-time limit is ten seconds, configurable from one to sixty.
Client cancellation, deadline expiry and graceful server shutdown terminate and
reap owned workers. A successful follow-up operation is part of the smoke tests.
Charts and other operations share those two slots and the same deadline,
cancellation and shutdown cleanup. Request bodies are bounded to 256 KiB and
expression/script/custom-function text to 65,536 characters each. Numerical
work and worker pipe output are bounded too.

The listener is explicitly `127.0.0.1`; configurable Kestrel endpoints and URL
settings cannot extend it. Requests validate the actual numeric Host/port,
loopback peer, Origin and cross-site Fetch Metadata. Execution requires a JSON
POST. No CORS policy exposes the endpoint to other websites. Frame embedding is
disabled. These controls protect a trusted local service from accidental network
exposure and ordinary cross-site invocation.

This is **not a sandbox for untrusted scripts**. Code has the user's filesystem
and process permissions. Portable process-tree termination is not equivalent to
Windows kill-on-close jobs: malicious/detached descendants and abrupt parent
death are not strongly contained. Memory/CPU quotas and public multi-user
authorization are not supplied. Native AOT, trimming, single-file publication,
automatic installation and signed application packaging are outside the POC.

## Reproduce the independent local build

Prerequisites: the architecture-matching .NET SDK selected by `global.json`,
Node from `.nvmrc`, npm from the frontend `packageManager`, and Python 3 for
the smoke harness. No Mono or XQuartz is needed.

Run from the repository root:

```sh
# Install/select the pinned npm in your development environment if necessary.
npm install --global npm@11.19.0

# Apple Silicon, running a native arm64 terminal and .NET SDK:
bash build/portable-web/build.sh osx-arm64
bash build/portable-web/run.sh artifacts/portable-web/osx-arm64

# Intel alternative:
bash build/portable-web/build.sh osx-x64
bash build/portable-web/run.sh artifacts/portable-web/osx-x64
```

Open **http://127.0.0.1:5099/calculate**. Keep the terminal open and use Ctrl-C to
stop the host. `localhost` is intentionally not an accepted alternate Host.
`run.sh <directory> --port 5100` selects another port. The host also accepts port
0 for an OS-selected listener; its startup log records the resulting address.

`COMPUTATOR_DOTNET` and `COMPUTATOR_NPM` can select local tool executables.
The launcher uses the native published apphost, so the selected runtime must
match that apphost's architecture; it does not reinterpret an x64 publication
through an arm64 `dotnet` process.
The build script also accepts an output directory as its second argument.
Linux development validation uses `linux-x64` (or `linux-arm64`) in the same
commands. Cross-publishing a Mac RID on Linux proves packaging only.

The Cake task runs `npm ci`, the full npm audit, type checking and the existing Vite production build
with `VITE_API_BASE_URL=/api` and `VITE_PORTABLE_LOCAL=true`. It writes
`dist-portable` separately from the ordinary Windows frontend output, then
publishes the new host. The whole publication directory is required. This is a
framework-dependent application: the matching .NET 10 ASP.NET Core runtime must
be installed on the machine running it. An SDK is convenient for development;
Node/npm/Python are not application runtime dependencies. A direct `dotnet
publish` fails clearly when portable frontend assets have not been built.

### Headless HTTP/process smoke tests

Stop any manually launched host; the harness launches and stops its own native
apphost on an available port:

```sh
python3 build/portable-web/smoke.py \
  --publish-directory artifacts/portable-web/osx-arm64 \
  --expected-rid osx-arm64 --expected-architecture Arm64 \
  --output artifacts/portable-web-evidence/osx-arm64
```

For Intel use `osx-x64` and `X64`. The harness rejects Windows desktop dependency
graphs, verifies the actual process architecture, exercises known numerical
answers and tests input/error/cancellation/timeout/shutdown behavior. It records
JSON and server/runtime logs. This is stronger than a build or an HTTP health
check, but has no desktop GUI acceptance claim.

### Actual browser to published portable backend

```sh
cd Computator.NET.WebClient
npx playwright install chromium webkit
COMPUTATOR_PORTABLE_PUBLISH_DIRECTORY="$PWD/../artifacts/portable-web/osx-arm64" \
  npx playwright test --config playwright.portable.config.ts
```

Linux CI uses `playwright install --with-deps chromium webkit` to install browser
system dependencies. Tests use real published API responses, with no mocked
mathematics or Windows service. They exercise visible calculations, numerical
results, UTF-8 files, navigation/reload, unsaved definitions, real-curve values,
overlays, pan/reset, empty viewports, PNG export and script/chart cancellation.
Successful screenshots and failure traces accompany JUnit output.
These browser tests additionally exercise `dotnet <published DLL>` startup;
the selected dotnet determines process architecture for that form of launch.
CI supplies `COMPUTATOR_EXPECTED_ARCHITECTURE` and checks the status response;
set it to `Arm64` or `X64` when reproducing architecture-specific browser evidence.
WebKit automation is not the installed Safari application.

## Evidence interpretation and Mac acceptance still required

The dedicated `Portable local web proof of concept` workflow runs this same
publication/HTTP/browser sequence on `ubuntu-24.04`, **`macos-15-intel`/osx-x64**
and **`macos-15`/osx-arm64**. The explicit labels avoid treating a moving
`macos-latest` architecture as evidence for both. It retains separate artifacts
and leaves Windows release evidence and the original GUI suites intact. The
pull request records actual run URLs, tested commits, failures and final results;
the presence of workflow YAML alone is not evidence that it passed.

The original 28 October investigation ran on Linux x64 without a physical
Mac/display. Its local evidence covered the production frontend build, portable
dependency graph, published application execution and then-current Engine
regression suite. Current native hosted Mac results must be read as
execution on those exact runners. **A green hosted/headless result is not
physical Mac or complete desktop certification.**

Before advertising a usable supported Mac release, perform and record these
checks on **both** an Intel Mac and an Apple Silicon Mac:

1. Install/run the complete candidate from a clean account, record OS, CPU,
   native process architecture, runtime and browser versions, and verify no
   Windows process/service is involved. Test startup, port conflicts, Ctrl-C,
   restart and recovery from an interrupted session.
2. Use installed Safari and an advertised Chromium browser interactively.
   Verify keyboard shortcuts, Polish/non-ASCII/dead-key/IME input, selection,
   copy/paste, editor focus, navigation, tab drafts and multiple windows.
3. Import/export actual UTF-8 `.tsl` and `.tslf` files through OS dialogs; check
   names, contents, cancellation, overwrite/download behavior and unsaved work.
4. Inspect Retina/non-Retina rendering, zoom, resize, full screen, multiple
   monitors, keyboard navigation and VoiceOver. Preserve visible screenshots
   and observed input behavior rather than relying on a process exit code.
5. Verify real-curve sample correctness, interactions and export in the actual
   advertised browsers, including hardware/browser failures. Complex and 3D
   charts need their own implementation and acceptance before being advertised.
6. For a Mono claim, first pass the native/display probe, then run the actual
   Computator GUI and its editor, documents, native GSL and supported chart
   workflows. Capture visible XQuartz pixels and real input. Verify clipboard,
   fonts, menus/dialogs and shutdown. Test x64/Rosetta separately if advertised.

Future parity work should extract the full numerical/catalog surface and
matching native GSL loading, and extend portable sampling to complex and
surface data. Sharing the bounded real-curve sampler and existing browser
renderer does not make the full Windows API portable. Rendering/document
behavior and distribution remain release gates. This proof of concept must
not close #292 or authorize any merge.

## Primary source references

- [Current Mono desktop boundaries](MONO-DESKTOP.md)
- [Portable Engine scope](followups/PORTABLE-ENGINE.md)
- [Windows API dependency graph](../Computator.NET.WebApi/Computator.NET.WebApi.csproj)
- [Existing Windows local-service lifetime](../Computator.NET.WebClient/Hosting/LocalWebSession.cs)
- [Mono 6.14.1 driver selection](https://github.com/wine-mono/mono/blob/mono-6.14.1/mcs/class/System.Windows.Forms/System.Windows.Forms/XplatUI.cs)
- [Mono 6.14.1 Carbon limitation](https://github.com/wine-mono/mono/blob/mono-6.14.1/mcs/class/System.Windows.Forms/System.Windows.Forms/XplatUICarbon.cs)
- [Homebrew mono-libgdiplus formula](https://github.com/Homebrew/homebrew-core/blob/main/Formula/m/mono-libgdiplus.rb), inspected blob `837e7b5f75ac88c84f8a08e10c7da03ff4cabfb4`
- [libgdiplus X11 entry point](https://github.com/mono/libgdiplus/blob/main/src/graphics.c), inspected blob `44685e4ed114e80e889ecf61a87051eaee41f6be`
- [Released libgdiplus 6.2 source](https://dl.winehq.org/mono/sources/libgdiplus/libgdiplus-6.2.tar.gz), independently checked SHA-256 `683adb7d99d03f6ee7985173a206a2243f76632682334ced4cae2fcd20c83bc9`; the same X11 condition is present in that release
- [libgdiplus upstream build instructions, including XQuartz](https://github.com/mono/libgdiplus#build-instructions)
- [XQuartz 2.8.0 Apple Silicon support](https://www.xquartz.org/releases/XQuartz-2.8.0.html)
- [XQuartz 2.8.6 rendering fixes](https://www.xquartz.org/releases/XQuartz-2.8.6.html)
- [GitHub hosted-runner architectures and labels](https://docs.github.com/en/actions/reference/runners/github-hosted-runners)
