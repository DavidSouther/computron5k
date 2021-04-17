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
            tree
        | Error(why) -> 
            TreeErrors.Add tree $"Failed to declare in scope: {why}"
    let checkDeclaration (tree: Tree<TreeData>) =
        match tree with
        | Node(d, []) -> 
            // Check that d.Token.Value has been declared
            match scope.Head.Get d.Token.Value with
            | Some symbol ->
                let symbolType = symbol.Data.["type"] :?> string
                Tree.Leaf({d with Data = d.Data.Add("type", symbolType)})
            | None ->
                let tree = Tree.Leaf({d with Data = d.Data.Add("type", "i")})
                TreeErrors.Add tree $"Undeclared variable: {d.Token.Value}"
        | Node(d, c) ->
            if d.Token.Type.Name = "Operator"
            then
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
            else tree

    let checkScope (tree: Tree<TreeData>) =
        let (Node(d, _)) = tree
        match d.Token.Value with
        | "f" -> declare tree
        | "i" -> declare tree
        | _ -> checkDeclaration tree
    Transformer(checkScope, pushScope, popScope)

