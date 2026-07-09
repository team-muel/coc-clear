# CoC-Clear — open the Unity editor with the same env the batch gate uses.
# NEVER set these inline in a shell one-liner: the `$` in `$env:` gets eaten by
# some tool wrappers, and the missing vars surface later as a UPM IPC failure
# with `error CS` = 0. Keep it in this file. (Playbook A1, E1)

$env:ALLUSERSPROFILE = 'C:\ProgramData'
$env:ProgramData     = 'C:\ProgramData'
$env:TMP             = $env:TEMP

$editor = 'C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe'
$proj   = Split-Path -Parent $PSScriptRoot

if (-not (Test-Path $editor)) { throw "Unity editor not found: $editor (check ProjectSettings/ProjectVersion.txt)" }

Start-Process -FilePath $editor -ArgumentList '-projectPath', $proj
Write-Output "unity-launched: $proj"
