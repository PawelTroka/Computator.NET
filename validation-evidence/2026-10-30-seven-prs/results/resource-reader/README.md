# Exact compiled-resource reader regression

This evidence records the local reproduction and correction of a Windows-only false failure in the newly added localization test support. It does not represent a defect in the production German or Czech translations.

## Cause and correction

Before correction, `XDocument.Load(path)` normalized literal CRLF sequences to LF when extracting source values. The .NET SDK/MSBuild resource compiler preserved those literal CRLF sequences in the compiled satellite. With CRLF source copies, `Strings/featuresInclude` compiled to 407 characters in German and 362 in Czech; the ordinary XML reader returned 400 and 355 respectively. Each compiled value contained seven CR and seven LF characters. The unmodified test therefore rejected the two otherwise-correct satellites (`before.log`).

The shared helper now creates `XmlTextReader` with `Normalization = false`, `DtdProcessing = Prohibit`, and `XmlResolver = null`, and loads it with `LoadOptions.PreserveWhitespace`. The comparison remains exact: no translation output or newline sequence is normalized before comparison. The change was appended to PR #306 as `a8a78e93b37ba516849a5e1cf470286e7cc15c63`, parent `3b83f4d88541e2cf497abedbb709ac05dc662945`. The changed helper blob is `08c19c5ab7f7fcfd2ac2818a538e326e5ae66c55`.

## Observations

- `full-real-crlf.log` and `full-real-lf.log`: all 338 German and 338 Czech strings in the complete seven-PR resource inventory matched their compiled satellites and regional fallback under both newline conventions.
- `full-after.log`: an expanded fixture containing eleven additional preserved whitespace/entity cases per language passed all 349 strings per language.
- `negative-controls.json` and the seven linked logs: six deliberately changed values (explicit CR entity, literal CR, Unicode text, leading space, whitespace-only value, and CR-only entity) were each rejected. A DTD-bearing source was also rejected. These are expected negative-control exits, not unresolved product failures.
- All 1,014 neutral/German/Czech string entries in the real inventory declare `xml:space="preserve"`. The helper is a contract for this repository's resources; it is not offered as a general replacement for every RESX compiler behavior.

## Compiler and fixture

The local probe used .NET SDK 10.0.403 on Linux and the actual SDK/MSBuild embedded-resource compiler, including satellite assembly generation. `Program.cs` and `Probe.csproj` are the exact observed probe sources. The project includes the current shared helper from the adjacent PR #306 checkout and references System.Resources.Extensions 10.0.13. Its absolute HintPath records the original local package location and must be adjusted when replayed elsewhere.

The neutral, German, and Czech RESX input copies came from the complete seven-PR inventory; copied production resource files and generated binaries are intentionally excluded from this compact evidence. To reconstruct, place those RESX files under the probe directory preserving relative resource paths, supply the helper from the fixed commit, then use the SDK to compile first and execute the probe against the corresponding source directory. The `contract-only` argument skips the diagnostic `featuresInclude` length printout. The LF and CRLF runs used separately compiled source copies; negative controls changed source values after compilation so the exact comparison had a known discrepancy to reject.

The final integration includes the fixed helper at head `f5f1fd0697e0665f56f3d2e4fd516f49860c0497`, CI merge `a854ee8d9cdd9f598b6c9fe4c00471faf3c5f290`, tree `40cc4713ef0bdb6b49a5cc32fab35b49e17997ce`.

## Independent Windows confirmation

The updated PR #306 GSL packaged-default workflow passed on Windows Server 2025 at CI merge `f8057a20a74698f3d1885161046929b9d4f1b0b1` (head `a8a78e93b37ba516849a5e1cf470286e7cc15c63`, base `fe1aba49b5f9538355ffaaa3d1683f69a23d2d9c`). Both satellite test cases passed with 318 German and 318 Czech strings plus `de-DE` and `cs-CZ` fallbacks; the independent branch excludes the additional twenty Find/Replace strings. Desktop tests passed 559/559 and Core integration tests passed 947/947.

Run: https://github.com/PawelTroka/Computator.NET/actions/runs/41113833510

Job: https://github.com/PawelTroka/Computator.NET/actions/runs/41113833510/job/124262791179
