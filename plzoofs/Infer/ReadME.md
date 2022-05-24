## Type inference



```fsharp
dotnet restore
dotnet build
dotnet run

? fun x -> x                  # I
fun x -> x : 'a -> 'a
? fun x -> fun y -> x         # K
fun x -> fun y -> x : 'a -> 'b -> 'a
? fun x -> fun y -> y         # KI
fun x -> fun y -> y : 'a -> 'b -> 'b
? fun f -> fun g -> fun x -> f (g x)   # B
fun f -> fun g -> fun x -> f (g x) : ('e -> 'd) -> ('c -> 'e) -> 'c -> 'd
? fun x -> fun y -> fun z -> x z (y z)      # S
fun x -> fun y -> fun z -> x z (y z) : ('c -> 'e -> 'd) -> ('c -> 'e) -> 'c -> 'd
? fun f -> (fun x -> f x x) (fun y -> f y y)       # M M / Ω
not unifiable: circularity
? 

```