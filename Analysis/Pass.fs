namespace Analysis

open AST

type PassManager (?passes0: List<Transformer<TreeData>>) =
    let passes = defaultArg passes0 []
    static member Empty = PassManager ()
    member _.AddPass pass =
        PassManager(passes @ [pass])
    member _.Run tree =
        let mutable tree = tree
        for pass in passes do
            tree <- pass.Transform tree
        tree

module Passes =
    let openScope (d: TreeData) =
        d.Token.Value = "{" || d.Token.Type.Name = "ROOT"
    let ScanErrors: Transformer<TreeData> =
        let scanErrors (tree: Tree<TreeData>): Tree<TreeData> =
            match tree with
            | Node(d, c) when d.Token.Type.Name = "Unknown" ->
                tree |> AST.TreeErrors.Add $"Unknown token type: {d.Token.Value}"
            | _ -> tree
        Transformer scanErrors
    let ScopePass: Transformer<TreeData> =
        let mutable scopes = [SymbolTable.Empty]
        let pushScope (tree: Tree<TreeData>) =
            match tree with
            | Node(d, c) when openScope d ->
                let scope = scopes.Head.New ()
                scopes <- scope :: scopes
                Tree.Node({d with Data = d.Data.Add("scope", scope)}, c)
            | _ -> tree
        let popScope (tree: Tree<TreeData>) =
            scopes <- match tree with
                      | Node(d, _) when openScope d-> scopes.Tail
                      | _ -> scopes
            tree
        Transformer(id, inAction0=pushScope, outAction0=popScope)

