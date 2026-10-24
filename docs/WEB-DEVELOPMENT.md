# Run the web projects from Rider or Visual Studio

Install the repository's pinned .NET SDK and Node/npm toolchain. Start **Computator.NET.WebApi**, then start **Computator.NET.WebClient** using its checked-in Development launch profile. The API listens on localhost:60996 and the browser opens localhost:63785. Both projects can be debugged separately; the UI does not silently spawn a second API behind the debugger.

The source-development frontend host prepares missing or changed assets through **Cake Web-Build** before listening. No prior `dotnet publish`, hidden manual npm build, or manually copied dist directory is required. Content fingerprints of both inputs and generated assets avoid rebuilding unchanged output while detecting a dist directory replaced by a package build with another API profile. Concurrent launches serialize only the frontend preparation. Failed tool installation/build is reported in the run console and startup fails, rather than serving an unexplained 404. If dist was initially missing, startup installs a physical file provider after building it.

Production does not build source or require Node at runtime. Use Cake Web-Publish for deployment; a package missing its frontend fails with an actionable startup error. The existing protected proxy configuration and same-origin browser suites are unchanged.

For rapid frontend iteration, `npm run dev` still provides Vite's development server. For ordinary IDE launch, edit and restart the WebClient; changed source is rebuilt. This deliberately does not replace Ace, its modes, the expression transformer or execution routes.

`./build.ps1 -Target Web-IDE-Validate` deletes the repository-owned generated dist directory, launches both real projects as source applications, checks deep routes/assets and verifies changing calculation results. It also replaces dist with a relative-API build, then starts a second source frontend on the existing development origin localhost:63784 and verifies that startup restores the direct-API profile. Ports 63784, 63785 and 60996 must be free. It is separate from the published-host suites so a successful publish cannot hide a broken IDE launch. Interactive IDE debugging remains a manual acceptance check.

Source development uses the explicit trusted-local Development API mode. The implemented [protected local package](LOCAL-WEB-PACKAGE.md) provides a separate self-contained launcher, private backend token and owned API process. Neither mode is a public execution sandbox.
