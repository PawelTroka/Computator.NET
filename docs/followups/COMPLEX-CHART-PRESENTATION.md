# Complex-chart presentation (issue #190)

The desktop complex chart now uses one overlay renderer for screen painting,
requested-size images, saving, printing and print preview. The renderer receives
the actual target rectangle and its graphics DPI. Domain-color samples remain
separate from axes and labels.

## Layout decisions

- Axis strokes, spacing and both arrowheads share a scale derived from the
  chart's logical dimensions. Label enlargement is bounded to 1–1.8 times the
  configured font size; point fonts receive target DPI exactly once.
- Both filled arrowheads have the same length and base width. At an edge the
  base moves inward while the tip and axis remain on the zero coordinate. An
  edge head is consequently asymmetric, rather than clipped or flattened.
- Label placement measures the actual glyph paths that are drawn, including
  italic overhangs. Candidate positions account for strokes, arrowheads and the
  other label, and prefer the nearest clear position at the positive axis end.
- Oversized custom labels are reduced together when necessary to fit both;
  the configured font object, family, style and unit are preserved. Explicit
  pixel-font units retain their existing device-pixel meaning.
- A small contrasting outline keeps the configured axis and text colors
  legible over both bright and dark domain colors. Temporary drawing resources
  are disposed, and the caller's graphics state is restored.
- The existing mapping `xMin + x * (xMax - xMin) / sampleWidth` and its imaginary
  counterpart remain unchanged. In-range axes stay on their true zero
  coordinates; ranges excluding zero retain the nearest-edge convention.
  Empty or invalid domains do not draw invented axes.

## Output behavior

`SaveImage`, printing and preview use the displayed client dimensions, retaining
PR #202's choice to keep the overlay sharp even when Quality reduces the plot's
sample count. They compose onto a fresh image, leaving the cached domain-color
pixels untouched. Empty charts retain their configured background color.

`GetImage(width, height)` returns exactly the requested dimensions and samples
at the configured quality without resizing the live control or replacing its
tooltip samples. All offscreen output uses explicit 96 DPI, so the same API
request does not depend on the server's or desktop's monitor. Display painting
uses the target graphics DPI and the existing application PerMonitorV2 policy.

## Automated validation

The portable geometry suite links the actual production layout and the same
geometry tests used by the desktop suite:

```sh
dotnet test Computator.NET.Engine.Tests/Computator.NET.Engine.Tests.csproj
```

On Windows, execute the production GDI+ and control regressions:

```powershell
dotnet test Computator.NET.Desktop.Tests/Computator.NET.Desktop.Tests.csproj `
  --filter 'FullyQualifiedName~ComplexAxesGeometryTests|FullyQualifiedName~ComplexChartRenderingTests'
```

The existing Cake Windows validation also executes these tests as part of
`Computator.NET.Desktop.Tests`. Coverage includes small, medium, large, wide and
tall charts; 96/144/192 DPI; exact and near-boundary axes; positive/negative-only
ranges; font and color preservation; low-quality saves; unchanged sample
coordinates; export failures; and preserving live control/cache state.

`ProduceBeforeAfterImagesFromTheActualDomainColoringAndGdiRenderers` writes
before/after PNGs for 13 scenarios to `TestResults/complex-chart-rendering/`.
The original overlay is reproduced from master commit
`575315952a219e4355b7f575ce0b165614ef6797`; the after overlay is the production
renderer. Each pair shares the same production domain-color background for
`z² − 1`. The manifest records tested source hashes, image hashes, font family,
dimensions and DPI. The existing desktop diagnostics artifact retains this
directory, and NUnit attaches the files to the test result.

Bitmap DPI tests exercise real font and geometry scaling, but do not simulate
moving a live window between physical monitors. Review the generated images and
the live chart at the intended Windows display scale before release.

## Scope review

The initial review inspected changed filenames across all 23 then-open pull
requests. None touched the complex-chart renderer or desktop DPI initialization.
A final duplicate check found the concurrently opened PR #202; this work was
consolidated into that PR as a follow-up, preserving its full-size save behavior,
relative arrow-size setting, offscreen export isolation and portable CI route.
Numerical corrections in #192, #199 and #200 and web-chart changes in #169 are independent.
No mathematical-function implementation, contour algorithm, public chart
interface, package version or application DPI policy is changed here.
