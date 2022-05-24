module TypeInfer
#load "syntax.fsx"

open System.Collections.Generic

open Syntax

exception Type_error of string

let assoc x ls = snd (List.find (fst >> ((=) x)) ls)

(* [ty_error msg] reports a type error by raising [Type_error msg]. *)
let type_error msg = raise (Type_error msg)


(* [rename t] renames parameters in type [t] so that they count from
    [0] up. This is useful for pretty printing. *)
let rename ty =
    let rec ren ((j, s) as c) =
        function
        | TInt -> TInt, c
        | TBool -> TBool, c
        | TParam k ->
            (try
                TParam(assoc k s), c
             with
             | :? KeyNotFoundException -> TParam j, (j + 1, (k, j) :: s))
        | TArrow (t1, t2) ->
            let u1, c' = ren c t1 in
            let u2, c'' = ren c' t2 in
            TArrow(u1, u2), c'' in fst (ren (0, []) ty)



(** [fresh ()] returns an unused type parameter. *)
let fresh =
    let k = ref 0 in

    fun () ->
        k.contents <- k.contents + 1
        TParam k.contents

(** [refresh t] replaces all parameters appearing in [t] with unused ones. *)
let refresh ty =
    let rec refresh s =
        function
        | TInt -> TInt, s
        | TBool -> TBool, s
        | TParam k ->
            (try
                assoc k s, s
             with
             | :? KeyNotFoundException -> let t = fresh () in t, (k, t) :: s)
        | TArrow (t1, t2) ->
            let u1, s' = refresh s t1 in
            let u2, s'' = refresh s' t2 in
            TArrow(u1, u2), s'' in fst (refresh [] ty)

(* [occurs k t] returns [true] if parameter [TParam k] appears in type [t]. *)
let rec occurs k =
    function
    | TInt -> false
    | TBool -> false
    | TParam j -> k = j
    | TArrow (t1, t2) -> occurs k t1 || occurs k t2

(* [tsubst [(k1,t1); ...; (kn,tn)] t] replaces in type [t] parameters
    [TParam ki] with types [ti]. *)
let rec tsubst s =
    function
    | (TInt
    | TBool) as t -> t
    | TParam k ->
        (try
            assoc k s
         with
         | :? KeyNotFoundException -> TParam k)
    | TArrow (t1, t2) -> TArrow(tsubst s t1, tsubst s t2)


(** [string_of_type] converts a Poly type to string. *)
let string_of_type ty =

    let rec to_str n ty =
        let (m, str) =
            match ty with
            | TInt -> (4, "int")
            | TBool -> (4, "bool")
            | TParam k ->
                (4,
                 (if k < 26 then // a -> z
                      "'" + string(char (97+k))
                  else
                      "'ty" + string k))
            | TArrow (ty1, ty2) -> (1, (to_str 1 ty1) + " -> " + (to_str 0 ty2)) in

        if m > n then str else "(" + str + ")" in

    to_str (-1) ty


let string_of_eq eq = 
    let string_of_pair p = 
        string_of_type (fst p) + ": " + string_of_type (snd p) 
    List.map string_of_pair  eq |> string

let string_of_sbst sbst = 
    let string_of_pair p = 
            // 0 --> a  1 ---> b
        "'" + string (char (97 + (fst p))) + ": " + string_of_type (snd p) 
    List.map string_of_pair  sbst |> string


(* [solve [(t1,u1); ...; (tn,un)] solves the system of equations
    [t1=u1], ..., [tn=un]. The solution is represented by a list of
    pairs [(k,t)], meaning that [TParam k] equals [t]. A type error is
    raised if there is no solution. The solution found is the most general
    one.
*)

let solve eq =
    let rec solve eq sbst =

        printfn $"(eq: {string_of_eq eq}, sbst: {string_of_sbst sbst})"
    
        (* The following warning directive tells OCaml not to worry about the
       pattern involving [TParam] and [when] guard below. *)
        match eq with
        | [] -> sbst

        | (t1, t2) :: eq when t1 = t2 -> solve eq sbst

        | ((TParam k, t) :: eq
        | (t, TParam k) :: eq) when (not (occurs k t)) ->
            let ts = tsubst [ (k, t) ] in

            solve
                (List.map (fun (ty1, ty2) -> (ts ty1, ts ty2)) eq)
                ((k, t)
                 :: (List.map (fun (n, u) -> (n, ts u)) sbst))

        | (TArrow (u1, v1), TArrow (u2, v2)) :: eq -> solve ((u1, u2) :: (v1, v2) :: eq) sbst
        | (t1, t2) :: _ ->
            type_error (
                "The types "
                + string_of_type t1
                + " and "
                + string_of_type t2
                + " are incompatible"
            )

     in solve eq []

(* [constraints_of gctx e] infers the type of expression [e] and a set
    of constraints, where [gctx] is global context of values that [e]
    may refer to. *)
let rec constraints_of gctx =
    let rec cnstr ctx =
        function
        | Var x ->
            (try
                assoc x ctx, []
             with
             | :? KeyNotFoundException ->
                 (try
                     (* we call [refresh] here to get let-polymorphism *)
                     refresh (assoc x gctx), []
                  with
                  | :? KeyNotFoundException -> type_error ("Unknown variable " + x)))

        | Int _ -> TInt, []

        | Bool _ -> TBool, []

        | Times (e1, e2)
        | Plus (e1, e2) ->
            let ty1, eq1 = cnstr ctx e1 in
            let ty2, eq2 = cnstr ctx e2 in
            TInt, (ty1, TInt) :: (ty2, TInt) :: eq1 @ eq2

        | Equal (e1, e2)
        | Less (e1, e2) ->
            let ty1, eq1 = cnstr ctx e1 in
            let ty2, eq2 = cnstr ctx e2 in
            TBool, (ty1, TInt) :: (ty2, TInt) :: eq1 @ eq2
        | If (e1, e2, e3) ->
            let ty1, eq1 = cnstr ctx e1 in
            let ty2, eq2 = cnstr ctx e2 in
            let ty3, eq3 = cnstr ctx e3 in
            ty2, (ty1, TBool) :: (ty2, ty3) :: eq1 @ eq2 @ eq3
        | Fun (x, e) ->
            let ty1 = fresh () in
            let ty2, eq = cnstr ((x, ty1) :: ctx) e in
            TArrow(ty1, ty2), eq
        | Apply (e1, e2) ->
            let ty1, eq1 = cnstr ctx e1 in
            let ty2, eq2 = cnstr ctx e2 in
            let ty = fresh () in
            ty, (ty1, TArrow(ty2, ty)) :: eq1 @ eq2 in cnstr []

(* [type_of ctx e] computes the principal type of expression [e] in
    context [ctx]. *)
let type_of ctx e =
    let ty, eq = constraints_of ctx e in 
    printfn $"ty: {string_of_type ty} eq: {string_of_eq eq}"
    tsubst (solve eq) ty
