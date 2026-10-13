# Validation and maintenance

Use [the Cake target graph](BUILD.md), not duplicate PowerShell build commands. `./build.ps1 -Target CI` runs the Windows checks; `./build.sh --target=Web-Validate` runs production frontend checks. `Publish` combines tests and local ZIP packaging. The wrapper scripts under `build/Validate-*.ps1` are compatibility aliases only.

## Evidence requirements

The authoritative status belongs to the current commit. The result guard rejects incomplete runs, missing reports, duplicate identities, reduced test discovery and undocumented skips. A build or startup test alone is not proof that a user workflow works.

Custom-function coverage now follows the actual displayed desktop editors through calculations, unsaved changes, scripting, save/reopen and highlighter bindings. The earlier compiler/API tests passed with manually supplied custom-function strings even when the desktop's DI factories fed presenters a different hidden editor. This regression is explicitly tested in all three editor implementations and both runtime branches.

The Windows suite tests both in-process API endpoints and real Kestrel requests. `Web-Validate` uses the published ASP.NET Core SPA host with a mocked mathematical API. The separate `Web-E2E` and `Web-CustomFunctions` targets passed 22 Chromium/Firefox checks against the published hosts, protected local proxy and actual mathematical API. Published-process smoke tests also exercise the real desktop executable and Kestrel host. Complete visual parity, high-DPI/editor interaction and real historical profiles still need manual acceptance; the browser checks do not cover every numerical method or certify public hosting.

The combined migration passed all 11 GitHub workflows and local Cake `Publish` at its reviewed integration commit. See [the release evidence and remaining gates](RELEASE-3.0.md) for the tested commit and scope. Later changes require their own current-commit checks.

The merged .NET 10 follow-up fixes escaped division in calculation/script URLs and removes those two skip allowances. These fixtures now exercise real Kestrel requests, including encoded slashes, literal percent sequences, and custom definitions. The remaining API skip is server-side 3D chart rendering. No new skips are permitted by default. Autocomplete uniqueness uses full JSON value equality so overloads remain distinct; it does not discard production entries to make assertions pass.

## Dependency maintenance

Direct NuGet versions live in `Directory.Packages.props`; per-project NuGet lockfiles are removed. The full resolved graph is audited during Cake restore. Direct version pins do not freeze transitive versions; this is a deliberate simplification, not a claim of byte-for-byte repeatable dependency resolution.

Keep the single npm lockfile consistent with `package.json` for `npm ci`. SDK, Node and Cake runner versions are explicit. Dependency bots may propose updates but do not auto-merge them. Review breaking changes and rerun the same Cake targets used by CI.

AppVeyor and Travis definitions invoke Cake as well. Provider-specific configuration changes are not proof those external accounts ran successfully; GitHub Actions results should not be described as AppVeyor/Travis execution evidence.
