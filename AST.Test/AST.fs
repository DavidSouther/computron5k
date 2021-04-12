module AST.Test

open FsUnit
open NUnit.Framework
open AST


[<TestFixture>]
type ASTTest () =
    [<Test>]
    member _.SExpression () =
        let node =
            Tree.Node ("+", [
                Tree.Leaf("a")
                Tree.Leaf("b")
            ])
        node
        |> Tree.ToSExpression
        |> should equal "(+ a b)"

    [<Test>]
    member _.Visit () =
        let ReplaceVisitor (map: Map<string, string>) =
            let replace (tree: Tree<string>) =
                match tree with
                | Node (t, c) ->
                    if map.ContainsKey(t)
                    then Node (map.[t], c)
                    else tree

            Transformer replace
        let replaceA = ReplaceVisitor(Map.empty.Add("a", "x"))
        let tree =
            Tree.Node ("+", [
                Tree.Leaf("a")
                Tree.Leaf("b")
            ])
        replaceA.Transform(tree)
        |> Tree.ToSExpression
        |> should equal "(+ x b)"
