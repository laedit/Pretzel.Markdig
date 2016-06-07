$ErrorActionPreference = "Stop"

Write-Host You need to have pretzel and pretzel.ScriptCs installed

Invoke-Expression .\Build.ps1

Invoke-Expression .\RunTests.ps1