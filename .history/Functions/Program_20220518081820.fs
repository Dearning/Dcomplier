// For more information see https://aka.ms/fsharp-console-apps
let no = card % 13
if no = 1 then "Ace"
elif no = 0 then "King"
elif no = 12 then "Queen"
elif no = 11 then "Jack"
else string no
