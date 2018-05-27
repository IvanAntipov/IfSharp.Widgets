// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r "./packages/FAKE/tools/FakeLib.dll"
open System.IO
open Fake.FileUtils

open Fake
open System

Target "Build-Nuget" (fun _ -> 
    printfn "Create nuget package"
    
    CleanDirs ["Nuget/.build";]

    let createNugetPackage() =
        let result =
            ExecProcess (fun info ->
                info.FileName <- currentDirectory @@ ".paket" @@ "paket.exe"
                info.WorkingDirectory <- currentDirectory @@ "Nuget"
                info.Arguments <- "pack .nupkg") TimeSpan.MaxValue
        if result <> 0 then failwithf "Nuget package failed"

    createNugetPackage()              
    )

RunTargetOrDefault "Build-Nuget"