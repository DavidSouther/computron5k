module Jack.Test

open FsUnit
open NUnit.Framework

open Jack
open AST

[<TestFixture>]
type TestJack () =
    let parse input =
        Jack.parser.Parse(input, "test")

    [<Test>]
    member _.Parse () =
        let tree =
            parse "if a = 5 { let b = 10; return b + 1; }"
        tree
        |> should equal "(program:test (if (= a 5) (; (let (= b 10)) (return (+ b 1))))"
