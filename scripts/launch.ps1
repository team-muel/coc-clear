# CoC-Clear — open the Unity editor with the same env the batch gate uses.
# NEVER set these inline in a shell one-liner: the `$` in `$env:` gets eaten by
# some tool wrappers, and the missing vars surface later as a UPM IPC failure
# with `error CS` = 0. Keep it in this file. (Playbook A1, E1)

$env:ALLUSERSPROFILE = 'C:\ProgramData'
$env:ProgramData     = 'C:\ProgramData'
$env:TMP             = $env:TEMP

$proj   = Split-Path -Parent $PSScriptRoot
$versionFile = Join-Path $proj 'ProjectSettings\ProjectVersion.txt'
$version = (Select-String -LiteralPath $versionFile -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
$editor = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"

if (-not (Test-Path $editor)) { throw "Unity editor not found: $editor (install the version in ProjectSettings/ProjectVersion.txt)" }

Start-Process -FilePath $editor -ArgumentList "-projectPath `"$proj`""
Write-Output "unity-launched: $proj"
