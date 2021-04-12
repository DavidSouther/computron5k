module ac

open Parser
open Scanner

let operatorTokens = TokenType.Literal("Operators", 20, ["+"; "-"; "="; "i"; "f"; "p"])
let identifiers = TokenType.From("Identifiers", 20, ["[a-eghj-oq-z]"])
let whitespace = BaseTokenTypes.Whitespace

let scanner = ScannerFactory [operatorTokens; identifiers; BaseTokenTypes.Value; BaseTokenTypes.Whitespace; BaseTokenTypes.EOF;]

let decl (parser: Parser) (token: Token) =
    AST.Tree.Node(token, [AST.Tree.Leaf(parser.next())])

let operators: List<Operator> = [
    RightOperator("f", -5, decl)
    RightOperator("i", -5, decl)
    RightOperator("p", -5, decl)
    BinaryOperator("*", 30)
    BinaryOperator("/", 30)
    BinaryOperator("+", 20)
    MixedOperator("-", 20, 40)
    RightBinaryOperator("=", 10)
]

let parser = ParserFactory.For(operators, identifiers0=identifiers)

let isEOF (next: Option<Token>) =
    next.IsSome && next.Value.Type.Name = BaseTokenTypes.EOF.Name

