(******************************************
 * Type inference for simple lambda terms *
 ******************************************)
module Infer

open System.Collections.Generic
open Ast

let code = ref 97 // 'a'

let reset_type_vars () = code.contents <- int ('a')

let next_type_var () : typ =
    let c = code.contents in

    if c > int ('z') then
        failwith "too many type variables"

    code.contents <- code.contents + 1
    TVar(string (char c))

let type_of (ae: aexpr) : typ =
    match ae with
    | AVar (_, a) -> a
    | AFun (_, _, a) -> a
    | AApp (_, _, a) -> a

(* annotate all subexpressions with types *)
(* bv = stack of bound variables for which current expression is in scope *)
(* fv = hashtable of known free variables *)

let assoc x ls = snd (List.find (fst >> ((=) x)) ls)

let annotate (e: expr) : aexpr =
    let h = Map.empty<id, typ> in

    let rec annotate' (e: expr) (bv: (id * typ) list) : aexpr =
        match e with
        | Var x ->
            (* bound variable? *)
            try
                let a = assoc x bv in AVar(x, a)
            (* known free variable? *)
            with
            | :? KeyNotFoundException ->
                try
                    let a = Map.find x h in AVar(x, a)
                (* unknown free variable *)
                with
                | :? KeyNotFoundException ->
                    let a = next_type_var () in
                    Map.add x a h |> ignore
                    AVar(x, a)

        | Fun (x, e) ->
            (* assign a new type to x *)
            let a = next_type_var () in
            let ae = annotate' e ((x, a) :: bv) in
            AFun(x, ae, Arrow(a, type_of ae))
        | App (e1, e2) -> AApp(annotate' e1 bv, annotate' e2 bv, next_type_var ()) in

    annotate' e []

(* collect constraints for unification *)
let rec collect (aexprs: aexpr list) (u: (typ * typ) list) : (typ * typ) list =
    match aexprs with
    | [] -> u
    | AVar (_, _) :: r -> collect r u
    | AFun (_, ae, _) :: r -> collect (ae :: r) u
    | AApp (ae1, ae2, a) :: r ->
        let (f, b) = (type_of ae1, type_of ae2) in collect (ae1 :: ae2 :: r) ((f, Arrow(b, a)) :: u)

(* collect the constraints and perform unification *)
let infer (e: expr) : typ =
    reset_type_vars ()
    let ae = annotate e in
    let cl = collect [ ae ] [] in
    let s = Unify.unify cl in
    Unify.apply s (type_of ae)
