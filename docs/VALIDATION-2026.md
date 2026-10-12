# Migration validation notes

The authoritative results are the checks on the **current head of PR #139**, not an earlier green commit. The longer numerical suite must complete; a cancelled run is not a pass.

## Coverage boundaries

The Windows matrix runs the core unit suite, numerical integration suite, real in-process mathematical API tests, and STA desktop startup/resource tests independently. Its desktop job also publishes and checks the portable payload. Each job restores the committed NuGet graph in locked mode and treats audit failures/advisories as restore errors.

The Linux job uses the committed npm lockfile, audits dependencies, type-checks and builds the frontend, publishes the actual ASP.NET Core 10 host, and runs Chromium/Firefox tests against that host. The mathematical API is mocked in those browser tests. This does not certify the full cross-process browser-to-Windows-API deployment.

Three API cases were **already ignored before this migration** and remain explicitly ignored: escaped division in a calculation URL, escaped division in a script URL, and server-side 3D chart rendering. No new ignored cases were added to make migration checks green.

## Autocomplete test migration

The older test compared every pair of dynamic JSON entries through NUnit's general-purpose collection equality. With the upgraded test framework, this exceeded the 120-second inactivity limit. The actual HTTP response completes normally. The migrated test uses Json.NET's value comparer in a hash set while preserving full-entry uniqueness, including nested metadata and typed menu signatures.

Insertion text alone is **not** the identity of a completion: distinct overloads may insert the same text. Dedicated regression cases verify that overloads remain distinct and that identical JSON values are recognized even when property order differs. Production autocomplete entries are not removed merely to make a test pass.

## Local production smoke check

After publishing the web client, start the two processes separately. The mathematical API must run on Windows, on a trusted private machine:

```powershell
dotnet run --project Computator.NET.WebApi -- --urls http://localhost:60996
```

In a separate terminal, start the published web client on its existing local origin:

```powershell
cd artifacts/webclient
dotnet Computator.NET.WebClient.dll --urls http://localhost:63785
```

Browse `http://localhost:63785`. Vite development uses `http://127.0.0.1:5173`. TSL retains full trust; these commands are not a public hosting recommendation. Check a real user profile, chart interactions and the remaining manual release gates in `MIGRATION-2026.md` before distributing a new build.
