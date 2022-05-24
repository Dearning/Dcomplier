module Main

open FSharp.Text.Lexing

let show e = printfn "%A" e


[<EntryPoint>]
let main argv =
    let lexbuf = LexBuffer<char>.FromString argv[0]

    Exprparser.main Exprlexer.token lexbuf |> show
    0
