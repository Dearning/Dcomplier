// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
[<EntryPoint>]
let main argv =
    let aNumber = 0
    printfn "Here's a number %i" aNumber

    Console.Write "Type a value:"
    let str = Console.ReadLine()
    printfn "You typed %s" str
    
    0 // return an integer exit code