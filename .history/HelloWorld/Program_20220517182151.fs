﻿// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"

let name = "Chris"
// name <- "Luis" // not allowed

let mutable name = "Chris"
name <- "Luis" // this statement is now allowed