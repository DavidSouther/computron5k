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
    TreeNode token children

let operators: List<Operator> = [
    BinaryOperator(".", 100)
    BinaryOperator("&", 65)
    BinaryOperator("*", 60)
    BinaryOperator("/", 60)
    BinaryOperator("+", 50)
    MixedOperator("-", 50, 70)
    RightOperator("~", 70)
    RightBinaryOperator("=", 10)
    GroupOperator("{", "}", true)
    BinaryOperator("[", 80, close="]")
    BinaryOperator(";", 2, continuation0=true)
    BinaryOperator(",", 3, continuation0=true)
    RightOperator("if", 5, ifStatement)
    RightOperator("return", 5)
    RightOperator("do", 5)
    { new Operator with // `let` must have an id, an =, and then an expression
        member _.Token = "let"
        member _.bindingPower = 5
        member _.leftAction _ left token =
            TreeNode {token with Value = $"{token.Value} not expected as infix"} [left]
        member _.nullAction parser token =
            let mutable id = parser.next ()
            let err = { id with
                         Value = $"Expected id, got {id.Type.Name}:{id.Value}"
                         Type = {id.Type with Error = true}}
            id <- if not(id.Type.Name = "Identifier")
                  then err
                  else id
            parser.expect "=" |> ignore
            let body = parser.expression 6
            TreeNode token [TreeLeaf(id); body] }
    { new Operator with // Call operator infix or group operator prefix
        member _.Token = "("
        member _.bindingPower = 1
        member _.leftAction parser left token =
            let right = parser.expression 0
            parser.expect(")") |> ignore
            TreeNode {token with Value = "call"} [left; right]
        member _.nullAction parser token =
            let right = parser.expression 0
            parser.expect(")") |> ignore
            right }
]

let parser = ParserFactory.For(operators)

