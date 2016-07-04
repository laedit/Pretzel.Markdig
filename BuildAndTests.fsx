#r @"System.IO"
#r @"System.IO.Compression"
#r @"System.IO.Compression.FileSystem"
#r @"System.Net"

open System.IO
open System.IO.Compression
open System.Net

let tempPath = Path.Combine(__SOURCE_DIRECTORY__, "temp")
let artifactsPath = Path.Combine(__SOURCE_DIRECTORY__, "artifacts")
let srcPath = Path.Combine(__SOURCE_DIRECTORY__, "src")
let markdigEngineZipPath = Path.Combine(artifactsPath, "MarkdigEngine.zip")
let testsitePath = Path.Combine(__SOURCE_DIRECTORY__, "testsite")
let pluginsPath = Path.Combine(testsitePath,"_plugins")
let postsPath = Path.Combine(testsitePath,"_posts")
let testGeneratedFilePath = Path.Combine(testsitePath, "_site/2015/11/06/MarkdigTest.html")

let CleanDir dirPath =
    if Directory.Exists dirPath then
        let di = new DirectoryInfo(dirPath)
        di.GetFiles() |> Array.iter (fun (file:FileInfo) -> file.Delete())
        di.GetDirectories() |> Array.iter (fun dir -> dir.Delete(true))
    else
        Directory.CreateDirectory dirPath |> ignore

    printfn "directory %s cleaned" dirPath

let StartProcess processPath args =
    let p = new System.Diagnostics.Process();
    p.StartInfo.FileName <- processPath
    p.StartInfo.Arguments <- args
    p.StartInfo.RedirectStandardOutput <- true
    p.StartInfo.RedirectStandardError <- true
    p.StartInfo.UseShellExecute <- false
    p.ErrorDataReceived.Add(fun d -> if d.Data <> null then printfn "%s" d.Data)
    p.OutputDataReceived.Add(fun d -> if d.Data <> null then printfn "%s" d.Data)
    p.StartInfo.CreateNoWindow <- true
    p.Start() |> ignore
    p.BeginErrorReadLine()
    p.BeginOutputReadLine()
    printfn "execute process '%s %s'" processPath args
    p.WaitForExit() |> ignore
    if p.ExitCode <> 0 then failwith ("error during execution of " + processPath)

let ExtractMarkdig(zipPath, markDigDllPath) =
    use zip = ZipFile.Open(zipPath, ZipArchiveMode.Read)
    let zipEntry = Seq.find(fun (entry:ZipArchiveEntry) -> entry.FullName = "lib/net40/Markdig.dll") zip.Entries
    zipEntry.ExtractToFile(markDigDllPath)

let GetMarkdig() =
    let zipPath = Path.Combine(tempPath, "Markdig.zip")
    let wc = new WebClient()
    wc.DownloadFile("https://www.nuget.org/api/v2/package/Markdig/", zipPath)
    ExtractMarkdig(zipPath, Path.Combine(tempPath, "Markdig.dll"))
    File.Delete zipPath
    printfn "%s" "Markdig extracted"

let Build =
    printfn "%s" "Start build"
    CleanDir tempPath
    CleanDir artifactsPath

    GetMarkdig()

    File.Copy(Path.Combine(srcPath, "MarkdigEngine.csx"), Path.Combine(tempPath, "MarkdigEngine.csx"))

    ZipFile.CreateFromDirectory(tempPath, markdigEngineZipPath)

    Directory.Delete(tempPath, true)
    printfn "%s" "MarkdigEngine.zip created"
    printfn "%s" "End build"

let AssertFileContains (fileData:string) wordToFind =
    if fileData.Contains wordToFind then
        let originalForeground = System.Console.ForegroundColor
        System.Console.ForegroundColor <- System.ConsoleColor.Green
        printfn "%s test passed" wordToFind
        System.Console.ForegroundColor = originalForeground
    else
        failwithf "Generated post doesn't contains '%s'" wordToFind

let Tests =
    printfn "%s" "Start tests"
    if not(Directory.Exists testsitePath) then
        StartProcess "C:/tools/Pretzel/Pretzel" "create testsite"
        File.AppendAllText(Path.Combine(postsPath, "2015-11-06-MarkdigTest.md"), """
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
        """)

    CleanDir pluginsPath
    ZipFile.ExtractToDirectory(markdigEngineZipPath, pluginsPath)
    StartProcess "C:/tools/Pretzel/Pretzel" "bake testsite --debug"
    let fileData = File.ReadAllText testGeneratedFilePath

    // table test
    AssertFileContains fileData "<table>" |> ignore

    // smiley test
    AssertFileContains fileData "😃" |> ignore

    // css class test
    AssertFileContains fileData "<p class=\"beautiful\">css class test </p>" |> ignore
    printfn "%s" "End tests"


match fsi.CommandLineArgs.Length with
| 1 -> // if there is only one args, it's fsi's path
    Build
    Tests
| _ -> match fsi.CommandLineArgs.[1] with
        | "build" -> Build
        | "tests" -> Tests
        | _ ->
            Build
            Tests