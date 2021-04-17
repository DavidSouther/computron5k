﻿namespace ac.Test

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
        let analyzer = PassManager.Empty.AddPass(Passes.ScopePass).AddPass(ac.DeclPass)
        let (AST.Tree.Node(data, _)) = analyzer.Run ast
        let table = data.Data.["scope"] :?> SymbolTable
        let symbols =
            table.symbols
            |> Seq.toList
            |> List.map(fun t -> 
                let declType = t.Value.Data.["type"]
                $"{t.Key}:{declType}")
            |> List.sortBy id
            |> String.concat(" ")
        symbols |> should equal "a:i x:f"
