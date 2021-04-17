namespace Analysis

open AST
open Scanner
open Parser

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
    let ScanErrors: Transformer<TreeData> =
        let scanErrors (tree: Tree<TreeData>): Tree<TreeData> =
            let (Node(d, c)) = tree
            if d.Token.Type.Name = "Unknown"
            then AST.TreeErrors.Add tree $"Unknown token type: {d.Token.Value}"
            else tree
        Transformer scanErrors
    let ScopePass: Transformer<TreeData> =
        let mutable scopes = [SymbolTable.Empty]
        let pushScope (tree: Tree<TreeData>) =
            let (Node(d, c)) = tree
            if d.Token.Value = "{" || d.Token.Type.Name = "ROOT"
            then
                let scope = scopes.Head.New ()
                scopes <- scope :: scopes
                Tree.Node({d with Data = d.Data.Add("scope", scope)}, c)
            else tree
        let popScope (tree: Tree<TreeData>) =
            let (Node(d, _)) = tree
            if d.Token.Value = "{" || d.Token.Type.Name = "ROOT"
            then scopes <- scopes.Tail
            tree
        Transformer(id, inAction0=pushScope, outAction0=popScope)

