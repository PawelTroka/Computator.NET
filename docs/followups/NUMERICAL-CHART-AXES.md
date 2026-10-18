# Numerical chart axes

This work addresses [issue #221](https://github.com/PawelTroka/Computator.NET/issues/221).
The ordinary `f(x)` chart already uses WinForms DataVisualization's numerical
labels and major/minor tick machinery. Complex domain-colouring charts and the
WPF `f(x,y)` / 3D surface chart now receive numerical markings through their
existing rendering paths.

## Related work and scope

The implementation starts from integrated master
`ba57b8f08c1014363d26e7b87249c0e51fe74688`. The current issue and newer pull requests
were checked before editing. [PR #202](https://github.com/PawelTroka/Computator.NET/pull/202)
provides the complex renderer's axis geometry, measured glyph paths, contrast
outlines, display scaling, and screen/export composition. Those paths remain in
use, including the later integration corrections to sample-cache isolation.

[PR #227](https://github.com/PawelTroka/Computator.NET/pull/227) changes the chart-area
editor's min/max labels. This change does not edit that view or presenter. It also
leaves [PR #225](https://github.com/PawelTroka/Computator.NET/pull/225)'s editor
colour-swatch work separate. The existing `f(x)` renderer and package versions
are unchanged.

## Shared numerical intervals

`Charting/Axes/AxisTickGenerator.cs` is independent of Windows controls, GDI and
WPF. Both custom renderers supply their visible axis length and measured label
spacing in the same units. Intervals use a decimal `1, 2, 2.5, 5` progression.
Minor subdivisions use less space than the major markings and are reduced when
the available spacing or floating-point precision cannot represent them.

Tick coordinates come from integer grid indices, rather than repeatedly adding
a floating-point step. Candidate counts are bounded. Range normalization avoids
overflow for opposite-sign finite extremes, and the chosen step accounts for
the precision available at large coordinate offsets. Invalid or unrepresentable
ranges do not produce an unbounded tick loop.

Formatting uses the tick interval to select precision, preserves nonzero tiny
values, removes negative zero, and switches to compact scientific notation for
small or large magnitudes. Both renderers pass the current culture. Distinct
major coordinates retain distinct numerical labels even when the visible range
is narrow relative to its offset.

## Complex charts

The numerical overlay uses `ComplexAxesLayout`'s existing origin and the same
`fraction * Width/Height` convention as the production samples. It does not
change the plotted function's coordinates or adopt a separate `Width - 1`
mapping. A zero outside the visible range leaves the axis on the existing
boundary; numerical values still describe the visible range.

`ComplexTicksLayout` takes glyph measurements from the actual GDI renderer. It
chooses horizontal and vertical intervals separately, reserves the coordinate
names, arrowheads and axis strokes, and places each numerical label only when
its measured rectangle fits. A common origin receives one zero label. Major
endpoint numbers remain eligible even where an arrowhead replaces their marker.

Major ticks are twice the length of minor ticks. Minor strokes are thinner.
Numeric text is smaller than coordinate names and uses the selected font family,
style and label colour, with the existing contrast outline. Point-font DPI is
applied once; explicitly selected pixel fonts retain their pixel units.

Screen rendering, saving, printing and `GetImage` continue through the existing
composition path. Export dimensions do not resize the live control or replace
its tooltip samples. Offscreen complex output keeps its explicit 96-DPI policy.

## WPF 3D and f(x,y) charts

The surface chart's numerical axes follow the existing three mesh segments at
the expanded data-bound corner. Each marker is projected through
`TransformMatrix.VertexToScreenPt`, using its real world coordinate. The
annotation layer measures upright text in WPF device-independent units.

The visible portion of each projected axis determines its interval. The layer
updates with chart transforms, viewport size, label settings and DPI, and checks
collisions between numbers, coordinate names and the projected axes. Short or
end-on axes omit labels that cannot fit. A collapsed constant coordinate has
one value and no invented subdivisions.

The projection's previous absolute small-X-range cutoff is removed, and zero
ranges receive a finite projection fallback. This allows small-scale and flat
surfaces to retain usable annotations. Mesh samples continue to use the existing
single-precision representation; the change does not expand the surface
renderer to arbitrary double-precision input magnitudes.

Axis cylinders and arrowheads use radial dimensions from each coordinate's
span, with a finite fallback for a collapsed dimension. Their shared mesh
builder also supplies the annotation layer's arrow bounds. Exact principal-axis
coordinate permutations avoid trigonometric residue from a large coordinate
overwhelming a very small one when independent axis scaling is selected.

Requested-size exports lay out a separate visual using the existing scene and
view matrix. Numerical label spacing is recalculated for the output aspect
ratio, while the live viewport, mesh and annotations remain intact.

## Focused validation

The production tick generator and complex tick geometry are linked into the
portable engine test project. Run them without Windows rendering dependencies:

```sh
dotnet test Computator.NET.Engine.Tests/Computator.NET.Engine.Tests.csproj -c Release \
  --filter 'FullyQualifiedName~AxisTickGeneratorTests|FullyQualifiedName~ComplexTickGeometryTests|FullyQualifiedName~ComplexAxesGeometryTests'
```

The Windows desktop project executes actual GDI and WPF rendering on an STA
thread. Run the axis fixtures and existing complex-renderer regressions:

```powershell
dotnet test Computator.NET.Desktop.Tests/Computator.NET.Desktop.Tests.csproj -c Release `
  --filter 'FullyQualifiedName~AxisTickGeneratorTests|FullyQualifiedName~ComplexTick|FullyQualifiedName~ComplexAxesGeometryTests|FullyQualifiedName~ComplexChartRenderingTests|FullyQualifiedName~Chart3DAxisRenderingTests'
```

The existing Cake Windows validation runs these tests as part of the complete
desktop suite. Rendered PNGs, manifests and test attachments are written below
`TestResults/chart-axis-rendering/` and retained in the
`diagnostics-Computator.NET.Desktop.Tests` workflow artifact. Assertions check
actual numerical ink, containment, collision avoidance, marker hierarchy,
coordinate alignment and export isolation, in addition to producing pictures
for review.

The existing complex before/after fixture still identifies its original
pre-PR-202 baseline. Its old images must not be described as a before/after
comparison for issue #221. The numerical-axis fixtures identify their own
production sources and image hashes. Bitmap DPI tests exercise real drawing
scale; they do not simulate physically moving a window between monitors.
