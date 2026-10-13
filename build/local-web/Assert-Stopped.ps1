$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
foreach ($port in @(4190, 60999)) {
    $stopped = $false
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        $socket = [Net.Sockets.TcpClient]::new()
        try { $socket.Connect('127.0.0.1', $port) }
        catch [Net.Sockets.SocketException] { $stopped = $true }
        finally { $socket.Dispose() }
        if ($stopped) { break }
        Start-Sleep -Milliseconds 200
    }
    if (!$stopped) { throw "Owned local-package process still listens on port $port after the frontend stopped. No unrelated process will be killed." }
}
Write-Host 'Frontend and its owned API both stopped; no listener was reused or left running.'
