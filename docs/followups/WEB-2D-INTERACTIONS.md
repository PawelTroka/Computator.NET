# Browser real and complex chart interactions

This work addresses the 2D chart portion of [issue #292](https://github.com/PawelTroka/Computator.NET/issues/292).
It is independent of the WebGL surface implementation in draft [PR #294](https://github.com/PawelTroka/Computator.NET/pull/294).
It does not establish complete cross-platform feature parity or change API hosting requirements.

## User-visible behavior

- **Draw chart!** replaces the current chart with the submitted expression and current bounds.
- **Add function** accumulates up to eight explicit real functions on the same numerical axes. Each function retains the custom-function source submitted with it. Visibility and removal controls operate on individual functions.
- Wheel zoom is anchored to the pointer. Pointer dragging pans; two-pointer gestures pan and zoom. Buttons and a focused chart's arrow, `+`, `-`, and `Home` keys provide alternatives. **Reset view** restores the initial draw bounds; **Apply bounds** resamples the submitted functions at explicitly entered bounds.
- Navigation requests fresh mathematical samples, including newly exposed coordinates. Rapid gestures coalesce for 180 ms. Cached samples provide a coordinate-correct preview while the new viewport is evaluated.
- Complex charts display one domain-color field, as the desktop does. They support **None**, **Logarithmic**, and **Linear** contours, contour spacing, magnitude color direction, axes visibility, and nearest-sample values under the pointer. Color and contour changes repaint the cached numerical grid without compiling or executing the expression again.
- **Download PNG** exports the displayed canvas, including its current viewport, visible functions, axes, and legend. Export is disabled while a navigation request is pending. The legacy PNG fallback keeps its existing image download.
- Cancellation invalidates the active batch, aborts the shared request signal, restores the last committed viewport, and prevents late results from changing the chart. Clearing or leaving the page also cancels outstanding work.

The expression editor remains a draft: typing, numeric input edits, and window resizing do not execute it. Adding a function uses the existing chart bounds. To use edited bounds with the existing functions, choose **Apply bounds**. To submit a different expression and bounds together, choose **Draw chart!**.

## Rendering architecture

The API is the sole expression evaluator. It continues to use `ModeDeterminer`, `ExpressionsEvaluator`, Roslyn/TSL, and the same request-local custom definitions. The browser receives numbers; it does not translate or evaluate user expressions in JavaScript.

The existing `POST /api/v2/chart` endpoint accepts an **opt-in** `interactive2D` flag. Default callers continue to receive real/complex PNGs. The flag is handled inside `ExecutionV2Controller.Chart`, preserving the existing validation filter, four-worker execution pool, bounded waiting queue, 30-second deadline, process ownership, and cancellation behavior. Introducing an unrelated endpoint would require explicit worker-dispatch changes; this implementation deliberately keeps that established execution path.

`Chart2DSampler` supplies explicit real polyline samples or a complex-value grid. `Plot2D` draws those samples with a mathematical coordinate transform on Canvas 2D. Axes, curves, complex colors, and the export share that transform. This avoids treating a server screenshot as the chart's coordinate model. The canvas and axes redraw responsively from cached data on resize, without server execution or a WebGL requirement.

`Chart` retains an immutable expression/custom-function snapshot per layer. A viewport refresh samples a bounded list sequentially, avoiding a burst that exhausts the shared worker slots. The complete batch publishes atomically. A failed addition or failed refresh retains the previous chart, its labels, and its export. All response publication is guarded by request identity as well as `AbortSignal` state.

## API contract

Example request:

```json
{
  "expression": "sin(x)",
  "customFunctions": "",
  "xMin": -5,
  "xMax": 5,
  "yMin": -2,
  "yMax": 2,
  "width": 800,
  "height": 500,
  "interactive2D": true,
  "curveSamples": 1025,
  "complexGridSize": 257
}
```

Explicit real responses use `kind: "curve"`, ascending `x`, nullable `y`, `finiteSamples`, and `missingSamples`. A null sample breaks the polyline; the renderer never connects across it. Explicit complex responses use `kind: "complex"`, ascending `x` and `y` axes, nullable `real[yIndex][xIndex]` and `imaginary[yIndex][xIndex]` matrices, and the same counts. Undefined complex samples always contain a pair of nulls. The browser verifies response shapes, ordered finite coordinates, limits, counts, and request-domain endpoints before publishing.

| Limit | Contract |
| --- | --- |
| Real base samples | 33–4097; browser default 1025 |
| Real discontinuity probes | At most 8 extra evaluations per base interval |
| Real response points | At most `2 * curveSamples - 1`, including null break markers |
| Complex samples per axis | 9–257; browser default 257, at most 66,049 values |
| Browser real layers | At most 8 |
| Bounds | Finite increasing intervals with finite positive spans and representable, strictly increasing sample coordinates |
| Request/worker limits | Existing source, request-size, image-dimension, queue, deadline, and output limits remain in force |

Non-finite results and arithmetic singularities become holes. Other evaluation exceptions remain failures. A domain containing no finite samples returns HTTP 422. Invalid sampling counts and invalid spans are rejected before worker allocation/compilation. Direct samplers accept cancellation tokens; isolated workers still terminate an uncooperative user function that never returns.

For real curves, bounded midpoint probing distinguishes a smooth steep line from common unresolved jumps or poles between base samples. Valid endpoints are retained and a null point breaks the suspect interval. This is a finite-resolution heuristic, not a proof of continuity: narrow spikes, fast oscillations, and closely spaced singularities can still alias. Zooming obtains fresh samples. The current UI uses a fixed bounded sampling density, not an arbitrary-precision adaptive mathematical plotter.

## Complex display decisions

The browser preserves the desktop phase convention (positive real red, negative real cyan). **Greater is darker** without contours uses the desktop brightness/saturation formula. **Greater is lighter** implements the documented black-at-zero/white-at-infinity direction; the current desktop formula does not actually reach that white limit, so exact color identity is not claimed.

Linear and logarithmic contours use closed-form band calculations. Linear spacing must be positive; logarithmic spacing must exceed one. Invalid spacing retains the last valid displayed setting and gives an inline error. Color direction remains effective with contours in the browser, whereas desktop contour modes ignore the direction setting.

The desktop **Exponential** contour option is omitted: its current magnitude-dependent loop can have a zero or negative increment for magnitudes at or below one. Copying that algorithm into the browser would create a responsiveness defect. A well-defined replacement and desktop reconciliation remain separate work.

## Compatibility with WebGL PR #294

The comparison target is PR #294 head `9fc1b7949fab5dab4cd3ce04cfef2bf944511b01`, based on master `fe1aba49b5f9538355ffaaa3d1683f69a23d2d9c`.

- This independent branch does not add Plotly or duplicate `SurfacePlot`.
- The new API branch applies only to `FunctionType.Real2D` and `FunctionType.Complex`. PR #294's explicit `Fxy` dispatch, grid limits, and `kind: "surface"` contract remain distinct.
- Both implementations share `Chart.tsx`; combining the branches requires reconciling that UI dispatch so numeric 2D data, surface data, and legacy PNG fallback are all accepted. It is not claimed to be a conflict-free merge.
- In an integrated version, `SurfacePlot` must retain its lazy loading, 33/65/97 quality selector, cached resize, camera controls, WebGL support reporting, and composed PNG export.
- Compatibility evidence is recorded below against exact source heads. No original PR or master is merged by this validation.

## Verification

The new `InteractiveChartApiTests` covers independent known real and complex values, grid orientation, singularity holes including an off-grid pole, undefined-domain recovery, unsaved definitions and isolation, sampling limits, overflow and precision limits, default/implicit PNG compatibility, bounded steep-line handling, and cancellation in both samplers.

Browser fixture tests in `tests/chart-interactions.spec.ts` exercise actual canvas interactions and export with deterministic API data. They check request bounds and submitted source, overlay management, cached resize/display changes, cancellation and stale-result rejection, malformed responses, and mixed-mode rejection. Existing PNG lifecycle tests remain intact.

`e2e/chart-interactions.spec.ts` adds published Windows API/browser scenarios without route mocks. Existing real-stack/custom-function/cancellation checks are updated to inspect numerical payloads and canvas output while retaining direct default-PNG assertions.

Commands (use the repository's pinned Node/npm and .NET SDK):

```sh
cd Computator.NET.WebClient
npm ci
npm run build
npx playwright install chromium firefox
npx playwright test tests/chart-interactions.spec.ts tests/chart-lifecycle.spec.ts
```

On Windows, `./build.ps1 -Target Web-E2E` and `./build.ps1 -Target Web-CustomFunctions` exercise the published services and browser; the existing Cake validation matrix runs API regression tests. The actual API remains Windows-dependent; Linux fixture-browser results are not a substitute for those real API checks.

### Evidence status — 28 October 2026

- Production frontend typecheck/build: passed locally, Node 24.22.1 and npm 11.19.0 (repository Node pin is 24.21.0).
- API/test Release cross-compilation: passed on Linux with .NET SDK 10.0.402 and `EnableWindowsTargeting=true`; zero compilation errors. This establishes compilation, not Windows runtime behavior.
- Browser execution, Windows API execution, and combined WebGL checkout: pending at the initial draft commit. Final results and exact validation references will be added after execution.

## Remaining desktop/browser differences

- Explicit real curves have overlays; complex charts intentionally retain a single domain-color field.
- Implicit real/complex expressions retain their existing server PNG path and manual bounds. They do not gain direct 2D navigation or overlays in this workstream.
- Desktop point-series/scripted plotting workflows, auto-scale variants, per-axis wheel modifiers, arbitrary series/legend styles, print dialogs, and full chart property editing are not implemented here.
- Complex exponential contours and exact desktop color-pixel parity remain unresolved as described above.
- The browser plot uses bounded samples; very fine features may require zooming. Large numbers of expensive user functions can make a sequential overlay refresh slow, with existing cancellation/deadline controls available.
- Browser access does not establish native Linux/macOS API hosting or certified desktop GUI behavior. PR #294 still owns 3D surface support.

Keep the implementation PR in draft until its evidence and remaining differences have been reviewed. Merging requires the owner's explicit approval.
