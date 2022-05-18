// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
using System
[<EntryPoint>]
let main argv =
    printfn "Welcome to the calculator program"
    printfn "Type the first number"
    let firstNo = Console.ReadLine()
    printfn "Type the second number"
    let secondNo = Console.ReadLine()
    printfn "First %s, Second %s" firstNo secondNo
    let sum = 0
    printfn "The sum is %i" sum
    0