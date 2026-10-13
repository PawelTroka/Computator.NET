# Travis: complementary Linux validation through Cake

Travis runs `./build.sh --target=Travis-Validate` on the declared Linux image. Cake owns frontend dependency installation/audit, TypeScript/Vite compilation, .NET 10 SPA publication and real Chromium/Firefox interactions against that published host. This inherited browser suite mocks the separate Windows mathematical API; it is not evidence that the full API is portable. The dedicated real-stack and portable-engine PRs provide their own independent gates.

No old Mono installation, Trusty/Xcode9 configuration, OS package repository modification, non-gating after_script tests or duplicated provider build logic remains. SDK installation uses exact-version/RID official metadata and archive SHA-512 validation, not execution of a downloaded bootstrap script. Node/npm/Cake/SDK versions must match the repository manifests. Reusable helper tests reject version drift, unapproved download origins, archive traversal, missing provider identity and accidental secret dumps.

Native Travis runs emit allowlisted provider build/job/commit metadata, actual checkout hash, OS/architecture and tool versions. Completion metadata and test counters are emitted only after the Cake validation graph succeeds. The common helpers are identical to the separate AppVeyor PR so the providers cannot acquire different pinning/identity policies.

The GitHub **Travis-Mirror** job installs the SDK through the same code and executes the Linux target, but its evidence explicitly says `nativeProviderRun=false`. It never synthesizes TRAVIS flags or masquerades as a provider build. The available connector search exposed no Travis administration/run tool, so this PR changes repository configuration but does not silently enable an account, webhook or subscription.

Acceptance requires the actual connected Travis project to run this final commit, a provider-native build URL/ID and its `travis-completed.json` evidence. A green GitHub mirror does not satisfy that external-provider gate. Before calling the provider supported, confirm its selected image, browser system dependencies and available account compute. No secrets should be made available to fork PRs.

After the portable-engine PR is accepted, add its `Portable-Validate` target as a complementary job on a genuinely supported OS/architecture. Do not invoke the Windows desktop graph under old Mono or call a Mac browser against a Windows server 'native Mac support'. Native library builds and installer signing belong in their separately reviewed tasks. Keep provider roles deliberate rather than running every expensive identical suite three times.

Primary provider metadata: https://docs.travis-ci.com/user/environment-variables/
SDK metadata: https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json
