// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
Console.Write "Type a value:"

let str = Console.ReadLine()

printfn "You typed %s" str
let first = "32"
let numberVersion = System.Int32.Parse first 
printfn "Number %i" numberVersion // Output: Number 32

let first = "32"
let numberVersion =  int first 
printfn "Number %i" numberVersion

[<EntryPoint>]

let main argv =
    let aNumber = 0
    printfn "Here's a number %i" aNumber

    
    
    
    0 // return an integer exit code