# Windows PowerShell 5.1 compatible: the clean guest must not need PowerShell 7,
# a modern shared runtime, Node or a development SDK to test the installed product.
# Keep the UTF-8 BOM: Windows PowerShell otherwise misparses Unicode fixture text.
[CmdletBinding()]
param([Parameter(Mandatory)][string]$InputDirectory, [Parameter(Mandatory)][string]$ResultDirectory, [switch]$DisposableMachine)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (!$DisposableMachine -or $env:USERNAME -ne 'WDAGUtilityAccount') { throw 'This harness only runs in an explicitly launched Windows Sandbox guest.' }
if (!(New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'The disposable guest must run elevated.' }
$env:COMPUTATOR_DISPOSABLE_INSTALL_TEST = '1'
New-Item -ItemType Directory -Path $ResultDirectory -Force | Out-Null
$report = [ordered]@{ schemaVersion = 1; status = 'running'; startedUtc = [DateTime]::UtcNow.ToString('o'); machine = $env:COMPUTERNAME; os = [Environment]::OSVersion.VersionString; stages = @(); historicalUpgrade = 'not-requested' }
$plan = Get-Content -LiteralPath (Join-Path $InputDirectory 'plan.json') -Raw | ConvertFrom-Json
$root = Join-Path $env:TEMP ('Computator-Acceptance-' + [Guid]::NewGuid().ToString('N'))
$application = Join-Path $root 'application'; New-Item -ItemType Directory -Path $root -Force | Out-Null
$msi = Join-Path $InputDirectory 'candidate.msi'
$inventoryPath = Join-Path $InputDirectory 'payload.xml'
$knownProducts = @()
$installer = New-Object -ComObject WindowsInstaller.Installer
function Property([string]$Path, [string]$Name) {
    $database = $installer.OpenDatabase($Path, 0)
    $view = $null; $record = $null
    try {
        $view = $database.OpenView(('SELECT `Value` FROM `Property` WHERE `Property` = ''{0}''' -f $Name))
        # Suppress COM void returns: otherwise WinPS includes nulls in this result.
        [void]$view.Execute()
        $record = $view.Fetch()
        if (!$record) { throw "Missing MSI property: $Name" }
        return [string]$record.StringData(1)
    } finally {
        if ($null -ne $record) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($record) }
        if ($null -ne $view) { [void]$view.Close(); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) }
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($database)
    }
}
function Installed([string]$Code) { return [int]$installer.ProductState($Code) -eq 5 }
function Msi([string[]]$Arguments, [string]$Log, [int[]]$Expected = @(0, 3010)) {
    $process = Start-Process msiexec.exe -ArgumentList ($Arguments + @('/qn', '/norestart', '/l*v', ('"' + (Join-Path $ResultDirectory $Log) + '"'))) -WindowStyle Hidden -Wait -PassThru
    if ($process.ExitCode -notin $Expected) { throw "Windows Installer returned $($process.ExitCode); see $Log." }
}
function Launch([string]$Directory) {
    $start = New-Object Diagnostics.ProcessStartInfo
    $start.FileName = Join-Path $Directory 'Computator.NET.exe'; $start.WorkingDirectory = $Directory; $start.UseShellExecute = $false
    $start.EnvironmentVariables['COMPUTATOR_DATA_DIRECTORY'] = $profile
    $process = New-Object Diagnostics.Process; $process.StartInfo = $start
    try {
        if (!$process.Start()) { throw 'Installed application did not start.' }
        $until = [DateTime]::UtcNow.AddSeconds(60)
        do {
            $process.Refresh()
            if ($process.HasExited) { throw "Installed product exited early ($($process.ExitCode))." }
            if ($process.MainWindowHandle -ne [IntPtr]::Zero -and $process.MainWindowTitle.StartsWith('Computator.NET')) { break }
            Start-Sleep -Milliseconds 200
        } while ([DateTime]::UtcNow -lt $until)
        if ($process.MainWindowHandle -eq [IntPtr]::Zero -or !$process.WaitForInputIdle(10000)) { throw 'Installed application did not become responsive.' }
        if (!$process.CloseMainWindow() -or !$process.WaitForExit(15000) -or $process.ExitCode -ne 0) { throw 'Installed application did not shut down normally.' }
    } finally { if ($process.Id -and !$process.HasExited) { $process.Kill(); $process.WaitForExit() }; $process.Dispose() }
}
try {
    if ($plan.schemaVersion -ne 1) { throw 'Unsupported acceptance plan.' }
    foreach ($pair in @(@($msi, $plan.candidateSha256), @($inventoryPath, $plan.inventorySha256))) {
        if ((Get-FileHash -LiteralPath $pair[0] -Algorithm SHA256).Hash -ne $pair[1]) { throw 'Acceptance input hash mismatch.' }
    }
    foreach ($directory in @((Join-Path $env:ProgramFiles 'dotnet'), (Join-Path ${env:ProgramFiles(x86)} 'dotnet'))) {
        if ((Test-Path (Join-Path $directory 'sdk')) -or (Test-Path (Join-Path $directory 'shared/Microsoft.NETCore.App'))) { throw 'This guest already has a modern SDK/shared runtime; it cannot certify the clean-machine profile.' }
    }
    $existing = @('HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*') | ForEach-Object { Get-ItemProperty $_ -ErrorAction SilentlyContinue } | Where-Object { $_.PSObject.Properties['DisplayName'] -and $_.DisplayName -eq 'Computator.NET' }
    if ($existing) { throw 'The guest already contains Computator.NET.' }
    $report.stages += 'clean-runtime-preflight'
    [xml]$inventory = Get-Content -LiteralPath $inventoryPath -Raw
    $candidateCode = [Guid]::Parse((Property $msi 'ProductCode')).ToString('B')
    if ($candidateCode -ne $plan.candidateProductCode -or (Property $msi 'UpgradeCode') -ne $plan.expectedUpgradeCode) { throw 'Candidate product/upgrade identity differs from the acceptance plan.' }
    $knownProducts += $candidateCode
    $profile = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Computator.NET'
    New-Item -ItemType Directory -Path $profile -Force | Out-Null
    $document = Join-Path $profile 'Zażółć gęślą — keep.tslf'
    [IO.File]::WriteAllText($document, 'static real Probe(real x) { return x/2; }', (New-Object Text.UTF8Encoding($false)))
    $documentHash = (Get-FileHash -LiteralPath $document).Hash
    if ($plan.historicalUpgradeRequested) {
        $legacy = Join-Path $InputDirectory 'legacy.msi'
        if ((Get-FileHash -LiteralPath $legacy).Hash -ne $plan.legacySha256 -or
            (Property $legacy 'UpgradeCode') -ne $plan.expectedUpgradeCode -or
            (Property $legacy 'ProductVersion') -ne $plan.expectedLegacyVersion) { throw 'The historical artifact does not match the approved product/version/hash.' }
        $legacyCode = [Guid]::Parse((Property $legacy 'ProductCode')).ToString('B')
        if ($legacyCode -eq $candidateCode) { throw 'An upgrade must not reuse the historical ProductCode.' }
        $knownProducts += $legacyCode
        Msi @('/i', ('"' + $legacy + '"'), ('INSTALLDIR="' + $application + '"')) 'legacy-install.log'
        if (!(Installed $legacyCode)) { throw 'Historical product registration is missing.' }
        Launch $application
        $oldExecutableHash = (Get-FileHash -LiteralPath (Join-Path $application 'Computator.NET.exe')).Hash
        $negative = Join-Path $root 'NEGATIVE-TEST-ONLY.msi'
        & (Join-Path $InputDirectory 'New-FailingUpgrade.ps1') -Source $msi -Destination $negative
        Msi @('/i', ('"' + $negative + '"'), ('INSTALLDIR="' + $application + '"')) 'failed-upgrade.log' @(1603)
        if (!(Installed $legacyCode) -or (Installed $candidateCode) -or
            (Get-FileHash -LiteralPath (Join-Path $application 'Computator.NET.exe')).Hash -ne $oldExecutableHash) { throw 'Failed upgrade did not restore the historical registration/executable.' }
        Launch $application
        $report.stages += 'historical-install-and-failed-upgrade-rollback'
        $report.historicalUpgrade = 'rollback-verified'
    }
    Msi @('/i', ('"' + $msi + '"'), ('INSTALLDIR="' + $application + '"')) 'candidate-install.log'
    if (!(Installed $candidateCode)) { throw 'Candidate product registration is missing.' }
    if ($plan.historicalUpgradeRequested -and (Installed $legacyCode)) { throw 'Successful upgrade left the old product registered.' }
    foreach ($file in $inventory.Payload.File) {
        $path = Join-Path $application $file.Path
        if (!(Test-Path -LiteralPath $path) -or (Get-FileHash -LiteralPath $path).Hash -ne $file.SHA256) { throw "Installed candidate payload differs: $($file.Path)" }
    }
    Launch $application
    $report.stages += 'candidate-install-payload-and-launch'
    Remove-Item -LiteralPath (Join-Path $application 'Static/functions.xml')
    Msi @('/fa', $candidateCode) 'repair.log'
    if (!(Test-Path -LiteralPath (Join-Path $application 'Static/functions.xml'))) { throw 'Repair failed to restore the removed application data file.' }
    Msi @('/x', $candidateCode) 'uninstall.log'
    if ((Installed $candidateCode) -or (Test-Path (Join-Path $application 'Computator.NET.exe'))) { throw 'Uninstall left the product installed.' }
    if (!(Test-Path -LiteralPath $document) -or (Get-FileHash -LiteralPath $document).Hash -ne $documentHash) { throw 'Installation/upgrade/repair/uninstall changed external user data.' }
    $report.stages += 'repair-uninstall-and-user-data-preservation'
    if ($plan.historicalUpgradeRequested) { $report.historicalUpgrade = 'passed' }
    $report.status = 'passed'
} catch {
    $report.status = 'failed'; $report.error = $_.Exception.Message
    Write-Error $_ -ErrorAction Continue
} finally {
    foreach ($code in $knownProducts) {
        try { if (Installed $code) { Msi @('/x', $code) ('cleanup-' + $code.Trim('{}') + '.log') } } catch { $report.cleanupError = $_.Exception.Message; $report.status = 'failed' }
    }
    $report.completedUtc = [DateTime]::UtcNow.ToString('o')
    $report.candidateSha256 = $plan.candidateSha256
    $report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $ResultDirectory 'acceptance.json') -Encoding UTF8
    [Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer) | Out-Null
}
if ($report.status -ne 'passed') { exit 1 }
