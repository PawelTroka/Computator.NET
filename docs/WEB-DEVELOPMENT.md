# Run the web projects from Rider or Visual Studio

Install the repository's pinned .NET SDK and Node/npm toolchain. Start **Computator.NET.WebApi**, then start **Computator.NET.WebClient** using its checked-in Development launch profile. The API listens on localhost:5080 and the browser opens localhost:5081. Both projects can be debugged separately; the UI does not silently spawn a second API behind the debugger.

These defaults are below Windows' [default dynamic TCP port range](https://learn.microsoft.com/en-us/troubleshoot/windows-client/networking/tcp-ip-port-exhaustion-troubleshooting#default-dynamic-port-range-for-tcpip), 49152–65535. The previous development ports were inside that range. A Windows port reservation can cause socket error 10013 (`WSAEACCES`) even when no process is visibly listening. A lower default avoids those dynamic-range reservations; it cannot guarantee that another application or an explicit reservation has not claimed the new port.

### If a listener still cannot start

Check the reported address and Windows' reservations with `netsh interface ipv4 show excludedportrange protocol=tcp` and `netsh interface ipv6 show excludedportrange protocol=tcp`. Select an available loopback port if necessary. The application never changes system reservations, elevates itself or silently switches to a different address.

For example, start the API on another port:

```powershell
dotnet run --project Computator.NET.WebApi -- --urls http://localhost:5084
```

Then, in the frontend terminal, use the matching public API URL before starting the WebClient (or Vite):

```powershell
$env:VITE_API_BASE_URL = 'http://localhost:5084/api'
dotnet run --project Computator.NET.WebClient
```

The source host includes `VITE_API_BASE_URL` in its asset fingerprint, so restarting rebuilds an outdated frontend automatically. For a saved IDE configuration, change the API profile's `applicationUrl` and set the same `VITE_API_BASE_URL` on the WebClient profile. Explicit `--urls` and `ASPNETCORE_URLS` remain authoritative; an enabled launch profile sets `ASPNETCORE_URLS` from its `applicationUrl`. Keep the frontend on its configured development origin so the API's explicit CORS allowlist still matches.

### Frontend preparation and validation

The source-development frontend host prepares missing or changed assets through **Cake Web-Build** before listening. No prior `dotnet publish`, hidden manual npm build, or manually copied dist directory is required. Content fingerprints of both inputs and generated assets avoid rebuilding unchanged output while detecting a dist directory replaced by a package build with another API profile. Concurrent launches serialize only the frontend preparation. Failed tool installation/build is reported in the run console and startup fails, rather than serving an unexplained 404. If dist was initially missing, startup installs a physical file provider after building it.

Production does not build source or require Node at runtime. Use Cake Web-Publish for deployment; a package missing its frontend fails with an actionable startup error. The existing protected proxy configuration and same-origin browser suites are unchanged.

For rapid frontend iteration, `npm run dev` still provides Vite's development server. For ordinary IDE launch, edit and restart the WebClient; changed source is rebuilt. This deliberately does not replace Ace, its modes, the expression transformer or execution routes.

`./build.ps1 -Target Web-IDE-Validate` deletes the repository-owned generated dist directory, launches both real projects as source applications, checks deep routes/assets and verifies changing calculation results. It also replaces dist with a relative-API build, then starts a second source frontend on the existing development origin localhost:5082 and verifies that startup restores the direct-API profile. Ports 5080, 5081 and 5082 must be free. It is separate from the published-host suites so a successful publish cannot hide a broken IDE launch. Interactive IDE debugging remains a manual acceptance check.

The IDE profiles use the trusted-local direct development API mode. The packaged local workspace uses the protected proxy and a private session token; neither mode is a public execution sandbox.
