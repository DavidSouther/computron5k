module Parser.Test

open FsUnit
open NUnit.Framework

open AST
open Parser

[<TestFixture>]
type TestParser () =
    (*
    let infix: Map<string, int * int * string> =
        [ ("=", (2, 1, ""))
          ("?", (4, 3, ":"))
          ("+", (5, 6, ""))
          ("-", (5, 6, ""))
          ("*", (7, 8, ""))
          ("/", (7, 8, ""))
          (".", (14, 13, ""))
          ]
        |> Map.ofList

    let prefix: Map<string, int * string> =
        [ ("+", (9, ""))
          ("-", (9, ""))
          ("(", (0, ")"))
          ]
        |> Map.ofList
    
    let postfix: Map<string, int * string> =
        [ ("!", (11, ""))
          ("[", (11, "]"))
          ]
        |> Map.ofList
    *)

    let operators: List<Operator> = [
        RightBinaryOperator(".", 100)
        BinaryOperator("*", 60)
        BinaryOperator("/", 60)
        BinaryOperator("+", 50)
        MixedOperator("-", 50, 70)
        RightBinaryOperator("=", 10)
        GroupOperator("(", ")")
        GroupOperator("{", "}")
    ]
    let parser = ParserFactory.For operators

    let parse input =
        let tree = parser.Parse(input, "test")
        match tree with
        | Node (t, []) -> tree
        | Node (t, c) -> c.Head
        |> Tree.ToSExpression

    [<Test>]
    member _.TestParse () =
        parse "1" |> should equal "1"
        parse "1 + 2 + 3 + 4" |> should equal "(+ (+ (+ 1 2) 3) 4)"
        parse "1 + 2 * 3" |> should equal "(+ 1 (* 2 3))"
        parse "a + b * c * d + e" |> should equal "(+ (+ a (* (* b c) d)) e)"
        parse "f . g . h" |> should equal "(. f (. g h))"
        parse "-9" |> should equal "(- 9)"
        parse "(1 + 2) * 3" |> should equal "(* (+ 1 2) 3)"
        (*
        parse "-9!" |> should equal "(- (! 9))"
        parse "(((0)))" |> should equal "0"
        parse "x[0][1]" |> should equal "([ ([ x 0) 1)"
        parse "a ? b : c ? d : e" |> should equal "(? a b (? c d e))"
        parse "a = 0 ? b : c = d" |> should equal "(= a (= (? 0 b c) d))"
        *)

