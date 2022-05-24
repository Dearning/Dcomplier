(*****************************
 * Main read-eval-print loop *
 *****************************)
module Repl

open System
open FSharp.Text.Lexing

open Ast

let parse (s: string) : Ast.expr =
    let lexbuf = LexBuffer<char>.FromString s
    Parser.main Lexer.token lexbuf

let i = "fun x -> x"
let s = "fun x -> fun y -> fun z -> x z (y z)"
let k = "fun x -> fun y -> x"
let o = "fun f -> fun g -> fun x -> f (g x)"
let y = "fun f -> (fun x -> f x x) (fun y -> f y y)"

let rec repl () =
    printf "%s" "? "

    let input = Console.ReadLine() in

    if input = "" then
        ()
    else
        try
            let e = parse input in
            let t = Infer.infer e in
            Printf.printf "%s : %s\n" (Ast.to_string e) (Ast.type_to_string t)
            repl ()
        with
        | Failure msg ->
            printfn "%s" msg
            repl ()
        | _ ->
            printfn "%s" "Error"
            repl ()

[<EntryPoint>]
let main argv =
    let _ = repl ()
    0
