# Execution workers and real cancellation

Roadmap item 10, final item in the ordered review series. Modern JSON execute/chart endpoints dispatch through a MVC action filter to an owned one-shot child process. The worker reuses the existing controller implementation without recursively entering MVC. Old legacy GET routes remain compatible and in-process; their cancellation semantics are not falsely advertised as changed.

Up to four workers run concurrently with a bounded total of 32 queued/active requests. Requests have a 30-second queue-plus-execution budget; stdin/stdout/stderr are bounded. Request cancellation or timeout kills the owned process tree and waits for the child to exit before releasing its slot. A Windows kill-on-close job owns each worker and its descendants, including when a directly launched API crashes or is stopped by the IDE. Completion also closes that job, so descendants cannot retain redirected pipe handles. Each completed request exits its process, bounding generated-assembly lifetime independently of collectible context behavior. A new request can start after an infinite or crashing script. The worker filter runs after the common execution-policy filter, so invalid requests are rejected before creating a child process.

The WebClient's existing result panel exposes Cancel execution, using the existing AbortController. The token-protected local proxy already forwards request cancellation to the API. Definitions, calculation, scripting and charts retain their existing JSON routes. Normal Console.Write from scripts is isolated from the protocol; protocol corruption is rejected, not executed.

```powershell
./build.ps1 -Target Worker-Validate
```

New tests start real API worker processes, terminate an infinite script, verify the PID exited, run a successful subsequent request, and verify caller cancellation and definition/protocol isolation. Cancellation waits for a marker written by the running script, proving that execution began. Another test abruptly terminates only the API parent and verifies that its running worker exits. Inherited real browser tests exercise the unchanged route contracts.

This is reliability isolation, NOT a public sandbox: workers run with the user's OS permissions, can perform side effects, and deliberately malicious code can bypass managed output redirection. Process-tree termination does not establish OS-enforced containment of adversarial descendants. No public service should be enabled without separate restricted identities, CPU/memory/job-object limits, filesystem/network policy and authorization. This PR does not set a hard memory cap or claim Windows/Linux/Mac security isolation. Manual launch/startup performance and platform acceptance remain required before release.
