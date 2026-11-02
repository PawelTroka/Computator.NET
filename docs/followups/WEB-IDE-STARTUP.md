# Web IDE startup validation and failure evidence

Run `./build.ps1 -Target Web-IDE-Validate` on Windows. The existing workflow runs
the same cold-start and restart scenarios with the pinned Node/npm tools and
with the inherited Node 24.11.1/npm 11.7.0 toolchain from issue #257. Cake selects
the repository's exact tools in both cases; the source hosts still inherit the
original IDE PATH. The full `Web-Restore` npm audit gate remains required.

The suite must keep all three checks:

1. Build missing frontend assets on a source-host launch and calculate `2x`
   through the real API, changing x from 21 to 9 and observing 42 then 18.
2. Serve actual JavaScript/CSS and preserve 404 responses for unknown APIs.
3. Replace assets built for the relative `/api` profile on an IDE-style restart,
   then calculate `40+2` through the direct localhost:5080 API.

Both calculation scenarios require the expected API origin, endpoint, POST
operation, HTTP 200, JSON result and displayed result. There are no execution
retries, mocked mathematical responses or warmup calculations. The worker's
30-second queue-plus-execution limit is unchanged. The browser allows 35 seconds
to observe the response, matching the existing real-stack suite: transport and
worker cleanup must not race the server deadline. A server 504 still fails.

## Retained diagnostics

The workflow's existing always-uploaded `ide-startup-results-<toolchain>` artifact
includes:

- `ide-api.log`: API startup and Information-level request start/completion logs.
  The suite explicitly overrides the Console provider's Warning default; this
  does not change production logging configuration.
- `ide-client.log`: the first source host's stdout/stderr, including asset preparation.
- `restarted-host.log`: the restarted source host's final 32,000 characters of
  output, written to the test output directory and attached after the child's
  streams close. It remains in the artifact when the test passes, too.
- `ide-startup.xml`, plus screenshots, page snapshots and traces for failed tests.

The initial host logs are outside Playwright's cleaned `test-results/ide`
directory. They are created before readiness polling, so failed startup is also
observable. Investigate request URL, body, status and timing in the trace before
assuming that a missing response means a missing request. Compare the API log
with the actual browser response and the worker policy in
[`EXECUTION-WORKERS.md`](EXECUTION-WORKERS.md).

## PR #315 investigation, November 1, 2026

Original source: `548004f9597bff8b44a96149be1c7c8e2ebef694`.
Original [run 41107317143](https://github.com/PawelTroka/Computator.NET/actions/runs/41107317143),
[failed job 124240455258](https://github.com/PawelTroka/Computator.NET/actions/runs/41107317143/job/124240455258),
[artifact 13006530185](https://github.com/PawelTroka/Computator.NET/actions/runs/41107317143/artifacts/13006530185).
The downloaded ZIP's SHA-256 was
`796eaf5d0e2fb2252a22a5582b35f4da848193b47ae32ca6f8f1d2ab0043e8db`.

| Evidence from the original traces | Finding |
| --- | --- |
| Initial HTML, JavaScript and CSS | HTTP 200; both launches used `index-HH0wOV6H.js` and `index-EuENuLC6.css` |
| Initial calculation | POST to `http://localhost:5080/api/v2/execute`; still pending at test teardown |
| Restart calculation | POST to the same API, HTTP **504** after **31,065.996 ms** |
| Restart response body | `{"title":"Execution exceeded its time or output limit and was terminated.","status":504}` |
| Restart CORS header | `Access-Control-Allow-Origin: http://localhost:5082` |
| Initial / restart autocomplete | 26,462.394 ms / 97.001 ms, both HTTP 200 |
| Calculate clicks | Both completed, in approximately 70 ms and 64 ms |
| Browser console | Restart reports the real HTTP 504; no recorded application initialization exception |

The first screenshot shows `Calculating...`; the restart screenshot shows the
worker-limit error. The restored API profile, real POST, server response and
CORS header rule out incorrect routing, missing assets, disabled Calculate
buttons and CORS rejection as explanations for these two failures.

The source starts a new worker for each calculation. Its deadline includes queue
wait, process startup, runtime/native initialization, compilation and execution.
The restart therefore does not reuse a warmed calculation process. The original
artifact did not retain worker phase timings or detailed backend startup logs,
so it cannot identify which internal phase exhausted the deadline.

Two findings are distinct: the worker really exceeded its limit, and the test's
identical 30-second wait hid that error behind a response timeout. This change
corrects the latter and preserves evidence for investigating the former. It does
not claim to fix an unmeasured compiler or Windows scheduling defect.

### Comparison and reproduction limits

PR #313 (`9ffdfa21a16feb0ca209e45694d1973af5bd513e`) passed the same issue-257
scenario in [run 41106162683](https://github.com/PawelTroka/Computator.NET/actions/runs/41106162683).
Its WebClient, WebApi, startup workflow and frontend tool-selection source match
PR #315 and their common base `fe1aba49b5f9538355ffaaa3d1683f69a23d2d9c`.
Both issue-257 jobs successfully selected Node 24.21.0/npm 11.19.0.

The failed #315 job used Windows image `20261030.294.1`; the passing #315 pinned
job and #313 issue-257 job used `20261021.278.1`. Browser creation, rendering and
autocomplete were also unusually slow on the failed runner. This is evidence
of broader host slowness, not proof that a specific image update, antivirus
component, CPU or Roslyn phase caused it.

An unchanged rerun on November 1,
[job 124925038413](https://github.com/PawelTroka/Computator.NET/actions/runs/41107317143/job/124925038413),
stopped at `Web-Restore`: five moderate findings in the existing MathJax tooling
dependency chain. It never ran Playwright and cannot establish whether the
original execution timeout reproduces. The separate
[audit fix #318](https://github.com/PawelTroka/Computator.NET/pull/318) supplies an
explicit prerequisite for fresh validation without disabling the security gate.
