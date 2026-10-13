# GSL error and distribution acceptance

Roadmap item 7. The process-wide native callback records an error in the calling thread's managed boundary and returns normally; the wrapper translates errors only after the native call has returned. The existing ReturnNAN setting keeps native return-value behavior, while ThrowException translates recorded errors to ArithmeticException. Host and generated TSL imports receive the same wrappers. Initialization installs the rooted callback once, not for every request. Raw status-return APIs remain preferable for new wrappers.

A count-checked one-time refactor prepares the 216 host/generated-code declarations for review. It is not a normal build step; the preparation workflow must be removed after its source commit is reviewed. Until those source changes and regression CI pass, this PR is not ready to merge.

`GSL-Distribution-Validate` builds the pinned candidate, runs existing native/numerical acceptance, stages its DLLs and provenance beside the application, clears the candidate override and launches the staged desktop. A present but invalid packaged native directory is an error, never a silent fallback. Ordinary distributions retain the embedded baseline until the explicit adoption target is selected; use this validated stage for MSI/MSIX only after review. The new loader supports the staged candidate, not arbitrary unknown native libraries.

Before release: verify the exact candidate's difficult-input behavior, native dependencies, source/patch/toolchain notices and intended architectures. This PR does not assert new ARM/x86 certification, complete sandboxing, or a newly published release. GNU GSL error handling documentation: https://www.gnu.org/software/gsl/doc/html/err.html .
