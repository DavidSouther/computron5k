namespace Analysis

open AST
open Scanner

type ScopeData = Token * Scope

type TreeData =
    | Token of Token
    | Scope of ScopeData

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

module Passes  =
    let ScopePass: Transformer<TreeData> =
        (*
        let mutable scopes = [SymbolTable.Empty]
        let mutable scopeMap: Map<TreeData, Scope> = Map.Empty
        let handleScope (tree: Tree<TreeData>) =
            match tree with
            | Node (t, c) ->
                match t with
                | Token t ->
                    if t.Value = "{"
                    then
                        let scope = scopes.Head.New ()
                        scopes <- scope :: scopes
                        Tree.Node
                | _ -> tree
        Transformer(handleScope, popScope)
        *)
        Transformer id
