// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
open System

let cardDescription (card: int) : string =
    let cardNo: int = card % 13
    if cardNo = 1 then "Ace"
    elif cardNo = 11 then "Jack"
    elif cardNo = 12 then "Queen"
    elif cardNo = 0 then "King"
    else string cardNo
    
[<EntryPoint>]
let main argv =
    // implement the rest
    0 // return an integer exit code