# Real web-stack validation and local deployment

Run `./build.ps1 -Target Web-E2E` on Windows. Cake builds the production assets with `VITE_API_BASE_URL=/api`, publishes both real hosts, installs Chromium/Firefox and runs the separate `playwright.e2e.config.ts` suite. No mathematical requests are mocked. Existing focused mocked tests remain under `Web-Validate`.

The two test ports (4174 and 60997) must be free. Playwright refuses to reuse another process, owns only the processes it starts, and cleans them up. Cake supplies a disposable profile instead of using real user settings. Results and traces go to `Computator.NET.WebClient/test-results`.

For a trusted local deployment, build with `VITE_API_BASE_URL=/api`; set `MathematicalApi__BaseUrl=http://127.0.0.1:60996` and a cryptographically random `MathematicalApi__Token` on the frontend process. Set the identical token as `ExecutionAccess__Token` on the API process. It must contain at least 32 random characters. Keep the token server-side; never use a `VITE_` variable for it. Both hosts must listen on loopback. The proxy rejects remote clients, non-loopback Host headers, cross-origin browser requests, non-JSON POST bodies and oversized bodies. The configured backend is a fixed loopback origin; request headers cannot choose its destination.

This is intentionally not a public execution service. TSL remains arbitrary full-trust code. A timeout stops waiting for a response, not arbitrary running code. Remote hosting, process/resource isolation and multi-user authorization remain separate acceptance work. The existing direct API mode is preserved when no execution token is configured; do not expose that mode publicly.

The frontend's existing explicit API default remains backward-compatible for old development commands. A relative URL must be selected when using this production proxy; merely hosting the old default remotely would target the visitor's localhost. Unknown API paths remain API 404 responses rather than SPA HTML.

This PR does not implement web custom-function editing or change the historical encoded-division route limitation. Those belong to the separate request-body/custom-functions PR. After both merge, expand this real-stack suite to cover those workflows; mocked tests are not a substitute.

Reference: https://playwright.dev/docs/test-webserver#multiple-web-servers
