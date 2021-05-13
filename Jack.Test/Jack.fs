module Jack.Test

open FsUnit
open NUnit.Framework

open Jack
open AST

[<TestFixture>]
type TestJack () =
    let parse input =
        // Jack.parser.Parse(input, "test")
        Empty

    [<Test>]
    member _.Parse () =
        parse "if (a = 5) { let b = 10; return b + 1 }"
        |> AST.Tree.ToSExpression
        |> should equal "(program:test (if (= a 5) (; (let b 10) (return (+ b 1)))))"

        parse "foo.bar.baz"
        |> AST.Tree.ToSExpression
        |> should equal "(program:test (. (. foo bar) baz))"

        parse "bar(1, 2, 3)"
        |> AST.Tree.ToSExpression
        |> should equal "(program:test (call bar (, 1 2 3)))"

        parse "bar.baz(1, 2, 3)"
        |> AST.Tree.ToSExpression
        |> should equal "(program:test (call (. bar baz) (, 1 2 3)))"

        parse "bar(1, (2 + 3))"
        |> AST.Tree.ToSExpression
        |> should equal "(program:test (call bar (, 1 (+ 2 3))))"

        parse "let a = 5"
        |> AST.Tree.ToSExpression
        |> should equal "(program:test (let a 5))"

        parse "let 5" // In Jack, an invalid statement.
        |> AST.Tree.ToSExpression
        |> should equal "(program:test (let ~Err:Expected id, got Value:5~))"

        parse "function foo(int y) y"
        // At this point, the parse technique fails. Operators aren't able to
        // understand the context of grouping (), calling (), and declaration list ().
        // So `function` will need to explicitly expect("("), expression, expect(")"),
        // which is really just recursive descent, so we have recursive descent with
        // more steps.
        // Maybe it could be n-ary `function` id `(` decl_list `)` expression
        // But we'd need to expliclty declare `id`, same as `let`, and `decl_list` is
        // more complicated than an expression.
        |> AST.Tree.ToSExpression
        |> should equal "(program:test (function foo (int y) y))"

        // Maybe there's a way to specifiy "statementy" things the same way there is
        // to specify operators? Unlikely, statements are probably going to be unique
        // in their form.

