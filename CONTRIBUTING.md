# Contributing to Computator.NET

[Overview](README.md) · [Developer setup](docs/DEVELOPING.md) · [Build and test](docs/BUILD.md) · [Documentation directory](docs/README.md)

Computator.NET shares mathematical and language implementations across several applications. A good contribution explains the user-visible problem, changes the smallest coherent part of that system, and supplies evidence for the behavior it affects.

## Before changing code

1. Search [open pull requests](https://github.com/PawelTroka/Computator.NET/pulls), [issues](https://github.com/PawelTroka/Computator.NET/issues) and recent merged changes. An open issue may already have a fix under review; an old TODO may already be implemented.
2. Reproduce the problem on your base commit. Include the expression, script, steps or command and the expected result. For numerical changes, distinguish a wrong result from an unsupported domain or an unsuitable tolerance.
3. Use one branch per coherent change. If an existing PR owns the same behavior, extend or coordinate that work rather than opening a competing implementation.

Use the SDK and dependency versions in the [repository manifests](docs/DEVELOPING.md#prerequisites), and open the single **Computator.NET.slnx**. [The project map](docs/DEVELOPING.md#project-map) shows where each application and test suite belongs.

## Work in the right layer

| Change | Implementation and evidence to consider |
| --- | --- |
| TSL parsing or notation | The production files linked into `Computator.NET.Engine`; shared compiler/evaluator regressions and Windows consumers |
| Mathematical functions or numerical methods | Host methods, any corresponding emitted `ToCode` implementation, domain/edge cases and the built-in reference entry |
| Desktop behavior | The actual presenter/window/editor composition and the relevant Windows test fixture |
| Web behavior | The frontend and API contracts; focused browser tests for UI behavior, real-API tests when execution semantics matter |
| Build, project or solution metadata | The canonical SLNX, evaluated MSBuild items, targeted builds and solution inventory validation |
| Function reference content | Authoring sources under `build/reference`, regeneration and [reference validation](docs/FUNCTION-REFERENCE.md) |

Source links intentionally let multiple projects use the same file. Editing a linked item edits the original source. Keep that sharing intact; a copied fix can leave another host with different mathematical behavior.

## Validate the behavior you changed

Choose a fast local check from the [test map](docs/DEVELOPING.md#test-workflow), then run the applicable Cake validation before review. For a bug fix, demonstrate that a regression reproduces the original failure and passes after the change. Tests should establish a behavior or invariant, rather than restating the implementation.

```powershell
# Windows examples; select the suite relevant to the change.
./build.ps1 -Target CoreTests
./build.ps1 -Target DesktopTests
./build.ps1 -Target ApiTests
```

```sh
# Portable production engine, its tests, and the published CLI contract.
./build.sh --target=Portable-Validate

# Frontend and production host with focused mocked-API browser tests.
./build.sh --target=Web-Validate
```

The [build guide](docs/BUILD.md) explains the broader targets and their output. Keep a Windows cross-build distinct from execution on Windows, and mocked browser tests distinct from the real mathematical API. Record what ran on the exact reviewed commit and what still needs an appropriate environment. Report failures and skipped work plainly.

Documentation-only changes normally need link, command and factual review rather than a full application test run. Check relative links and anchors, confirm examples against the implementation, and keep historical measurements attached to their original commit.

## Keep repository contracts discoverable

- **One solution:** add supported projects to `Computator.NET.slnx`; link new developer-facing guides and tooling in appropriate solution folders. Retired projects and explicit Mono probes have documented exceptions.
- **One orchestrator:** put shared build/test/package behavior in Cake. Provider YAML and launchers should invoke it, with platform setup kept where it belongs.
- **One source of version truth:** edit `Directory.Packages.props` for application NuGet pins and review the single frontend lockfile for npm changes. Follow the [dependency policy](docs/BUILD.md#dependency-policy-one-frontend-lockfile-not-twelve-nuget-graphs); retain auditing.
- **Current entry points:** update the README, developer guide or relevant command guide when the workflow changes. Add a link from the [documentation directory](docs/README.md) for a new guide.

## Open a reviewable pull request

Include these four things in the description:

1. **Problem:** who encounters it, a small reproduction and an issue link when one exists.
2. **Change:** the resulting behavior and any compatibility or numerical tradeoff.
3. **Validation:** exact commands, meaningful results, platform and limitations; add actual before/after captures for a visual change.
4. **Relationship to existing work:** mention dependencies or overlapping PRs and the intended review order.

Do not turn historical test totals into current success claims. Signing, installation on clean machines, publishing and release acceptance have their own [gates](docs/RELEASE-3.0.md).

## Report a useful bug

Include the application surface (desktop, web, CLI or Mono), version or commit, operating system, reproduction steps and actual/expected behavior. For development failures, include the failing command and relevant output from `dotnet --info`; web toolchain problems also benefit from `node --version` and `npm --version` as seen by the process that failed. A short script or screenshot often makes the problem much easier to isolate.

Remove access tokens, personal profile data and unrelated private paths from public logs. Use a disposable [development profile](docs/DEVELOPING.md#run-and-debug) when experimenting with settings or document migration.
