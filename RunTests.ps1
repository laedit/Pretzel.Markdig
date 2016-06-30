$ErrorActionPreference = "Stop"

trap {
    "$_.Exception|format-list -force"
    Throw $_.Exception
    continue
}

$currentPath = (Get-Item -Path ".\" -Verbose).FullName
$testsiteFolder = "$currentPath\testsite"
$pluginsFolder = "$testsiteFolder\_plugins"
$postsFolder = "$testsiteFolder\_posts"
$testGeneratedFile = "$testsiteFolder\_site\2015\11\06\MarkdigTest.html"

If (-Not (Test-Path $testsiteFolder)){
    & "C:\tools\Pretzel\pretzel" create testsite

    if ($LASTEXITCODE -ne 0) 
    {
        Exit
    }

    New-Item "$postsFolder/2015-11-06-MarkdigTest.md" -type file -value @"
---
layout: post
title: "My First Post"
author: "Author"
comments: true
---

## Hello world...

```cs
static void Main() 
{
    Console.WriteLine("Hello World!");
}
```

This is my first post on the site!

First Header  | Second Header
------------- | -------------
Content Cell  | Content Cell
Content Cell  | Content Cell 

:)

css class test {.beautiful}
"@
}

If (Test-Path $pluginsFolder){
    Remove-Item $pluginsFolder -recurse
    Start-Sleep -milliseconds 100
}
New-Item $pluginsFolder -type directory
Start-Sleep -milliseconds 100
Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::ExtractToDirectory("$currentPath/artifacts/MarkdigEngine.zip", $pluginsFolder)

& "C:\tools\Pretzel\pretzel" bake testsite --debug

if ($LASTEXITCODE -ne 0) 
{
    Exit $LASTEXITCODE
}

function AssertFileContains
{
    $file = Get-Content $args[0]
    $wordToFind = $args[1]
    
    $containsWord = $file | %{$_ -match $wordToFind}
    
    If($containsWord -contains $true)
    {
        $originalForeground = $host.UI.RawUI.ForegroundColor
        $host.UI.RawUI.ForegroundColor = "green"
        Write-Host $args[1] + " test passed"
        $host.UI.RawUI.ForegroundColor = $originalForeground
    }
    Else {
        throw "Generated post doesn't contains '" + $args[1] + "'"
    }
}

# table test
AssertFileContains $testGeneratedFile "<table class=""table"">"

# smiley test
AssertFileContains $testGeneratedFile "😃"

# css class test
AssertFileContains $testGeneratedFile "<p class=""beautiful"">css class test </p>"
