$ErrorActionPreference = "Stop"

$currentPath = (Get-Item -Path ".\" -Verbose).FullName
$artifactsFolder = "$currentPath\artifacts"
$tempFolder = "$currentPath\temp"

tools\nuget install Markdig -ExcludeVersion

Add-Type -assembly "system.io.compression.filesystem"

If (Test-Path $tempFolder){
    Remove-Item $tempFolder -recurse
    Start-Sleep -milliseconds 100
}

New-Item $tempFolder -type directory
Start-Sleep -milliseconds 100

If (Test-Path $artifactsFolder){
    Remove-Item $artifactsFolder -recurse
    Start-Sleep -milliseconds 100
}

New-Item $artifactsFolder -type directory
Start-Sleep -milliseconds 100

Copy-Item "Markdig\lib\net40\Markdig.dll" "$tempFolder\Markdig.dll"
Copy-Item "src\MarkdigEngine.csx" "$tempFolder\MarkdigEngine.csx"

[io.compression.zipfile]::CreateFromDirectory("$tempFolder", "$artifactsFolder\MarkdigEngine.zip")
Remove-Item $tempFolder -recurse