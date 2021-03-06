module Ast

type label = string
type identifier = string
type index = int
type Hashtable<'k, 'v> = System.Collections.Generic.Dictionary<'k, 'v>

/// Small Basic arithmetic operation
type arithmetic =
    | Add
    | Subtract
    | Multiply
    | Divide

/// Small Basic comparison operaton
type comparison =
    | Eq
    | Ne
    | Lt
    | Gt
    | Le
    | Ge

/// Small Basic logical operation
type logical =
    | And
    | Or

/// Small Basic value
type value =
    | Bool of bool
    | Int of int
    | Double of double
    | String of string
    | Array of Hashtable<value, value>

/// Small Basic expression
type expr =
    | Literal of value
    | Var of identifier
    | GetAt of location
    | Func of invoke
    | Neg of expr
    | Arithmetic of expr * arithmetic * expr
    | Comparison of expr * comparison * expr
    | Logical of expr * logical * expr

and location = Location of identifier * expr list

and invoke =
    | Method of string * string * expr []
    | PropertyGet of string * string

type assign = Set of identifier * expr

/// Small Basic instruction
type instruction =
    | Assign of assign
    | SetAt of location * expr
    | PropertySet of string * string * expr
    | Action of invoke
    | For of assign * expr * expr
    | EndFor
    | If of expr
    | ElseIf of expr
    | Else
    | EndIf
    | While of expr
    | EndWhile
    | Sub of identifier
    | EndSub
    | GoSub of identifier
    | Label of label
    | Goto of label
