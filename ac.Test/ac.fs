namespace ac.Test

open FsUnit
open NUnit.Framework

open Analysis

[<TestFixture>]
type ACTest () =
    let parse input =
        ac.parser.Parse(input, "test")

    [<Test>]
    member _.Parsing () =
        parse "f x x=12.345 p x"
        |> AST.Tree.ToSExpression
        |> should equal "(program:test (f x) (= x 12.345) (p x))"

        parse "x x x x"
        |> AST.Tree.ToSExpression
        |> should equal "(program:test x x x x)"

        parse "x - 5 = 6 + 10"
        |> AST.Tree.ToSExpression
        |> should equal "(program:test (= (- x 5) (+ 6 10)))"


    [<Test>]
    member _.Scope () =
        let ast = parse "i a f x"
        (*
        let analyzer = PassManager.Empty.AddPass ScopePass
        let analysis = analyzer.Run ast
        let table = analysis.scope :?> SymbolTable
        table.symbols
        *)
        ()

