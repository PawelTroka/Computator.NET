# Windows PowerShell 5.1; keep the UTF-8 BOM for Unicode fixtures.
# No dotnet, compiler, package manager or developer SDK is used here.
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$MsiPath,
    [Parameter(Mandatory)][string]$InventoryPath,
    [Parameter(Mandatory)][string]$CandidateBundlePath,
    [Parameter(Mandatory)][string]$HistoricalBundlePath,
    [Parameter(Mandatory)][string]$ResultDirectory,
    [switch]$DisposableMachine,
    [switch]$RequireNoModernRuntime
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'AcceptanceContracts.psm1') -Force
$hosted = $env:GITHUB_ACTIONS -eq 'true' -and $env:RUNNER_ENVIRONMENT -eq 'github-hosted'
$sandbox = $env:USERNAME -eq 'WDAGUtilityAccount'
if (!$DisposableMachine -or $env:COMPUTATOR_DISPOSABLE_INSTALL_TEST -ne '1' -or (!$hosted -and !$sandbox)) {
    throw 'Historical upgrades require explicit consent and a disposable GitHub-hosted Windows runner or Windows Sandbox guest.'
}
if (![Environment]::Is64BitProcess -or !(New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Use elevated 64-bit Windows PowerShell in the disposable guest.'
}
function SafeFile([string]$Path) {
    $item = Get-Item -LiteralPath $Path
    if ($item.PSIsContainer) { throw "Expected a file: $Path" }
    for ($part = $item; $null -ne $part; $part = if ($part -is [IO.FileInfo]) { $part.Directory } else { $part.Parent }) {
        if ($part.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Redirected acceptance input: $Path" }
    }
    return $item.FullName
}
$msi = SafeFile $MsiPath; $inventoryFile = SafeFile $InventoryPath
$candidateBundle = SafeFile $CandidateBundlePath; $historicalBundle = SafeFile $HistoricalBundlePath
$fixture = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'historical-2.3.2.json') -Raw | ConvertFrom-Json
if ((Get-FileHash -LiteralPath $historicalBundle).Hash -ne $fixture.sha256 -or (Get-Item -LiteralPath $historicalBundle).Length -ne $fixture.size) { throw 'Historical EXE differs from the pinned official asset.' }
[xml]$inventory = Get-Content -LiteralPath $inventoryFile -Raw
$candidateCode = [Guid]::Parse($inventory.Payload.ProductCode).ToString('B')
$legacyCode = $fixture.productCode
$root = Join-Path $env:TEMP ('Computator-Historical-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $root -Force | Out-Null
$results = [IO.Path]::GetFullPath($ResultDirectory)
if (Test-Path -LiteralPath (Join-Path $results 'acceptance.json')) { throw 'Refusing to mix historical acceptance runs in an existing report directory.' }
New-Item -ItemType Directory -Path $results -Force | Out-Null
for ($part = Get-Item -LiteralPath $results; $null -ne $part; $part = $part.Parent) {
    if ($part.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Acceptance results must not have redirected ancestors.' }
}
$report = [ordered]@{
    schemaVersion = 2; status = 'running'; historicalUpgrade = 'not-completed'; stages = @()
    startedUtc = [DateTime]::UtcNow.ToString('o'); machine = $env:COMPUTERNAME; os = [Environment]::OSVersion.VersionString
    environment = $(if ($hosted) { 'github-hosted-sdk-equipped' } else { 'windows-sandbox' })
    powershellVersion = $PSVersionTable.PSVersion.ToString()
    harnessSha256 = (Get-FileHash -LiteralPath $PSCommandPath).Hash
    noModernRuntimeRequired = [bool]$RequireNoModernRuntime
    candidateSha256 = (Get-FileHash -LiteralPath $msi).Hash
    candidateBundleSha256 = (Get-FileHash -LiteralPath $candidateBundle).Hash
    inventorySha256 = (Get-FileHash -LiteralPath $inventoryFile).Hash
    historicalBundleSha256 = (Get-FileHash -LiteralPath $historicalBundle).Hash
    candidateProductCode = $candidateCode; historicalProductCode = $legacyCode; historicalBundleCode = $fixture.bundleCode
    cleanupErrors = @()
}
$installer = New-Object -ComObject WindowsInstaller.Installer
$startedInstallation = $false
function Property([string]$Path, [string]$Name) {
    $db = $installer.OpenDatabase($Path, 0)
    $view = $null; $record = $null
    try {
        $view = $db.OpenView(('SELECT `Value` FROM `Property` WHERE `Property` = ''{0}''' -f $Name))
        # IDispatch void calls can emit explicit null pipeline entries in WinPS.
        # Returning those alongside the value turns scalar comparisons into filters.
        [void]$view.Execute()
        $record = $view.Fetch()
        if (!$record) { throw "Missing MSI property: $Name" }
        return [string]$record.StringData(1)
    } finally {
        if ($null -ne $record) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($record) }
        if ($null -ne $view) { [void]$view.Close(); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) }
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($db)
    }
}
function Installed([string]$Code) { return [int]$installer.ProductState($Code) -eq 5 }
function Registrations {
    @('HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*') |
        ForEach-Object { Get-ItemProperty $_ -ErrorAction SilentlyContinue } |
        Where-Object { $_.PSObject.Properties['DisplayName'] -and $_.DisplayName -eq 'Computator.NET' }
}
function Run([string]$Executable, [string[]]$Arguments, [int[]]$Expected = @(0,3010)) {
    $capture = Join-Path $results ('process-' + [Guid]::NewGuid().ToString('N'))
    $process = Start-Process -FilePath $Executable -ArgumentList $Arguments -PassThru -WindowStyle Hidden -RedirectStandardOutput ($capture + '.stdout.log') -RedirectStandardError ($capture + '.stderr.log')
    try {
        $null = $process.Handle
        if (!$process.WaitForExit(300000)) { $process.Kill(); throw "Timed out: $Executable; see $capture.*.log" }
        $process.Refresh()
        if ($process.ExitCode -notin $Expected) { throw "$Executable returned $($process.ExitCode); expected $($Expected -join ','); see $capture.*.log" }
        return [int]$process.ExitCode
    } finally { $process.Dispose() }
}
function Msi([string[]]$Arguments, [string]$Log, [int[]]$Expected = @(0,3010)) {
    Run 'msiexec.exe' ($Arguments + @('/qn','/norestart','/l*v', ('"' + (Join-Path $results $Log) + '"'))) $Expected | Out-Null
}
function Bundle([string]$Path, [string]$Operation, [string]$Log) {
    Run $Path @($Operation,'/quiet','/norestart','/log', ('"' + (Join-Path $results $Log) + '"')) | Out-Null
}
function ShortcutPaths {
    foreach ($kind in @('CommonDesktopDirectory','CommonPrograms')) {
        Join-Path ([Environment]::GetFolderPath($kind)) 'Computator.NET.lnk'
    }
}
function ShellSnapshot([string]$ExpectedExecutable) {
    $state = [ordered]@{}
    $shell = New-Object -ComObject WScript.Shell
    try {
        foreach ($path in @(ShortcutPaths)) {
            if (!(Test-Path -LiteralPath $path)) { throw "Missing shortcut: $path" }
            $link = $shell.CreateShortcut($path)
            if ($link.TargetPath -ne $ExpectedExecutable -or !(Test-Path -LiteralPath $link.TargetPath)) { throw "Shortcut does not point to the installed executable: $path -> $($link.TargetPath)" }
            $state[$path] = $link.TargetPath
        }
    } finally { [Runtime.InteropServices.Marshal]::FinalReleaseComObject($shell) | Out-Null }
    foreach ($extension in @('tsl','tslf')) {
        $key = [Microsoft.Win32.Registry]::ClassesRoot.OpenSubKey('.' + $extension)
        if (!$key) { throw "Missing .$extension association." }
        try { $progId = [string]$key.GetValue('') } finally { $key.Dispose() }
        if ($progId -ne ($extension + '.file')) { throw "Unexpected .$extension ProgID: $progId" }
        $key = [Microsoft.Win32.Registry]::ClassesRoot.OpenSubKey($progId + '\shell\Open\command')
        if (!$key) { throw "Missing $progId open command." }
        try { $command = [string]$key.GetValue('') } finally { $key.Dispose() }
        if ($command -notlike ('*"' + $ExpectedExecutable + '"*') -or $command -notlike '*"%1"*') { throw "Incorrect quoted document open command: $command" }
        $state[$extension] = $command
    }
    return ($state | ConvertTo-Json -Compress)
}
function InstalledExecutable {
    $shell = New-Object -ComObject WScript.Shell
    try { return $shell.CreateShortcut(@(ShortcutPaths)[0]).TargetPath }
    finally { [Runtime.InteropServices.Marshal]::FinalReleaseComObject($shell) | Out-Null }
}
function Launch([string]$Executable) {
    $start = New-Object Diagnostics.ProcessStartInfo
    $start.FileName = $Executable; $start.WorkingDirectory = Split-Path -Parent $Executable; $start.UseShellExecute = $false
    # 2.3.2 has no data-directory override. Both versions must use the real guest profile.
    $start.EnvironmentVariables.Remove('COMPUTATOR_DATA_DIRECTORY')
    $process = New-Object Diagnostics.Process; $process.StartInfo = $start
    $didStart = $false
    try {
        $didStart = $process.Start(); if (!$didStart) { throw 'Installed executable did not start.' }
        $until = [DateTime]::UtcNow.AddSeconds(90)
        do {
            $process.Refresh()
            if ($process.HasExited) { throw "Installed application exited early: $($process.ExitCode)" }
            if ($process.MainWindowHandle -ne [IntPtr]::Zero -and $process.MainWindowTitle.StartsWith('Computator.NET')) { break }
            Start-Sleep -Milliseconds 200
        } while ([DateTime]::UtcNow -lt $until)
        if ($process.MainWindowHandle -eq [IntPtr]::Zero -or !$process.MainWindowTitle.StartsWith('Computator.NET') -or !$process.WaitForInputIdle(10000)) { throw 'Installed application did not show a responsive main window.' }
        if (!$process.CloseMainWindow() -or !$process.WaitForExit(30000) -or $process.ExitCode -ne 0) { throw 'Installed application did not shut down normally.' }
    } finally {
        if ($didStart -and !$process.HasExited) { $process.Kill(); $process.WaitForExit() }
        $process.Dispose()
    }
}
function Snapshot([string]$Directory) {
    $hashes = @{}
    foreach ($file in Get-ChildItem -LiteralPath $Directory -Recurse -File) {
        if ($file.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'A redirected file appeared in the acceptance payload.' }
        $hashes[$file.FullName] = (Get-FileHash -LiteralPath $file.FullName).Hash
    }
    return $hashes
}
function AssertHashes($Hashes) {
    foreach ($path in $Hashes.Keys) {
        if (!(Test-Path -LiteralPath $path) -or (Get-FileHash -LiteralPath $path).Hash -ne $Hashes[$path]) { throw "Preservation/hash check failed: $path" }
    }
}
function LegacyProfile([string]$Directory, [switch]$Verify) {
    $arguments = @('-NoProfile','-File', ('"' + (Join-Path $PSScriptRoot 'Write-HistoricalProfile.ps1') + '"'), '-InstallDirectory', ('"' + $Directory + '"'))
    if ($Verify) { $arguments += '-VerifyOnly' }
    Run (Join-Path $env:WINDIR 'SysWOW64/WindowsPowerShell/v1.0/powershell.exe') $arguments @(0) | Out-Null
}
try {
    $runtimeRoots = @((Join-Path $env:ProgramFiles 'dotnet'), (Join-Path ${env:ProgramFiles(x86)} 'dotnet'), (Join-Path $env:USERPROFILE '.dotnet'))
    $runtimeRoots += @(Get-ChildItem Env: | Where-Object Name -like 'DOTNET_ROOT*' | ForEach-Object Value)
    foreach ($view in @([Microsoft.Win32.RegistryView]::Registry32,[Microsoft.Win32.RegistryView]::Registry64)) {
        $hive = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, $view)
        try {
            foreach ($arch in @('x86','x64','arm64')) {
                $key = $hive.OpenSubKey('SOFTWARE\dotnet\Setup\InstalledVersions\' + $arch)
                if ($key) { try { $location = $key.GetValue('InstallLocation'); if ($location) { $runtimeRoots += $location } } finally { $key.Dispose() } }
            }
        } finally { $hive.Dispose() }
    }
    $dotnet = Get-Command dotnet.exe -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($dotnet) { $runtimeRoots += Split-Path -Parent $dotnet.Source }
    $present = @($runtimeRoots | Sort-Object -Unique | Where-Object { (Test-Path (Join-Path $_ 'sdk')) -or (Test-Path (Join-Path $_ 'shared')) })
    $report.modernRuntimeLocations = $present
    if ($RequireNoModernRuntime -and ($present.Count -gt 0 -or $dotnet)) { throw 'A modern SDK/runtime was found; this machine cannot certify clean no-SDK acceptance.' }
    if ($RequireNoModernRuntime) { $report.stages += 'clean-runtime-preflight' }
    if (@(Registrations).Count -gt 0 -or (Installed $legacyCode) -or (Installed $candidateCode)) { throw 'Refusing to touch an existing Computator.NET installation.' }
    $profile = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Computator.NET'
    if (Test-Path -LiteralPath $profile) { throw 'Refusing to touch an existing Computator.NET user profile.' }
    $identity = [ordered]@{}
    foreach ($name in @('ProductCode','UpgradeCode','ProductVersion')) {
        $value = @(Property $msi $name)
        if ($value.Count -ne 1 -or $value[0] -isnot [string]) { throw "MSI property reader did not return one string: $name" }
        $identity[$name] = $value[0]
    }
    $report.candidateMsiIdentity = $identity
    if ($identity.ProductCode -ne $candidateCode -or $identity.UpgradeCode -ne $fixture.upgradeCode -or
        $identity.ProductVersion -ne $inventory.Payload.Version -or $candidateCode -eq $legacyCode) { throw 'Candidate MSI/inventory identity does not describe a major upgrade of the historical product.' }
    if (@($inventory.Payload.File).Count -lt 1) { throw 'Empty candidate inventory.' }
    $startedInstallation = $true
    Bundle $historicalBundle '/install' 'historical-bundle-install.log'
    if (!(Installed $legacyCode) -or !(@(Registrations | Where-Object PSChildName -eq $fixture.bundleCode).Count -eq 1)) { throw 'The original EXE did not register both its historical MSI and Burn bundle.' }
    $oldExecutable = InstalledExecutable
    $oldDirectory = Split-Path -Parent $oldExecutable
    $report.historicalInstallDirectory = $oldDirectory
    $oldShell = ShellSnapshot $oldExecutable
    LegacyProfile $oldDirectory
    $documents = @(
        (Join-Path $profile 'Acceptance scripts/Zażółć gęślą - keep.tsl'),
        (Join-Path $profile 'Acceptance functions/Zażółć gęślą - keep.tslf'),
        (Join-Path $root 'External documents/another file.tsl'),
        (Join-Path $root 'External documents/another file.tslf')
    )
    foreach ($path in $documents) {
        New-Item -ItemType Directory -Path (Split-Path -Parent $path) -Force | Out-Null
        $content = if ([IO.Path]::GetExtension($path) -eq '.tslf') { 'real AcceptanceIdentity(real x) { return x; }' } else { 'return 2+3;' }
        [IO.File]::WriteAllText($path, $content, (New-Object Text.UTF8Encoding($false)))
    }
    Launch $oldExecutable
    LegacyProfile $oldDirectory -Verify
    $preserved = @{}
    foreach ($path in ($documents + @(Join-Path $profile 'settings.dat'))) { $preserved[$path] = (Get-FileHash -LiteralPath $path).Hash }
    $oldFiles = Snapshot $oldDirectory
    $report.historicalPayloadFiles = $oldFiles.Count
    $report.stages += 'official-exe-install-profile-and-launch'
    $negative = Join-Path $root 'NEGATIVE-TEST-ONLY.msi'
    & (Join-Path $PSScriptRoot 'New-FailingUpgrade.ps1') -Source $msi -Destination $negative
    $report.negativeMsiSha256 = (Get-FileHash -LiteralPath $negative).Hash
    Msi @('/i', ('"' + $negative + '"')) 'failed-upgrade.log' @(1603)
    $failureLog = Get-Content -LiteralPath (Join-Path $results 'failed-upgrade.log') -Raw
    $report.rollbackLogProof = Assert-HistoricalRollbackLog $failureLog $legacyCode $candidateCode
    if (!(Installed $legacyCode) -or (Installed $candidateCode)) { throw 'Failed upgrade did not restore the old MSI registration.' }
    if (@(Registrations | Where-Object PSChildName -eq $fixture.bundleCode).Count -ne 1) { throw 'Rollback lost the historical bundle registration.' }
    AssertHashes $oldFiles; AssertHashes $preserved
    if ((ShellSnapshot $oldExecutable) -ne $oldShell) { throw 'Rollback changed historical shortcuts or associations.' }
    Launch $oldExecutable
    LegacyProfile $oldDirectory -Verify
    AssertHashes $preserved
    $report.stages += 'historical-install-and-failed-upgrade-rollback'
    Bundle $candidateBundle '/install' 'candidate-bundle-upgrade.log'
    if (!(Installed $candidateCode) -or (Installed $legacyCode) -or @(Registrations | Where-Object PSChildName -eq $fixture.bundleCode).Count -ne 0) { throw 'EXE upgrade left the historical MSI or Burn bundle registered.' }
    $newExecutable = InstalledExecutable
    $newDirectory = Split-Path -Parent $newExecutable
    $report.candidateInstallDirectory = $newDirectory
    if ($newDirectory -eq $oldDirectory) { throw 'The x86-to-x64 default-directory migration did not occur.' }
    ShellSnapshot $newExecutable | Out-Null
    foreach ($file in $inventory.Payload.File) {
        $path = [IO.Path]::GetFullPath((Join-Path $newDirectory $file.Path))
        if (!$path.StartsWith($newDirectory.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Inventory path escapes the installation directory.' }
        if (!(Test-Path -LiteralPath $path) -or (Get-FileHash -LiteralPath $path).Hash -ne $file.SHA256) { throw "Candidate installed payload differs: $($file.Path)" }
    }
    $report.candidatePayloadFiles = @($inventory.Payload.File).Count
    AssertHashes $preserved
    Launch $newExecutable
    AssertHashes $preserved
    $jsonPath = Join-Path $profile 'settings.json'
    $settings = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($settings.SchemaVersion -ne 1 -or $settings.Language -ne 'pl-PL' -or !$settings.ShowReturnTypeInExpression -or
        $settings.ShowParametersTypeInExpression -or $settings.ShowReturnTypeInScripting -or $settings.ShowParametersTypeInScripting -or
        $settings.ExpressionFont.Size -ne 18.25 -or $settings.ScriptingFont.Size -ne 14.5 -or
        $settings.ScriptingDirectory -ne (Join-Path $profile 'Acceptance scripts') -or
        $settings.CustomFunctionsDirectory -ne (Join-Path $profile 'Acceptance functions') -or
        $settings.WorkingDirectory -ne (Join-Path $profile 'Acceptance workspace')) { throw 'Candidate did not import the actual historical non-default preferences.' }
    $preserved[$jsonPath] = (Get-FileHash -LiteralPath $jsonPath).Hash
    $report.stages += 'candidate-install-payload-and-launch','real-settings-migration','shortcuts-and-file-associations'
    $repairPath = Join-Path $newDirectory 'Static/functions.xml'
    $repairHash = (Get-FileHash -LiteralPath $repairPath).Hash
    Remove-Item -LiteralPath $repairPath
    Msi @('/fa', $candidateCode) 'candidate-repair.log'
    if ((Get-FileHash -LiteralPath $repairPath).Hash -ne $repairHash) { throw 'Repair did not restore the exact candidate file.' }
    AssertHashes $preserved
    ShellSnapshot $newExecutable | Out-Null
    Bundle $candidateBundle '/uninstall' 'candidate-bundle-uninstall.log'
    if ((Installed $candidateCode) -or (Installed $legacyCode) -or @(Registrations).Count -ne 0 -or (Test-Path -LiteralPath $newExecutable)) { throw 'Uninstall left a product, bundle or executable behind.' }
    foreach ($path in @(ShortcutPaths)) { if (Test-Path -LiteralPath $path) { throw "Uninstall left shortcut: $path" } }
    foreach ($extension in @('tsl','tslf')) {
        $key = [Microsoft.Win32.Registry]::ClassesRoot.OpenSubKey($extension + '.file\shell\Open\command')
        if ($key) { $key.Dispose(); throw "Uninstall left the .$extension open command." }
    }
    AssertHashes $preserved
    $report.preservedFiles = @($preserved.Keys | Sort-Object | ForEach-Object { @{ path = $_; sha256 = $preserved[$_] } })
    $report.stages += 'repair-uninstall-and-user-data-preservation'
    $report.historicalUpgrade = 'passed'; $report.status = 'passed'
} catch {
    $report.status = 'failed'; $report.error = $_.Exception.ToString(); $report.errorLocation = $_.ScriptStackTrace
    Write-Error $_ -ErrorAction Continue
} finally {
    if ($startedInstallation) {
        foreach ($bundle in @(@($candidateBundle,'candidate'),@($historicalBundle,'historical'))) {
            try {
                if (@(Registrations | Where-Object { $_.PSObject.Properties['BundleCachePath'] -and $_.DisplayVersion -eq $(if ($bundle[1] -eq 'historical') { $fixture.productVersion } else { $inventory.Payload.Version }) }).Count -gt 0) {
                    Bundle $bundle[0] '/uninstall' ('cleanup-' + $bundle[1] + '.log')
                }
            } catch { $report.cleanupErrors += $_.Exception.Message }
        }
        foreach ($code in @($candidateCode,$legacyCode)) {
            try { if (Installed $code) { Msi @('/x',$code) ('cleanup-' + $code.Trim('{}') + '.log') } }
            catch { $report.cleanupErrors += $_.Exception.Message }
        }
        if (@(Registrations).Count -gt 0) { $report.cleanupErrors += 'Computator.NET registrations remain; discard the guest.' }
    }
    if ($report.cleanupErrors.Count -gt 0) { $report.status = 'failed' }
    $report.completedUtc = [DateTime]::UtcNow.ToString('o')
    $report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $results 'acceptance.json') -Encoding UTF8
    [Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer) | Out-Null
    # Keep diagnostics and fixture files in this disposable guest. Do not recursively
    # delete user-profile paths or export the intentionally modified MSI as a candidate.
    if (Test-Path -LiteralPath (Join-Path $root 'NEGATIVE-TEST-ONLY.msi')) { Remove-Item -LiteralPath (Join-Path $root 'NEGATIVE-TEST-ONLY.msi') -Force }
}
if ($report.status -ne 'passed') { exit 1 }
Write-Host 'Historical EXE -> failed MSI rollback -> candidate EXE upgrade, settings migration, documents, shell integration, repair and uninstall passed.'
