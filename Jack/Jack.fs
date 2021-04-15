module Jack

open AST
open Scanner
open Parser

let keywords: Set<string> = Set.ofSeq [
    "class"
    "constructor"
    "function"
    "method"
    "static"
    "field"
    "var"
    "int"
    "char"
    "boolean"
    "void"
    "true"
    "false"
    "null"
    "this"
    "let"
    "do"
    "if"
    "else"
    "while"
    "return"
]

let ifStatement (parser: Parser) (token: Token) =
    let expression = parser.expression(1)
    parser.expect("{") |> ignore
    let thenBody = parser.expression(1)
    parser.expect("}") |> ignore
    let mutable children = [expression; thenBody]
    if parser.next().Type.Name = "else"
    then children <- children @ [parser.expression(0)]
    Tree.Node(token, children)

let operators: List<Operator> = [
    RightBinaryOperator(".", 100)
    BinaryOperator("&", 65)
    BinaryOperator("*", 60)
    BinaryOperator("/", 60)
    BinaryOperator("+", 50)
    MixedOperator("-", 50, 70)
    RightOperator("~", 70)
    RightBinaryOperator("=", 10)
    GroupOperator("(", ")")
    GroupOperator("{", "}", true)
    BinaryOperator("[", 80, close="]")
    BinaryOperator(";", 2, continuation0=true)
    BinaryOperator(",", 3, continuation0=true)
    RightOperator("if", 5, ifStatement)
    RightOperator("let", 5);
    RightOperator("return", 5)
    RightOperator("do", 5)
]

let parser = ParserFactory.For(operators)

