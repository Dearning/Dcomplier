module Syntax 
(** Abstract syntax *)

(** The type of variable names. *)
type name = string

(** Types in Poly. *)
type htype =
    | TInt (** integers [int] *)
    | TBool (** booleans [bool] *)
    | TParam of int (** parameter *)
    | TArrow of htype * htype (** Function type [s -> t] *)

(** Poly expressions. *)

type expr =
    | Var of name (* variable *)
    | Int of int (* integer constant *)
    | Bool of bool (* boolean value *)
    | Times of expr * expr (* product [e1 * e2] *)
    | Plus of expr * expr (* sum [e1 + e2] *)
    | Equal of expr * expr (* integer equality [e1 = e2] *)
    | Less of expr * expr (* integer comparison [e1 < e2] *)
    | If of expr * expr * expr (* conditional [if e1 then e2 else e3] *)
    | Fun of name * expr (* function [fun x -> e] *)
    | Apply of expr * expr (* application [e1 e2] *)
