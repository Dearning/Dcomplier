let cardFace (card:int) :string = 
    let no = card % 13
    if no = 14 || no = 1 then "Ace"
    elif no = 13 then "King"
    elif no = 12 then "Queen"
    elif no = 11 then "Jack"
    else string no

printfn "%s" (cardFace 11)
let add2 a = a + 2
let multiply3 a = a * 3 
let addAndMultiply = add2 >> multiply3

printfn "%i" (addAndMultiply 2) // 12
