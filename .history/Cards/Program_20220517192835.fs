// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
let cardNo = 12

let cardDescription = 
    if cardNo = 1 || cardNo = 14 then "Ace"
    elif cardNo = 11 then "Jack"
    elif cardNo = 12 then "Queen"
    elif cardNo = 13 then "King"
    else string cardNo
printfn "%s" cardDescription

let mutable quit = false
let no = 11
while not quit do
    printf "Guess a number: "
    let guess = Console.ReadLine() 
    let guessNo = int guess
    if guessNo = no then
        quit <- true
        printfn "You guessed correctly %i is the secret number" no
    else
        printfn "%i is incorrect" guessNo