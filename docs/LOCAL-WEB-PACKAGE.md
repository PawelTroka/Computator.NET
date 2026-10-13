# Protected local application — roadmap 1

Build with `./build.ps1 -Target Local-Web-Package`. The result is a self-contained Windows x64 ZIP containing both hosts, the exact relative-API frontend assets and Start-Computator.cmd. Extract it, run the launcher, and use the browser it opens. No user-managed execution token, SDK or Node installation is required by the candidate.

The frontend generates a fresh cryptographic token in memory, selects a loopback API port, starts only its sibling packaged API, waits for authenticated readiness and uses the protected same-origin proxy. Browser requests never receive the private backend token. The child API belongs to a Windows job with kill-on-close lifetime behavior; frontend shutdown/crash closes that owned job. This is process lifetime management, not an untrusted-code sandbox.

Production mathematical API startup now requires a token. Tokenless development requires both the Development environment and the explicit AllowUnauthenticatedDevelopment option. The checked-in API IDE profile intentionally declares that compatibility mode, keeps loopback client/Host checks, and does not weaken production. Start the two IDE projects separately as described by the preceding WebClient startup PR. Its source-start test must use that explicit Development profile when combining the changes.

The deployment graph specifies its own relative-API asset build, not a test of the outer Cake target's name. Aggregating Local-Web-Package into a release target therefore preserves the same deployment behavior. Existing component ZIPs remain developer-oriented; the LocalWeb ZIP is the normal complete browser-facing product candidate. No global service, firewall exception, public hosting, signing or release upload is created.

Validation creates the ZIP, unpacks it, compares all payload hashes, and runs Chromium/Firefox against the unpacked self-contained frontend and its automatically owned API. It checks calculations, definitions, generated images, deep links, unauthorized backend access, cross-origin rejection and listener cleanup. The API transport/smoke tests use real private tokens rather than a test-only production bypass. Source-development and installer acceptance remain separate checks.

An HTTP timeout still cannot terminate arbitrary script execution; the later worker/cancellation PR addresses that reliability boundary. Trusted scripts retain operating-system permissions. Do not expose this product as a multi-user Internet service.

Review the self-contained package on a supported clean Windows machine without shared development runtimes before certification. The current CI runner has an SDK installed, even though the tested payload includes its own runtime. This PR introduces no extra solution, project or NuGet lockfiles.
