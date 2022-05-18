// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"

[<EntryPoint>]
let main argv =
    printfn "Welcome to the calculator program"
    // read input from the console and assign to `sum`
    let sum = 0
    printfn "The sum is %i" sum
    0