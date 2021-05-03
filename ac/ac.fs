module ac

open AST
open Parser
open Scanner
open Production
open Analysis

let identifiers = TokenType.From("Identifier", 20, ["[a-eghj-oq-z]"])
let declaration = TokenType.Literal("Declaration", 20, ["i"; "f"])

let operators: List<Operator> = [
    BinaryOperator("*", 30)
    BinaryOperator("/", 30)
    BinaryOperator("+", 20)
    MixedOperator("-", 20, 40)
]

let parser = ParserFactory.For(operators, identifiers0=identifiers)

let Expression = ExpresionProduction operators
let Identifier = SimpleProduction identifiers
let Assignment = StatementProduction("Assignment", [Identifier; ConstProduction "="; Expression])
let Declaration = StatementProduction("Declaration", [SimpleProduction declaration; Identifier])
let Print = StatementProduction("Print", [ConstProduction "p"; Identifier])
let Program = RepeatedProduction("Program", OneOfProduction([Declaration; Assignment; Print]))

let isEOF (next: Option<Token>) =
    next.IsSome && next.Value.Type.Name = BaseTokenTypes.EOF.Name

let DeclPass: Transformer<TreeData> =
    let mutable scope = [SymbolTable.Empty]
    let pushScope (tree: Tree<TreeData>) =
        // When entering a block, update the current scope
        match tree with
        | Node(d, _) when d.Data.ContainsKey "scope" ->
             scope <- d.Data.["scope"] :?> Scope :: scope
        | _ -> ()
        tree
    let popScope (tree: Tree<TreeData>) =
        // When exiting a block, update the node with any updates
        match tree with
        | Node(d, c) when d.Data.ContainsKey "scope" ->
            let scope1 = scope.Head
            scope <- scope.Tail
            Tree.Node({d with Data = d.Data.Add("scope", scope1 :> obj)}, c)
        | _ ->  tree
    let declare (tree: Tree<TreeData>) =
        match tree with
        | Node(d, c) ->
            match c.Head with
            | Node(v, _) ->
                let declare = scope.Head.Declare(v.Token.Value, d.Token.Position, Some(Map.empty.Add("type", d.Token.Value :> obj)))
                match declare with
                | Ok(s) ->
                    scope <- s :: scope.Tail
                    Tree.Node({d with Data = d.Data.Add("variable", c.Head)}, [])
                | Error(why) -> 
                    tree |> TreeErrors.Add $"Failed to declare in scope: {why}"
            | Empty -> tree
        | Empty -> tree
    let checkDeclaration (tree: Tree<TreeData>) =
        match tree with
        | Node(d, []) when d.Token.Type.Name = "Identifier" -> 
            // Check that d.Token.Value has been declared
            match scope.Head.Lookup d.Token.Value with
            | Some symbol ->
                let symbolType = symbol.Data.["type"] :?> string
                Tree.Leaf({d with Data = d.Data.Add("type", symbolType)})
            | None ->
                let tree = Tree.Leaf({d with Data = d.Data.Add("type", "i")})
                tree |> TreeErrors.Add $"Undeclared variable: {d.Token.Value}"
        | Node(d, c) when d.Token.Type.Name = "Operator" ->
            if d.Token.Value = "i" || d.Token.Value = "f"
            then
                Tree.Node({d with Data = d.Data.Add("type", d.Token.Value)}, c)
            else
                let allint =
                    c
                    |> List.map(fun (Node(d, _)) -> d.Data.["type"] :?> string)
                    |> List.forall(fun t -> t = "i")
                if allint
                then Tree.Node({d with Data = d.Data.Add("type", "i")}, c)
                else Tree.Node({d with Data = d.Data.Add("type", "f")}, c)
        | Node(d, c) when d.Token.Type.Name = "Value" ->
            if d.Token.Value.Contains('.')
            then Tree.Node({d with Data = d.Data.Add("type", "f")}, c)
            else Tree.Node({d with Data = d.Data.Add("type", "i")}, c)
        | _ -> tree

    let checkScope (tree: Tree<TreeData>) =
        let (Node(d, _)) = tree
        match d.Token.Value with
        | "f" -> declare tree
        | "i" -> declare tree
        | _ -> checkDeclaration tree
    Transformer(checkScope, pushScope, popScope)

type Value =
    abstract Add: Value -> Value
    abstract Sub: Value -> Value

type IValue (value: int) =
    member _.Value: int = value
    override  t.ToString () = $"{t.Value}"
    static member From (value: string) = IValue(value |> int)
    interface Value with
        member t.Add (value: Value) =
            if value :? FValue
            then FValue((t.Value |> float) + (value :?> FValue).Value) :> Value
            else IValue(t.Value + (value :?> IValue).Value) :> Value

        member t.Sub (value: Value) =
            if value :? FValue
            then FValue((t.Value |> float) - (value :?> FValue).Value) :> Value
            else IValue(t.Value - (value :?> IValue).Value) :> Value

and FValue (value: float) =
    member _.Value: float = value
    override  t.ToString () = $"{t.Value:F5}"
    static member From (value: string) = FValue(value |> float)
    static member NaN = FValue nan
    interface Value with
        member t.Add (value: Value) =
            let value = 
                if value :? FValue
                then (value :?> FValue).Value
                else (value :?> IValue).Value |> float
            FValue(t.Value + value) :> Value

        member t.Sub (value: Value) =
            let value = 
                if value :? FValue
                then (value :?> FValue).Value
                else (value :?> IValue).Value |> float
            FValue(t.Value - value) :> Value


let getValue (tree: TreeData): Value =
    if tree.Data.ContainsKey "value"
    then tree.Data.["value"]
    else FValue.NaN :> obj
    :?> Value

let setValue (tree: Tree<TreeData>) (value: Value): Tree<TreeData> =
    match tree with
    | Node(d, c) -> Tree.Node({d with Data = d.Data.Add("value", value)}, c)
    | _ -> tree

let InterpretPass: Transformer<TreeData> =
    let mutable scope = SymbolTable.Empty
    let mutable scopes = [scope]
    let pushScope (tree: Tree<TreeData>) =
        // When entering a block, update the current scope
        match tree with
        | Node(d, _) when d.Data.ContainsKey "scope" ->
            scope <- d.Data.["scope"] :?> Scope
            scopes <- scope :: scopes
        | _ -> ()
        tree

    let popScope (tree: Tree<TreeData>) =
        // When exiting a block, update the node with any updates
        match tree with
        | Node(d, c) when d.Data.ContainsKey "scope" ->
            let scope1 = scope
            scopes <- scopes.Tail
            scope <- scopes.Head
            Tree.Node({d with Data = d.Data.Add("scope", scope :> obj)}, c)
        | _ -> tree

    let interpret (tree: Tree<TreeData>) =
        match tree with
        | Empty -> tree
        | Node(d, c) ->
            match d.Token.Type.Name with 
            | "Value" ->
                if d.Token.Value.Contains "."
                then FValue.From(d.Token.Value) :> Value
                else IValue.From(d.Token.Value) :> Value
                |> setValue tree 
            | "Operator" ->
                match d.Token.Value with
                | "=" ->
                    match Tree.BinOp tree with
                    | Some (lhs, rhs) ->
                        let id = lhs.Token.Value
                        let value = 
                            if rhs.Data.ContainsKey "value"
                            then rhs.Data.["value"]
                            else FValue.NaN :> obj
                        match scope.Lookup id with
                        | Some symbol ->
                            scope <- scope.Set(id, {symbol with Data = symbol.Data.Add("value", value)})
                            scopes <- scope :: scopes.Tail
                            tree
                        | None ->
                            let message = $"Assign to uninitialized variable {id}"
                            tree |> TreeErrors.Add message
                    | None ->
                        tree |> TreeErrors.Add $"ASSIGN missing sides"
                | "+" ->
                    match Tree.BinOp tree with
                    | Some (lhs, rhs) ->
                        getValue(lhs).Add(getValue(rhs)) |> setValue tree
                    | None -> tree |> TreeErrors.Add $"ADD missing sides"
                | "-" ->
                    match Tree.BinOp tree with
                    | Some (lhs, rhs) ->
                        getValue(lhs).Sub(getValue(rhs)) |> setValue tree
                    | None -> tree |> TreeErrors.Add $"SUB missing sides"
                | "p" ->
                    match tree with
                    | Node(id, _) ->
                        let id = id.Token.Value
                        match scope.Lookup id with
                        | Some symbol ->
                            if symbol.Data.ContainsKey "value"
                            then
                                let value = symbol.Data.["value"]
                                Tree.Node({d with Data = d.Data.Add("out", $"p {id} = {value}")}, c)
                            else
                                tree |> TreeErrors.Add $"PRINT uninitialized variable {id}"
                        | None -> tree |> TreeErrors.Add $"PRINT undeclared variable {id}"
                    | Empty -> tree
                | _ -> tree
            | "Identifier" ->
                let id = d.Token.Value
                match scope.Lookup id with
                | Some symbol ->
                    if symbol.Data.ContainsKey "value"
                    then
                        let value = symbol.Data.["value"]
                        Tree.Node({d with Data = d.Data.Add("value", value)}, c)
                    else
                        tree |> TreeErrors.Add $"READ uninitialized variable {id}"
                | None -> 
                    // TreeErrors.Add tree $"READ undeclared variable {id}"
                    // Already checked in DeclPass
                    tree
            | _ -> tree

    Transformer(interpret, pushScope, popScope)

