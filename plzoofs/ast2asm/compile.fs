(* source language ---------------------------------------------------------- *)

module Compile

type var = string

(* An object language: a simple datatype of 64-bit integer expressions
   Legal variables are X1 .. X8.
*)
type exp =
    | Var of var (* string representing an object-language variable *)
    | Const of int64 (* a constant int64 value *)
    | Add of exp * exp (* sum of two expressions *)
    | Mul of exp * exp (* product of two expressions *)
    | Neg of exp (* negation of an expression *)

(* Static context: contains the set of legally scoped variables *)
let context =
    [ "X1"
      "X2"
      "X3"
      "X4"
      "X5"
      "X6"
      "X7"
      "X8" ]

(* Checks that the source program is well formed *)
let rec static_check (e: exp) : unit =
    (match e with
     | Var x ->
         if List.contains x context then
             ()
         else
             failwith (Printf.sprintf "Variable not in scope: %s" x)
     | Const _ -> ()
     | Add (e1, e2)
     | Mul (e1, e2) ->
         static_check e1
         static_check e2
     | Neg (e) -> static_check e)



(* back-end translation to x86 ---------------------------------------------- *)
open X86
open X86.Asm

(* Demonstrates x86_64 System V AMD64 ABI calling conventions:
   - first six integer/pointer args are passed in:
     rdi, rsi, rdx, rcx, r8, r9
   - arguments seven and higher are pushed onto the stack (right-to-left)

   return value in rax

   - callee save: rbp, rbx, r12-r15
   - caller save: all others
*)

let function_prologue : X86.ins list =
    [ Pushq, [ !@Rbp ] // 过程调用头部代码， 保存栈帧基指针
      Movq, [ !@Rsp; !@Rbp ] ]

let function_epilogue : X86.ins list = [ Popq, [ !@Rbp ]; Retq, [] ] //恢复原来的栈帧基指针

(* compiling the context ---------------------------------------------------- *)

(* In this simple language, each free identifier of the expression maps
   directly to an X86 operand.
*)


(* Assumes that x is a well-scoped variable *)
let compile_var (x: var) : X86.operand =
    //  printfn "windows:%A\n" Platform.isWindows
    match Platform.isWindows with
    | true ->
        match x with // MS x64 call convention
        | "X1" -> !@Rcx //寄存器传递参数 X1-X4
        | "X2" -> !@Rdx
        | "X3" -> !@R08
        | "X4" -> !@R09
        | "X5" -> Ind3(Lit 48L, Rbp) //栈传递参数 X5-X8
        | "X6" -> Ind3(Lit 56L, Rbp)
        | "X7" -> Ind3(Lit 64L, Rbp)
        | "X8" -> Ind3(Lit 72L, Rbp)
        | _ -> failwith (Printf.sprintf "unbound variable %s" x)
    | false ->
        match x with // AMD64 call convention
        | "X1" -> !@Rdi //寄存器传递参数 X1-X6
        | "X2" -> !@Rsi
        | "X3" -> !@Rdx
        | "X4" -> !@Rcx
        | "X5" -> !@R08
        | "X6" -> !@R09
        | "X7" -> Ind3(Lit 16L, Rbp) // 栈传递参数 X7,X8
        | "X8" -> Ind3(Lit 24L, Rbp)
        | _ -> failwith (Printf.sprintf "unbound variable %s" x)



(*
    编译策略，用 rax 寄存器保存表达式的计算结果
    compilation strategy 1: use the Rax invariant ------------------------ *)

(* Invariant:
    - compile the expression directly into Rax
    - use the stack to remember intermediate results for binary operations
    - use the dedicated register R10 for binary operations.
*)

let rec compile_exp (e: exp) : X86.ins list =
    let compile_op op e1 e2 =
        (compile_exp e1)
        @ [ Pushq, [ !@Rax ] ] // 将 e1 的结果入栈，因为编译 e2 的时候需要 访问 rax
          @ ((compile_exp e2))
            @ [ Popq, [ !@R10 ] // 从栈上获得 e1 的结果
                op, [ !@R10; !@Rax ] ] // 表达式的最后结果 放到 rax 寄存器

    (match e with
     | Var x -> [ Movq, [ compile_var x; !@Rax ] ]
     | Const c -> [ Movq, [ Imm(Lit c); !@Rax ] ]
     | Add (e1, e2) -> compile_op Addq e1 e2
     | Mul (e1, e2) -> compile_op Imulq e1 e2
     | Neg e -> compile_exp e @ [ Imulq, [ !&(-1); !@Rax ] ])




let compile1 (src: exp) : X86.prog =
    [ gtext
          "program"
          (function_prologue
           @ (compile_exp src) @ function_epilogue) ]





(* compilation strategy 2: use the stack-based ir --------------------------- *)

(* intermediate representation ---------------------------------------------- *)

(* A stack-oriented intermediate representation *)
type insn =
    | IPushC of int64 (* push an int32 constant onto the stack *)
    | IPushV of var (* push value of var onto the stack *)
    | IMul (* multiply the top two values on the stack *)
    | IAdd (* add the top two values on the stack *)
    | INeg (* negate the top value on the stack *)

type program = insn list

let rec flatten (e: exp) : program =
    (match e with
     | Var x -> [ IPushV x ]
     | Const c -> [ IPushC c ]
     | Add (e1, e2) -> (flatten e1) @ (flatten e2) @ [ IAdd ]
     | Mul (e1, e2) -> (flatten e1) @ (flatten e2) @ [ IMul ]
     | Neg (e) -> (flatten e) @ [ INeg ])


(* use the x86 stack *)

let compile_op (op: opcode) : X86.ins list =
    [ Popq, [ !@Rax ] //  栈上取值 rax
      Popq, [ !@R10 ] //  栈上取值 r10
      op, [ !@R10; !@Rax ] //  运算  结果保存在 rax
      Pushq, [ !@Rax ] ] //  rax 结果上栈

let compile_neg : X86.ins list =
    [ Popq, [ !@Rax ]
      Imulq, [ !&(-1); !@Rax ]
      Pushq, [ !@Rax ] ]

let rec compile_insn (i: insn) : X86.ins list = // 栈式虚拟机指令，直接用x86的栈
    (match i with
     | IPushC c -> [ Pushq, [ Imm(Lit c) ] ]
     | IPushV x -> [ Pushq, [ compile_var x ] ]
     | IMul -> compile_op Imulq
     | IAdd -> compile_op Addq
     | INeg -> compile_neg)



let compile2 (src: exp) : X86.prog =
    let ir : program =
        static_check src
        flatten src

    [ gtext
          "program"
          (function_prologue
           @ (List.concat (List.map compile_insn ir))
             @ [ Popq, [ !@Rax ] ] // 从栈上取回 运算值，放到 rax 作为结果
               @ function_epilogue) ] // 从栈上取回 运算值，放到 rax 作为结果
