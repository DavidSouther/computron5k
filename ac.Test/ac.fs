namespace ac.Test

open FsUnit
open NUnit.Framework

[<TestFixture>]
type ACTest () =
    let parse input =
        ac.parser.Parse(input, "test")
        |> AST.Tree.ToSExpression

    [<Test>]
    member _.Parsing () =
        parse "f x x=12.345 p x"
        |> should equal "(program:test (f x) (= x 12.345) (p x))"
