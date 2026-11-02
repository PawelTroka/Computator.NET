# Interactive f(x,y) surfaces in the browser

Scope: issue [#292](https://github.com/PawelTroka/Computator.NET/issues/292).
The integrated browser supports **explicit real** height fields z = f(x,y)
through the Windows-backed mathematical API. The separate native Mono software
renderer is described in [MONO-DESKTOP.md](../MONO-DESKTOP.md); its Linux/X11
acceptance does not establish macOS GUI support. The independent
[portable local preview](../MACOS-APPLICATION.md) has its own reduced backend
and currently returns an explicit unsupported response for charts.

## Rendering and evaluation contract

POST `/api/v2/chart` retains PNG for default real/complex requests. The integrated
browser opts into `interactive2D: true` to receive `kind: "curve"` or
`kind: "complex"` numerical data for explicit functions, with PNG fallback for
implicit expressions; see [2D interactions](WEB-2D-INTERACTIONS.md).
For expressions classified as `CalculationsMode.Fxy`, the same
endpoint uses the existing trusted-local Roslyn `ExpressionsEvaluator` and
request-local, unsaved custom functions. It returns JSON with:

- `kind: "surface"`
- `x[]`, `y[]` and row-major `z[yIndex][xIndex]`
- `finiteSamples` and `missingSamples`, with null height values for
  non-finite/arithmetically singular coordinates

The response is a **bounded numerical surface**, not a prerendered picture.
`gridSize` defaults to 65 and must be 9–129, at most 16,641 evaluations.
Width/height, source-size, numeric bounds, 30-second worker and execution
queue limits remain enforced. Request cancellation terminates the owned worker;
the sampler also checks a cancellation token between evaluations. Samples are
request-local and are never cached or forwarded to an external host. Completely
non-finite surfaces are rejected with HTTP 422 rather than displayed as zeroes.

For Fxy, the browser lazily loads the pinned, bundled `plotly.js-gl3d-dist-min`
package for GPU WebGL rendering. It supports interactive turntable rotation, keyboard-accessible camera controls,
consistent wheel zoom and touch zoom, shading and occlusion, numeric x/y/f(x,y) axes, holes
at undefined points, tooltips, responsive resize without reevaluation, and PNG
export of the composed view. In environments without functional WebGL, the
application reports a rendering error instead of claiming to draw a surface.
The default camera, axis margins and normalized display axes keep a tall
paraboloid inside the initial and resized viewport, with room for axis labels.
Normalized axis lengths are a presentation choice, not an equal-units geometric
projection: the original x/y/f(x,y) coordinates and tick values remain unchanged.
The chart states this distinction below the expression. Camera gestures still
allow deliberate close-up views; Reset camera restores the complete default view.
Browser regressions measure the actual rendered mesh's clearance from image
edges in the initial/narrow viewport and the composed 1200×800 PNG, including
the real mathematical API path.
The client validates array shape/counts before passing data to Plotly.
A functional WebGL context is a prerequisite: if disabled or unavailable
(e.g. a software-only headless Firefox runner), the app displays a clear
rendering error and disables image export instead of showing a fake 3D
success state. Windows Chromium and Firefox real-API E2E verify interactive
camera changes and PNG export when WebGL is available.

Alternatives considered: server-side WinForms/WPF screenshots would still be
noninteractive and Windows-only; a custom Three.js scene would require separate
axis/picking/export implementations and a larger maintenance surface. Plotly's
partial gl3d build ships inside the offline frontend, not via a CDN.

## Tests and reproducibility

- `dotnet test Computator.NET.WebApi.IntegrationTests/Computator.NET.WebApi.IntegrationTests.csproj --filter FullyQualifiedName~ChartApiTests` (Windows .NET 10):
  real HTTP worker contract and exact paraboloid, plane, oscillatory surface
  samples; unsaved custom functions, singular gaps, bound limits and cancellation.
- `cd Computator.NET.WebClient && npm ci && npm run build && npx playwright test tests/chart-surface.spec.ts`:
  real Chromium/Firefox WebGL rotation, wheel zoom, axis labels, PNG export,
  cancellation, validation and request-local unsaved editor definitions using
  a deterministic sample response.
- `./build.ps1 -Target Web-E2E` (Windows): published API and web app,
  real numerical 3D samples, UI camera movement and image export through the
  same-origin proxy (no route interception). The CI workflow records evidence.

The previously ignored legacy `GET /api/chart/2y` PNG test is replaced by
passing JSON surface tests; the legacy image route is **not** claimed to support
interactive 3D or server-side Fxy rasterization. The result guard no longer
permits the skipped 3D test.

## Still missing for complete platform parity

- **Linux Mono desktop:** the integrated native viewer supports bounded
  explicit surfaces, point series, camera interactions, axes and PNG export.
  Remaining acceptance and its synchronous trusted-code limits are documented
  in [MONO-DESKTOP.md](../MONO-DESKTOP.md).
- **macOS:** standalone native/Mono desktop experience and both Intel and
  Apple Silicon GUI tests remain unproven.
- **Standalone non-Windows web hosting:** the full Web API still depends on
  Windows UI/chart assemblies and native catalogs. The portable local preview
  runs independently on Linux/macOS for its documented calculation/script
  subset, but does not yet supply chart rendering or the full native catalog.
- **Other browser chart workflows:** implicit 3D equations, scripted point
  series, desktop autoscale variants and full property/print dialogs remain
  separate work. Accumulated real overlays, zoom/pan, complex contour/color
  selectors and their export are already integrated through
  [2D interactions](WEB-2D-INTERACTIONS.md). Large/slow arbitrary user functions
  remain subject to existing worker execution limits.

No Windows desktop rendering path, WinForms/WPF charting code, or native
packaging target is replaced by this work.
