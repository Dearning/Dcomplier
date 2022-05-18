// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
// using System
let age = 66
if age > 65 then printfn "Senior citizen" else printfn "Citizen"

let cardValue = 1
let cardDescription = if cardValue = 1 then "Ace" elif cardValue = 14 then "Ace" else "A card"
printfn "%s" cardDescription

[<EntryPoint>]
let main argv =
    printfn "Welcome to the calculator program"
    // read input from the console and assign to `sum`
    printfn "Type the first number"
    let firstNo = Console.ReadLine()
    printfn "Type the second number"
    let secondNo = Console.ReadLine()
    printfn "First %s, Second %s" firstNo secondNo
    let sum = 0
    printfn "The sum is %i" sum 
    0