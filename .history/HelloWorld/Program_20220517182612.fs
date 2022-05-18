// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"

let name = "Chris"
// name <- "Luis" // not allowed

let mutable name = "Chris"
name <- "Luis" // this statement is now allowed

let name = "Luis"
let company = "Microsoft"
printfn $"Name: {name}, Company: {company}"

let firstNumber = 2000
let secondNumber = 21
printfn $"The year is: {firstNumber + secondNumber}"

let name = "Chris"
printf "Hi %s" name
// prints: Hi Chris