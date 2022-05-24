## 构建、运行

expr LR 分析

```sh
dotnet clean
dotnet build
dotnet run "(a + b) * c"
# or

dotnet fsi main.fsx


```
## 修改 exprparser.fsy

```sh
%left Plus_token Minus_token
%left Times_token Divide_token
```

- 调整结合性指示 %left ==> %right
  - 查看 运行的结果有何不同
- 添加 一元减法 运算符
  - 添加 %token UMINUS
  - 修改语法 factor 规则
  - 添加优先级指示 %prec UMINUS

参考 calcvar 项目