# AppVeyor: a real secondary Windows gate, not another build implementation

AppVeyor calls `./build.ps1 -Target AppVeyor-Validate` for Debug and Release. Cake owns the same audited restore, Windows tests, browser tests, published process checks and portable packaging used by GitHub. The provider file only selects the image, prepares tools, caches dependencies and retains reports/artifacts. Test failures are gating; no tests are moved into a non-gating after_script phase.

The SDK is downloaded from Microsoft's exact-version/RID release metadata and verified with its SHA-512 before extraction. No remote bootstrap script is executed. Existing/unrecognized tool directories are not overwritten, archive paths/links are checked, and the installed SDK must match global.json. Node, npm and Cake must match their repository manifests exactly; mismatched preinstalled image tools are reported rather than silently accepted. The shared bootstrap/evidence helpers are identical in the Travis PR.

`AppVeyor-Preflight` requires native AppVeyor metadata, verifies the actual checked-out commit (including a provider merge commit when appropriate), and writes only allowlisted build IDs/versions/OS details. It never dumps the environment or API tokens. Completion evidence is emitted only after the real Cake Publish target succeeds and includes actual test-report counters. TRX files are uploaded to AppVeyor's test results as well as retained as artifacts.

The separate GitHub **AppVeyor-Mirror** job tests this SDK bootstrap and helper rejection cases, but records `nativeProviderRun=false`. It does not set fake AppVeyor flags or claim a native-provider success. The real AppVeyor project must be connected to this repository and its webhook enabled by the account owner; no provider administration connector was available in this session. No credentials are needed by the committed scripts and no external provider account settings were modified.

Acceptance requires an actual AppVeyor run for the candidate commit, its provider URL/build ID, both configurations, and the generated completion JSON/test artifacts. Until that exists, a green GitHub mirror is only script evidence. The MSI/MSIX follow-ups are merged; adding their supported validation targets to a deliberate secondary installer job remains separate provider work. Do not call old disabled targets or accept WiX terms implicitly. Signing/release jobs belong in protected owner-authorized environments, never fork PRs.

SDK source: https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json
Provider image and metadata: https://www.appveyor.com/docs/windows-images-software/ ; https://www.appveyor.com/docs/environment-variables/
