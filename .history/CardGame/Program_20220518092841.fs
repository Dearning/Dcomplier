// For more information see https://aka.ms/fsharp-console-apps
// 0 = 11, 11, 12, 13 = 10, else the actual number
let cardValue card =
    let value = card % 13
    if value = 0 then 11
    elif value = 10 || value = 11 || value = 12 then 10
    else value
