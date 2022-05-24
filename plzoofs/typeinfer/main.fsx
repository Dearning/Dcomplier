#load "syntax.fsx" "type_infer.fsx"

open Syntax
open TypeInfer


let e1 = Int 3
let e2 = Var "x"
let e3 = Fun("y", Plus(e1, Var "x"))
let e4 = Fun("x", Var "x") // I
let e5 = Fun("x", Fun("y", Var "x")) // K

let e6 =
    Fun("x", Fun("y", Fun("z", Apply(Apply(Var "x", Var "z"), Apply(Var "y", Var "z"))))) // S


let exps = [ e1; e2; e3; e4; e5 ]
// let exps = [e7]


let show = printfn "%A"

let showtype  =  string_of_type >> show




"e3: f y = 3 + x" |> show
printfn "%A" e3 

type_of [("x",TInt)] e3  |> showtype

"e6: S x y z =  x z (y z)" |> show
printfn "%A"  e6

type_of [("x",TInt)] e6  |> showtype

List.map (type_of [ ("x", TInt) ] >> string_of_type) exps
|> printfn "%A"

// type error 
try
    type_of [] e3  |> showtype
with
    | :? Type_error as e -> printfn "%A" e

try
    let e7 = Plus(e4, e1)

    type_of [] e7  |> showtype
with
    | :? Type_error as e -> printfn "%A" e

// List.map (type_of []) exps |> printfn "%A"
// List.map typ exps |> printfn "%A"
