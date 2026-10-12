[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Path,
    [Parameter(Mandatory)][ValidateRange(1, 10000000)][int]$MinimumTests,
    [string[]]$AllowedSkippedTests = @()
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# An exit code of zero is not sufficient: test adapters can discover no tests.
# Parse the actual result entries, including NUnit's NotExecuted outcomes (its
# TRX summary does not always include these in the notExecuted counter).
$settings = [System.Xml.XmlReaderSettings]::new()
$settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
$settings.XmlResolver = $null
$reader = [System.Xml.XmlReader]::Create((Resolve-Path -LiteralPath $Path).Path, $settings)
$document = [System.Xml.XmlDocument]::new()
$document.XmlResolver = $null
try { $document.Load($reader) } finally { $reader.Dispose() }
$namespaces = [System.Xml.XmlNamespaceManager]::new($document.NameTable)
$namespaces.AddNamespace('t', 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010')
$summary = $document.SelectSingleNode('/t:TestRun/t:ResultSummary', $namespaces)
if ($null -eq $summary -or $summary.GetAttribute('outcome') -notin @('Completed', 'Passed')) {
    throw "The test run did not complete successfully: $Path"
}
$results = @($document.SelectNodes('/t:TestRun/t:Results/t:UnitTestResult', $namespaces))
if ($results.Count -lt $MinimumTests) {
    throw "Test discovery regressed: $($results.Count) results, expected at least $MinimumTests in $Path"
}
$definitions = @{}
foreach ($definition in $document.SelectNodes('/t:TestRun/t:TestDefinitions/t:UnitTest', $namespaces)) {
    $definitions[$definition.GetAttribute('id')] = $definition.SelectSingleNode('t:TestMethod', $namespaces)
}
$seen = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$passed = 0
$skipped = 0
foreach ($result in $results) {
    $id = $result.GetAttribute('testId')
    if (!$id -or !$seen.Add($id) -or !$definitions.ContainsKey($id) -or $null -eq $definitions[$id]) {
        throw "Missing or duplicate test identity in $Path"
    }
    $identity = $definitions[$id].GetAttribute('className') + '.' + $result.GetAttribute('testName')
    switch ($result.GetAttribute('outcome')) {
        'Passed' { $passed++ }
        'NotExecuted' {
            if ($identity -cnotin $AllowedSkippedTests) { throw "Unexpected skipped test: $identity" }
            $skipped++
        }
        default { throw "Unsuccessful test: $identity ($($result.GetAttribute('outcome')))" }
    }
}
$counters = $summary.SelectSingleNode('t:Counters', $namespaces)
if ($null -eq $counters -or [int]$counters.GetAttribute('total') -ne $results.Count -or
    [int]$counters.GetAttribute('passed') -ne $passed) {
    throw "Inconsistent test result counters: $Path"
}
Write-Host "Verified $($results.Count) test results: $passed passed, $skipped documented skips."
