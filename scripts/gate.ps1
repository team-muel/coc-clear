# CoC-Clear — the merge gate. Batchmode EditMode tests, all of them.
#
# Contract (Playbook A):
#   A1  env parity with launch.ps1 - ALLUSERSPROFILE + ProgramData + TMP.
#       Missing ProgramData => UPM local server fails => exit 1, no results XML,
#       and zero `error CS` lines, which reads like a phantom failure.
#   A2  NEVER pass -quit together with -runTests. The editor quits before the
#       test runner finishes and reports success with nothing run.
#   A3  Single project lock: close the Unity editor before running this.
#
# Usage:  pwsh -NoProfile -File scripts/gate.ps1
# Exit 0 only if every EditMode test passed.

$ErrorActionPreference = 'Stop'

$env:ALLUSERSPROFILE = 'C:\ProgramData'
$env:ProgramData     = 'C:\ProgramData'
$env:TMP             = $env:TEMP

$editor  = 'C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe'
$proj    = Split-Path -Parent $PSScriptRoot
$results = Join-Path $proj '_scratch\editmode-results.xml'
$log     = Join-Path $proj '_scratch\editmode.log'

New-Item -ItemType Directory -Force -Path (Split-Path $results) | Out-Null
Remove-Item $results -ErrorAction SilentlyContinue

if (Get-Process Unity -ErrorAction SilentlyContinue) {
    throw 'Unity editor is running. Close it first (single project lock).'
}

# Unity.exe is a GUI-subsystem binary: `& $editor ...` returns IMMEDIATELY and
# leaves $LASTEXITCODE empty, so the gate "fails" before Unity has even started.
# Start-Process -Wait -PassThru is the only shape that actually blocks. (Playbook H1)
$unityArgs = @(
    '-batchmode', '-projectPath', $proj,
    '-runTests', '-testPlatform', 'EditMode',
    '-testResults', $results, '-logFile', $log
)
$p = Start-Process -FilePath $editor -ArgumentList $unityArgs -Wait -PassThru -NoNewWindow
$code = $p.ExitCode

if (-not (Test-Path $results)) {
    Write-Host "GATE FAIL: no results XML. exit=$code" -ForegroundColor Red
    Write-Host "Check $log for 'Unity Package Manager' errors (see A1)."
    exit 1
}

[xml]$xml = Get-Content $results
$r = $xml.'test-run'
Write-Host "EditMode: $($r.passed)/$($r.total)  failed=$($r.failed)  skipped=$($r.skipped)"

# Batch runs touch ProjectSettings; keep the tree clean so PRs stay honest.
git -C $proj checkout -- ProjectSettings/ 2>$null

if ($r.failed -ne '0' -or $code -ne 0) {
    Write-Host 'GATE FAIL' -ForegroundColor Red
    exit 1
}
Write-Host 'GATE PASS' -ForegroundColor Green
exit 0
