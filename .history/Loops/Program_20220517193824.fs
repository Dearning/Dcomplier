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

let suit (no:int) : string = 
    let suitNo:int = no / 13
    if suitNo = 0 then "Hearts"
    elif suitNo = 1 then "Spades"
    elif suitNo = 2 then "Diamonds"
    else "Clubs"

let cards = [ 1; 10; 2; 34 ]


[<EntryPoint>]
let main argv =
    let cards = [ 1; 10; 2; 34 ]
    
    // add for loop
    printfn "%s of %s" (cardDescription(card)) (suit(card))
    // implement the rest
    0 // return an integer exit code