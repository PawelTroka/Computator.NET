## October 18 clean-machine release gate

The post-NLog installer candidate was built from commit `84d98f4b6449af1f800ad545e1a68eb677e6db39`, which has the same Git tree `68f91394ead0fc4f8da814cbce2b06ecbf23ab69` as final master `ba57b8f08c1014363d26e7b87249c0e51fe74688`. The package provenance remains the earlier build commit.

Verified installer workflow: https://github.com/PawelTroka/Computator.NET/actions/runs/39090717532 (artifact `12122547257`). Exact SHA-256: MSI `d3ad9438e7e0bf3e6b06c7bcb502b168435e5bd634546fef87308ffce50d27ff`; EXE `72a2b01b984880707166e5d13518f598df062638a9847a9335ed5eb45b199b1e`; inventory `b38ff6ad61a75f3b1e319c2c18bd846d1115703622ed3021d74756640c7ba253`; authentic 2.3.2 historical EXE `30b107dced1c147b6f3ba9089b97d2131dd43aa195d5a23c3a95d9b150149d4a`.

Static verification found byte-identical bootstrapper/standalone MSI and matching inventory hashes for all 1,335 payload files. The original 2.3.2 EXE and embedded modern-Windows MSI matched their pinned identities. The unchanged `Clean-Machine-Harness-Validate` and `Clean-Machine-Prepare` targets passed on Linux; Windows Installer COM and Windows PowerShell 5.1 were not exercised there. The release-facing `Clean-Machine-Verify` correctly rejected the SDK-equipped hosted report with `Missing acceptance stage: clean-runtime-preflight`.

**Actual offline Windows Sandbox execution remains pending.** An x64 Windows host with Windows Sandbox enabled must run the existing `Clean-Machine-Prepare` target against the four exact inputs, open its newly generated `.wsb` with read-only inputs and isolated writable results, preserve the full results and logs, and pass `Clean-Machine-Verify` using both candidate hashes. Do not reuse a `.wsb` generated with Linux paths. This is not authorization to sign, publish or submit the release. See the existing instructions below for the full historical upgrade, intentional failure/rollback, repair and removal procedure.

