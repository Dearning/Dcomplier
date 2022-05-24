open System
// using System;
let cards = [ 0 .. 5 ]
// let cards = [21; 3; 1; 7; 9; 23]
let hand = []
let cardFace card = 
    let no = card % 13
    if no = 1 then "Ace"
    elif no = 0 then "King"
    elif no = 12 then "Queen"
    elif no = 11 then "Jack"
    else string no

let suit card =
    let no = card / 13
    if no = 0 then "Hearts"
    elif no = 1 then "Spades"
    elif no = 2 then "Diamonds"
    else "Clubs"

let shuffle list =
    let random = System.Random()
    list |> List.sortBy (fun x -> random.Next())

let printCard card = printfn "%s of %s" (cardFace card) (suit card)

let printAll list = List.iter(fun x -> printCard(x)) list

let take (no:int) (list) = List.take no list

// cards |> shuffle |> take 3 |> printAll


printfn "cards%A" cards
printfn "hand%A" hand

let drawCard (tuple: int list * int list) = 
    let deck = fst tuple
    let draw = snd tuple
    let firstCard = deck.Head
    printfn "%i" firstCard

    let hand = 
        draw
        |> List.append [firstCard]
       
    printfn "bdeck%A" deck
    printfn "draw%A" draw 
    printfn "draw%A" draw
    printfn "hand%A" hand
    (deck.Tail, hand)

let d, h = (cards, hand) |> drawCard |> drawCard

printfn "Deck: %A Hand: %A" d h // Deck: [2; 3; 4; 5] Hand: [1; 0]