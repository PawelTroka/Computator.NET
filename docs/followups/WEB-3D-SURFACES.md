# Interactive f(x,y) surfaces in the browser

Scope: issue [#292](https://github.com/PawelTroka/Computator.NET/issues/292).
This change adds a browser visualization path for **explicit real** height fields
z = f(x,y); it does **not** implement a native Linux/macOS 3D desktop control
or make the Windows-dependent API portable.

## Rendering and evaluation contract

The existing POST `/api/v2/chart` retains PNG for 2D real and complex
expressions. For expressions classified as `CalculationsMode.Fxy`, the same
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
The client validates array shape/counts before passing data to Plotly.

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

- **Linux Mono desktop:** its chart GUI still rejects 3D requests; implement
  and exercise an actual supported viewer, or explicitly offer a local browser
  workflow with matching authentication and native prerequisites.
- **macOS:** standalone native/Mono desktop experience and both Intel and
  Apple Silicon GUI tests remain unproven.
- **Standalone non-Windows web hosting:** the full Web API depends on Windows
  UI/chart assemblies and native catalogs. Browser access from Linux/macOS
  to a Windows-hosted service does not constitute a portable local package.
- **Other chart workflows:** implicit 3D equations, scripted point series,
  multiple accumulated 2D functions, desktop zoom/pan and complex chart
  selectors remain separate follow-ups. Large/slow arbitrary user functions
  are subject to the existing worker execution limits.

No Windows desktop rendering path, WinForms/WPF charting code, or native
packaging target is replaced by this work.
