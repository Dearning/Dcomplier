module Parser

open System
open ScanRat.ScanRat
open Ski


let (-) (a: char) (b: char) =
    let chars = seq { a..b } |> String.Concat
    oneOf chars

let lc = production ("lc")
let var = 'a' - 'z'
let space = (~~ " ").oneOrMore.opt

let tap a =
    printfn "%A" a
    a

lc.rule <-
    lc .+ space + lc --> fun (l, r) -> LApp (l, r)
    |- space 
        + ~~ "\\" 
        + space 
        +. var
       .+ space
       .+ ~~ "."
       .+ space
       + lc
       --> fun (var, lc) ->
               //tap (var,lc)
               LAbs(string var, lc)

    |- var --> (string >> LVar)

let safeParseAst (input: string) =
    let inp = input.Trim()
    parse lc inp 

let parseAst (input: string) =
    let inp = input.Trim()
    let result = parse lc inp

    match result with
    | Success s -> s.Value
    | Failure _f ->

        let { Index = pos; Expectations = expected } = _f
        failwith $"parsing failed at: {pos} expected: {expected}"
