module ac

open AST
open Parser
open Scanner
open Analysis

let operatorTokens = TokenType.Literal("Operator", 20, ["+"; "-"; "="; "i"; "f"; "p"])
let identifiers = TokenType.From("Identifier", 20, ["[a-eghj-oq-z]"])
let whitespace = BaseTokenTypes.Whitespace

let scanner = ScannerFactory [operatorTokens; identifiers; BaseTokenTypes.Value; BaseTokenTypes.Whitespace; BaseTokenTypes.EOF;]

let decl (parser: Parser) (token: Token) =
    let next = parser.next()
    match next.Type.Name with
    | "Identifier" -> TreeNode token [TreeLeaf(next)]
    | _ -> TreeNode token [TreeErrors.Add(TreeLeaf(next))($"Expected Identifier, got {next.ToRepl()}")]

let operators: List<Operator> = [
    RightOperator("f", -5, decl)
    RightOperator("i", -5, decl)
    RightOperator("p", -5, decl)
    BinaryOperator("*", 30)
    BinaryOperator("/", 30)
    BinaryOperator("+", 20)
    MixedOperator("-", 20, 40)
    RightBinaryOperator("=", 10)
]

let parser = ParserFactory.For(operators, identifiers0=identifiers)

let isEOF (next: Option<Token>) =
    next.IsSome && next.Value.Type.Name = BaseTokenTypes.EOF.Name


let DeclPass: Transformer<TreeData> =
    let mutable scope = [SymbolTable.Empty]
    let pushScope (tree: Tree<TreeData>) =
        // When entering a block, update the current scope
        let (Node(d, _)) = tree
        if d.Data.ContainsKey "scope"
        then scope <- d.Data.["scope"] :?> Scope :: scope
        tree
    let popScope (tree: Tree<TreeData>) =
        // When exiting a block, update the node with any updates
        let (Node(d, c)) = tree
        if d.Data.ContainsKey "scope"
        then
            let scope1 = scope.Head
            scope <- scope.Tail
            Tree.Node({d with Data = d.Data.Add("scope", scope1 :> obj)}, c)
        else tree
    let declare (tree: Tree<TreeData>) =
        let (Node(d, c)) = tree
        let (Node(v, _)) = c.Head
        let declare = scope.Head.Declare(v.Token.Value, d.Token.Position, Some(Map.empty.Add("type", d.Token.Value :> obj)))
        match declare with
        | Ok(s) ->
            scope <- s :: scope.Tail
            Tree.Node({d with Data = d.Data.Add("variable", c.Head)}, [])
        | Error(why) -> 
            TreeErrors.Add tree $"Failed to declare in scope: {why}"
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
                TreeErrors.Add tree $"Undeclared variable: {d.Token.Value}"
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
    let (Node(d, c)) = tree
    Tree.Node({d with Data = d.Data.Add("value", value)}, c)

let InterpretPass: Transformer<TreeData> =
    let mutable scope = SymbolTable.Empty
    let mutable scopes = [scope]
    let pushScope (tree: Tree<TreeData>) =
        // When entering a block, update the current scope
        let (Node(d, _)) = tree
        if d.Data.ContainsKey "scope"
        then
            scope <- d.Data.["scope"] :?> Scope
            scopes <- scope :: scopes
        tree

    let popScope (tree: Tree<TreeData>) =
        // When exiting a block, update the node with any updates
        let (Node(d, c)) = tree
        if d.Data.ContainsKey "scope"
        then
            let scope1 = scope
            scopes <- scopes.Tail
            scope <- scopes.Head
            Tree.Node({d with Data = d.Data.Add("scope", scope :> obj)}, c)
        else tree

    let interpret (tree: Tree<TreeData>) =
        let (Node(d, c)) = tree
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
                        TreeErrors.Add tree $"Assign to uninitialized variable {id}"
                | None ->
                    TreeErrors.Add tree $"ASSIGN missing sides"
            | "+" ->
                match Tree.BinOp tree with
                | Some (lhs, rhs) ->
                    getValue(lhs).Add(getValue(rhs)) |> setValue tree
                | None -> TreeErrors.Add tree $"ADD missing sides"
            | "-" ->
                match Tree.BinOp tree with
                | Some (lhs, rhs) ->
                    getValue(lhs).Sub(getValue(rhs)) |> setValue tree
                | None -> TreeErrors.Add tree $"SUB missing sides"
            | "p" ->
                let (Node(id, _)) = c.Head 
                let id = id.Token.Value
                match scope.Lookup id with
                | Some symbol ->
                    if symbol.Data.ContainsKey "value"
                    then
                        let value = symbol.Data.["value"]
                        Tree.Node({d with Data = d.Data.Add("out", $"p {id} = {value}")}, c)
                    else
                        TreeErrors.Add tree $"PRINT uninitialized variable {id}"
                | None -> TreeErrors.Add tree $"PRINT undeclared variable {id}"
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
                    TreeErrors.Add tree $"READ uninitialized variable {id}"
            | None -> 
                // TreeErrors.Add tree $"READ undeclared variable {id}"
                // Already checked in DeclPass
                tree
        | _ -> tree

    Transformer(interpret, pushScope, popScope)

