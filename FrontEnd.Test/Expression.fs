module Parser.Test

open FsUnit
open NUnit.Framework

open AST
open Scanner
open Parser

[<TestFixture>]
type TestParser () =
    let operators: List<Operator> = [
        RightBinaryOperator(".", 100)
        BinaryOperator("*", 60)
        BinaryOperator("/", 60)
        BinaryOperator("+", 50)
        MixedOperator("-", 50, 70)
        RightBinaryOperator("=", 10)
        GroupOperator("(", ")")
        GroupOperator("{", "}", scope0=true)
        BinaryOperator("[", 80, close="]")
        { new Operator with
            member _.Tokens = ["?"; ":"]
            member t.bindingPower = 20
            member t.leftAction (parser: Parser) (condition: Tree<TreeData>) (token: Token) =
                let bp = t.bindingPower - 1
                let truth = parser.expression bp
                let error = parser.expect ":" |> TreeLeaf
                let falsy = parser.expression bp
                TreeNode token [condition; truth; falsy; error]
            member _.nullAction (parser: Parser) (token: Token) =
                let error = errorToken($"Unexpected {token.Value} in prefix position", token.Position)
                TreeLeaf error }
    ]

    let Parse (parser: ParserFactory) (input: string) =
        let scannerFactory = ScannerFactory([BaseTokenTypes.Identifier; BaseTokenTypes.Value] @ parser.TokenTypes, [BaseTokenTypes.Whitespace])
        let scanner = scannerFactory.Scan(input, "test")
        let tree = parser.Parse(scanner)
        match tree with
        | Node (t, []) -> tree
        | Node (t, c) -> c.Head
        | Empty -> Empty
        |> Tree.ToSExpression

    [<Test>]
    member _.TestParse () =
        let parse = ParserFactory.For operators |> Parse
        parse "1" |> should equal "1"
        parse "1 + 2 + 3 + 4" |> should equal "(+ (+ (+ 1 2) 3) 4)"
        parse "1 + 2 * 3" |> should equal "(+ 1 (* 2 3))"
        parse "a + b * c * d + e" |> should equal "(+ (+ a (* (* b c) d)) e)"
        parse "f . g . h" |> should equal "(. f (. g h))"
        parse "-9" |> should equal "(- 9)"
        parse "(1 + 2) * 3" |> should equal "(* (+ 1 2) 3)"
        parse "(((0)))" |> should equal "0"
        parse "x[0][1]" |> should equal "([ ([ x 0) 1)"
        parse "{ b = c }" |> should equal "({ (= b c))"
        (* TODO
        parse "a ? b : c" |> should equal "(? a b c :)"
        parse "a = 0 ? b : c = d" |> should equal "(= a (= (? 0 b c :) d))"
        parse "a ? b ? c : d : e" |> should equal "(? a (? b c d :) e :"
        parse "a ? b : c ? d : e" |> should equal "(? a b (? c d e :) :)"
        *)


    [<Test>]
    member _.TestContinuation () =
        let parse =
            ParserFactory.For [
                BinaryOperator(";", 1, continuation0=true)
                RightBinaryOperator("=", 10)
            ]
            |> Parse

        parse "a; b; c" |> should equal "(; a b c)"
        parse "a = b; c" |> should equal "(; (= a b) c)"
