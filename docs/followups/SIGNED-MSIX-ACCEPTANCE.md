# Signed MSIX activation and upgrade acceptance

Roadmap item 8. `MSIX-Activation-Contracts` compiles the COM/PInvoke helper and exercises rejection contracts without installing anything. Actual acceptance is deliberately manual:

```powershell
$env:COMPUTATOR_DISPOSABLE_INSTALL_TEST = '1'
# Only in a new disposable interactive test account:
New-Item "$env:USERPROFILE/Computator-MSIX-Acceptance-Account.txt"
./build.ps1 -Target MSIX-Signed-Acceptance '--signed-package=C:/Candidates/current.msix' '--signer-thumbprint=OWNER_APPROVED_THUMBPRINT' '--baseline-package=C:/Candidates/previous.msix'
```

The harness refuses an existing installation, untrusted/unapproved signers and invalid application identities. It optionally installs the approved baseline, then the candidate, verifies a version advance, invokes IApplicationActivationManager for application and Windows.File contracts, and checks the returned process's real package identity using GetPackageFullName. Normal shutdown is required. No certificate is imported; the package must already have an owner-approved trusted signature. The harness does not fetch or sign packages.

Evidence distinguishes application activation from file-contract activation; it does not claim the editor rendered a particular document or every Windows shell/IME behavior. Historical Store signing may require a different approved baseline policy, which must be reviewed rather than bypassed. The disposable account is mandatory because shell-activated applications do not inherit the test process's profile override in the same way as directly started child processes.

A passing contracts job is not a signed installation, Store submission, certification, or real historical-upgrade result. Those remain owner-operated acceptance gates. User-data preservation and failed-update rollback must also be reviewed with the real historical corpus before release.
