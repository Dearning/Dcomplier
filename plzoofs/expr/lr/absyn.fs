module Absyn

type expr =
    | Term_as_Expr of term
    | Plus_Expr of (expr * expr)
    | Minus_Expr of (expr * expr)

and term =
    | Factor_as_Term of factor
    | Mult_Term of (term * term)
    | Div_Term of (term * term)

and factor =
    | Id_as_Factor of string
    | Parenthesized_Expr_as_Factor of expr
