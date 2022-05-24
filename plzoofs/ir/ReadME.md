## 说明

- ir1-ir5 **表达能力逐渐增加的** 中间表示IR
  - 表达式
  - 命令
  - 控制流
  - 函数调用
  - 全局声明
- run.fsx 执行脚本
- ir-by-hand.fs  F# 模拟中间语言

```sh
dotnet build
dotnet run
```

## Intermediate Representations

### IR1: Expressions 表达式

IR1 实现了 `A标准形` （A Norm Form）

- 将程序变换为 A标准形，也称为 `A转换` A-translation  

  - A标准形中，操作的所有参数必须是`平凡的(常量或变量)`，不能是其他嵌套的操作

  ```
  f(g(x),h(y))
  
  ===》 ANN
  
  let v0 = g(x) in
      let v1 = h(y) in
          f(v0,v1)
          
  ```

  

- 机器语言程序是基本指令的线性序列，每条指令最多只能执行一次算术运算，因此需要使用多条指令来表示复杂的表达式。

- 通过将基于表达式的程序重组为一系列原子操作，`A转换`缩小了基于表达式的程序和线性指令序列之间的差距。

- IR1将复杂表达式表示为 `Let 语句序列`，每个Let语句执行一个`原子操作`。



- simple arithmetic expressions, immutable global variables
  - 操作 `Add Mul`
  - 操作数 `Id Const Var`
    - 临时变量 `Id of uid`
    - 全局变量 `Var of string`
- 将嵌套表达式 转换为 `let` 指令`序列`
  - `insn = Let of uid * bop * opn * opn`
  - `prog = insn list`
- 建立**临时变量**保存嵌套表达式的中间结果
  - 临时变量按序编号 0 1 2 ...（对应到寄存器）
  - SSA 单赋值形式
- 重要思想
  - 高级语言的高层表示向机器语言的底层表示的转换
    - 嵌套的语法树 ---> 线性指令序列
    - 无限数目的变量 ---> 有限数目的寄存器
    - 变量名称  --->  内存地址  或 寄存器
    - 复杂的数据类型 --->  无类型 （整型、浮点型、位操作、字节操作）
    - 高级语言特性（闭包、垃圾回收、协程、安全检查、语言库...）---> 运行时支持
  - 如何通过**层次结构**，实现上述转换



```F#
type uid = int        (* Unique identifiers for temporaries. *)
type var = string      
type opn =            (* syntactic values / operands *)
    | Id of uid           // 临时变量
    | Const of int64
    | Var of var        // 全局变量
type bop =            (* binary operations *)
    | Add
    | Mul
(* instructions *)
(* note that there is no nesting of operations! *)
type insn = Let of uid * bop * opn * opn
type program = { insns: insn list; ret: opn }


(1 + X4) + (3 + (X1 * 5)

=== ANN ==>

let tmp0 = add 1L varX4 in
let tmp1 = mul varX1 5L in
let tmp2 = add 3L tmp1 in
let tmp3 = add tmp0 tmp2 in
ret tmp3
```

### IR2: Commands 命令

变量赋值，顺序语言

由于机器语言中，变量的存储机制有两种，分别是

- 寄存器，CPU 可直接访问，速度快，但是数量有限 16，32，。。。

- 栈、堆，在内存中，数量只受内存限制，但是访问速度慢

  IR2 引入了 `Load Store 结构` 区分这两种变量访问机制

  - Load 表示从内存变量加载数据到寄存器变量
  - Store 表示将寄存器的数据存储到内存
  - 所有的基本操作的操作数 如 ADD MUL 只针对寄存器变量

- global mutable variables
  - `type var = string`
- commands for update and sequencing
- load store 结构
  - `Load of uid * var` 从内存变量（内存地址）加载到临时变量（寄存器）
  - 运算的操作数只是操作临时变量
  - `Store of var * opn` 将临时变量保存到内存变量

```F#
    type uid = int             (* Unique identifiers for temporaries. *)
    type var = string
    type opn =
        | Id of uid
        | Const of int64
    type bop =
        | Add
        | Mul
    type insn =
        | Let of uid * bop * opn * opn
        | Load of uid * var          // NEW
        | Store of var * opn         // NEW
    type program = { insns: insn list }
    
    X1 := (1 + X4) + (3 + (X1 * 5) ) ;
    Skip ;
    X2 := X1 * X1 ;     
===>
let tmp0 = load varX4 in
let tmp1 = add 1L tmp0 in
let tmp2 = load varX1 in
let tmp3 = mul tmp2 5L in
let tmp4 = add 3L tmp3 in
let tmp5 = add tmp1 tmp4 in
let _ = store tmp5 varX1 in
let tmp6 = load varX1 in
let tmp7 = load varX1 in
let tmp8 = mul tmp6 tmp7 in
let _ = store tmp8 varX2 in
()
```

### IR3: Local control flow 局部控制流

IR1 与IR2的线性序列，无法表示`IF 语句`等高级的程序控制流

- 需要引入分支语句
- 同时为了让程序的结构更加清晰，方便后续生成机器指令，进一步进入了 `基本块`和`控制流图`的概念



- conditional commands & while loops
  - 比较指令 `ICmp of uid * cmpop * opn * opn`
- basic blocks
  - 基本块内部只包括代码序列 `insn list`
  - 基本块终结指令`terminator`是转移指令（条件转移 `Cbr`、无条件转移 `Br`、返回`Ret`）
- cfg 控制流图组成
  - 唯一的 `entry block`
  - `lbl * block list`
  - 一个控制流图，对应到高级语言的一个函数

```F#
type uid = string          
type var = string
type lbl = string                          // NEW
type opn =
    | Id of uid
    | Const of int64
type bop =
    | Add
    | Mul
type cmpop =                (* comparison operations*) // NEW
    | Eq
    | Lt
type insn =
    | Let of uid * bop * opn * opn
    | Load of uid * var
    | Store of var * opn
    | ICmp of uid * cmpop * opn * opn   // NEW
type terminator =                       // NEW
    | Ret
    | Br of lbl              (* unconditional branch *)
    | Cbr of opn * lbl * lbl (* conditional branch *)
type block =                 (* Basic blocks *)      // NEW
    { insns: insn list
      terminator: terminator }
(* Control Flow Graph: a pair of an entry block and a set labeled blocks *)
type cfg = block * (lbl * block) list    // NEW 唯一入口block
type program = cfg                       // NEW

X2 := X1 + X2;      
IFNZ X2 THEN        
X1 := X1 + 1    
ELSE
X2 := X1        
X2 := X2 * X1    
   ===>
   let program () =       
       let rec entry () =      
           let tmp0 = load varX1 in
           let tmp1 = load varX2 in
           let tmp2 = add tmp0 tmp1 in
           let _ = store tmp2 varX1 in
           let tmp3 = load varX2 in
           let guard7 = icmp eq tmp3 0L in
           cbr guard7 z_branch9 nz_branch8
       and nz_branch8 () =     
           let tmp4 = load varX1 in
           let tmp5 = add tmp4 1L in
           let _ = store tmp5 varX1 in
           br merge10
       and merge10 () =        
           let tmp11 = load varX2 in
           let tmp12 = load varX1 in
           let tmp13 = mul tmp11 tmp12 in
           let _ = store tmp13 varX2 in
           ret ()
       in entry ()
```

### IR4: Procedures (top-level functions)过程/函数

IR3 已经可以表示 常规计算过程，但是无法实现过程抽象，也就是函数，IR4 引入了 `Call指令`，为了处理函数调用的参数（局部变量），分配栈帧空间，引入了`Alloca 指令`

- 函数声明
  - 名称  `fn_name`
  - 参数列表 `param: uid list`
- local state 局部变量
  - `Alloca of uid` 在栈上分配局部变量
- call stack 调用堆栈
  - `Call of uid * fn_name * (opn list)`
  - `Call` 不是 terminator

```F#
type uid = string (* Unique identifiers for temporaries. *)
type var = string
type lbl = string
type fn_name = string
type opn =
    | Id of uid
    | Const of int64
type bop =
    | Add
    | Mul
type cmpop =
    | Eq
    | Lt
type insn =
    | Let of uid * bop * opn * opn
    | Load of uid * var
    | Store of var * opn
    | ICmp of uid * cmpop * opn * opn
    | Call of uid * fn_name * (opn list)  // NEW
    | Alloca of uid                       // NEW
type terminator =
    | Ret
    | Br of lbl                (* unconditional branch *)
    | Cbr of opn * lbl * lbl   (* conditional branch *)
type block =
    { insns: insn list
      terminator: terminator }
type cfg = block * (lbl * block) list
type fdecl =                               // NEW
    { name: fn_name
      param: uid list
      cfg: cfg }
type program = { fdecls: fdecl list }      // NEW
```

### IR5: ”almost” LLVM IR

完整的程序，多个顶层函数声明，多个顶层变量声明构成，IR5 引入

- 全局变量
- 全局函数

至此，所有机制已经具备，可以将抽象语法树 完整的翻译为中间语言，IR5的形式是 与LLVM的基本形式

```F#
type uid = string (* Unique identifiers for temporaries. *)
type lbl = string
type gid = string
type opn =
    | Id of uid
    | Const of int64
type bop =
    | Add
    | Mul
type cmpop =
    | Eq
    | Lt
type insn =
    | Binop of bop * opn * opn (* Rename let to binop *)
    | Load of gid
    | Store of gid * opn
    | ICmp of cmpop * opn * opn
    | Call of gid * (opn list)
    | Alloca
type terminator =
    | Ret
    | Br of lbl (* unconditional branch *)
    | Cbr of opn * lbl * lbl (* conditional branch *)
type block =
    { insns: (uid * insn) list
      terminator: terminator }
type cfg = block * (lbl * block) list
type gdecl = GInt of int64                 // NEW
type fdecl = { param: uid list; cfg: cfg } // NEW
type program =
    { gdecls: (gid * gdecl) list           // NEW
      fdecls: (gid * fdecl) list }         // NEW
```

请仔细思考上面各个IR的表示机制，理解中间语言的构造思路，重点理解中间语言与高级语言，机器语言的区别

- 具有高级语言的特性
  - 无限存储
  - 函数
- 具有机器语言的特性
  - 线性指令
  - 跳转标签
  - 区分，栈，寄存器
- 自己的独特特性
  - 静态单赋值
  - 基本块

## 参考资源

|      | Upenn CIS 341 - Compilers      |                                                              |
| ---- | ------------------------------ | ------------------------------------------------------------ |
|      | IRs I                  | [lec06.pdf](https://www.seas.upenn.edu/~cis341/current/lectures/lec06.pdf) |
|      | IRs II  | [lec07.pdf](https://www.seas.upenn.edu/~cis341/current/lectures/lec07.pdf) |