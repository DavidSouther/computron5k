module Parser

open AST

open Scanner

type Operator =
    abstract Token: string
    abstract bindingPower: int
    abstract leftAction: Parser -> (* left *) Tree<Token> -> Token -> Tree<Token>
    abstract nullAction: Parser -> Token -> Tree<Token>
and Parser =
    abstract expression: (* mbp *) int  -> Tree<Token> 
    abstract expect: (* token *) string -> Token
    abstract next: unit -> Token

type Identifiers = TokenType
type Values = TokenType
type Whitespace = TokenType
type Comments = TokenType
type Keywords = Set<string>

let makeOperatorParts (operators: List<Operator>) =
    let allOps = operators |> List.map(fun op -> (op.Token, op))
    let opsMap = allOps |> Map.ofList
    let (allOps, _) = allOps |> List.unzip
    let opsToken = TokenType.Literal("Operator", 100, allOps)
    (opsToken, opsMap)

let errorPosition =
    { Position.File = "unknown";
        Line = -1;
        Column = -1;
        Index = -1 }

let errorToken (value: string, position: Position) =
     { Token.Value = $"'{value}'";
       Position = position;
       Type =
       { TokenType.Name = "ERROR";
         Priority = -1;
         ToMatch = []}}

let defaultToSkip = ["Whitespace"; "Comment"] |> Set.ofList

type BinaryOperator
    ( token: string,
      bindingPower: int,
      ?leftAssociative0: bool,
      ?close: string,
      ?continuation0: bool) =
    let continuation = defaultArg continuation0 false
    let nextBindingPower =
        if defaultArg leftAssociative0 true
        then bindingPower + 1
        else bindingPower - 1
    member _.Token = token
    interface Operator with
        member _.Token = token
        member _.bindingPower = bindingPower
        member t.nullAction (_: Parser) (token: Token) =
            Tree.Leaf(errorToken($"Expected {t.Token} on left side", token.Position))
        member t.leftAction (parser: Parser) (left: Tree<Token>) (token: Token) =
            let right = parser.expression nextBindingPower
            if close.IsSome then parser.expect close.Value |> ignore
            let (Node (rToken, rChildren)) = right 
            let (Node (lToken, lChildren)) = left
            if continuation &&
                token.Value = t.Token &&
                lToken.Value = token.Value
            then
                if rToken.Value = token.Value
                then Tree.Node(token, lChildren @ rChildren)
                else Tree.Node(token, lChildren @ [right])
            else Tree.Node(token, [left; right])

let RightBinaryOperator (token: string, bindingPower: int) =
    BinaryOperator(token, bindingPower, false)

type MixedOperator (token: string, bindingPower: int, prefixBindingPower: int, ?leftAssociative0: bool) =
    let nextBindingPower =
        if defaultArg leftAssociative0 true
        then bindingPower + 1
        else bindingPower - 1
    member _.Token = token
    interface Operator with
        member _.Token = token
        member _.bindingPower = bindingPower
        member t.nullAction (parser: Parser) (token: Token) =
            let right = parser.expression prefixBindingPower
            Tree.Node(token, [right]) 
        member _.leftAction (parser: Parser) (left: Tree<Token>) (token: Token) =
            let right = parser.expression nextBindingPower
            Tree.Node(token, [left; right])

type RightOperator (token: string, bindingPower: int, ?nullAction0: Parser -> Token -> Tree<Token>) =
    member _.Token = token
    interface Operator with
        member _.Token = token
        member _.bindingPower = bindingPower
        member t.nullAction (parser: Parser) (token: Token) =
            match nullAction0 with
            | Some action -> action parser token
            | None ->
                let right = parser.expression bindingPower
                Tree.Node(token, [right]) 
        member _.leftAction (parser: Parser) (left: Tree<Token>) (token: Token) =
            Tree.Leaf(errorToken($"Expected {token.Value} on right side", token.Position))

type GroupOperator (token: string, close: string, ?scope0: bool) =
    let scope = defaultArg scope0 false
    member _.Token = token
    interface Operator with
        member _.Token = token
        member _.bindingPower = 0
        member _.nullAction (parser: Parser) (token: Token) =
            let group = parser.expression 0
            let errorToken = parser.expect close
            if errorToken.Type.Name = "ERROR"
            then Tree.Node(token, [group; Tree.Leaf errorToken])
            else
                if scope
                then Tree.Node(token, [group])
                else group
        member _.leftAction (parser: Parser) (left: Tree<Token>) (token: Token) =
            let error = errorToken($"Unexpected {token.Value} in infix position", token.Position)
            Tree.Node(error, [left])

type ParserFactory (scannerFactory: ScannerFactory, operatorMap: Map<string, Operator>, ?toSkip0: Set<string>) =
    let toSkip = defaultArg toSkip0 defaultToSkip

    let doParse (scanner: Scanner): Tree<Token> = 
        let shouldSkip () =
            match scanner.Next with
            | None -> true
            | Some t -> toSkip.Contains t.Type.Name

        let consume () = while shouldSkip() do scanner.Advance() |> ignore
        let Peek () =
            consume()
            match scanner.Next with
            | None -> errorToken("ERROR", errorPosition)
            | Some t -> t

        let Advance () =
            let peek = Peek()
            scanner.Advance() |> ignore
            peek

        let mutable parser: Parser =
            { new Parser with
                member _.expression (mbp: int) =
                    Tree.Leaf(errorToken("Unimplemented Parser", errorPosition))
                member _.expect (token: string) =
                    errorToken("Unimplemented Parser", errorPosition)
                member _.next () =
                    errorToken("Unimplemented Parser", errorPosition)
                    }

        let nud (token: Token) =
            if operatorMap.ContainsKey token.Value
            then operatorMap.[token.Value].nullAction parser token
            else Tree.Leaf token

        let led (token: Token) (left: Tree<Token>) =
            if operatorMap.ContainsKey token.Value
            then operatorMap.[token.Value].leftAction parser left token
            else left

        let bp () =
            let token = Peek()
            if token.Type.Name = "EOF"
            then System.Int32.MinValue 
            else
                if operatorMap.ContainsKey token.Value
                then operatorMap.[token.Value].bindingPower
                else 0

        parser <- { new Parser with
            member _.expression (mbp: int) =
                let peek = Advance ()            
                let mutable left = nud peek
                while mbp < bp () do
                    let peek = Advance ()
                    left <- led peek left
                left
            member _.expect (token: string) =
                let peek = Advance ()
                if peek.Value = token
                then peek
                else errorToken($"Expected {token}, got {peek.Value}", peek.Position)
            member _.next () =
                Advance ()
                }

        let rootToken = 
            { Token.Value = $"program:{scanner.File}"
              Type =
                { TokenType.Name = "ROOT"
                  Priority = 0
                  ToMatch = [] }
              Position =
                { Position.File = scanner.File
                  Line = 1
                  Column = 1
                  Index = 0 }}

        let isEOF (next: Token) =
            next.Type.Name = BaseTokenTypes.EOF.Name

        // Parse until the scanner is empty
        let mutable expressions: List<Tree<Token>> = List.empty
        while not(isEOF(Peek())) do
            expressions <- expressions @ [parser.expression 0]
        Tree.Node(rootToken, expressions)
        
    member _.Parse (contents, file) =
        doParse(scannerFactory.Scan(contents, file))

    member _.ParseFile (path: string) =
        doParse(scannerFactory.From(path))

    static member For
        (
            operators: List<Operator>,
            ?keywords0: Keywords,
            ?identifiers0: Identifiers,
            ?values0: Values,
            ?comments0: Comments,
            ?whitespace0: Whitespace
         ) =
            let identifiers = defaultArg identifiers0 BaseTokenTypes.Identifier
            let values = defaultArg values0 BaseTokenTypes.Value
            let comments = defaultArg comments0 BaseTokenTypes.Comment
            let whitespace = defaultArg whitespace0 BaseTokenTypes.Whitespace

            let (operatorTokenType, operatorMap) = makeOperatorParts operators
            let mutable tokenTypes = [operatorTokenType; identifiers; values; comments; whitespace; BaseTokenTypes.Error; BaseTokenTypes.EOF]

            let keywords = defaultArg keywords0 Set.empty
            if not keywords.IsEmpty then
                let keywords = TokenType.Literal("Keywords", 80, keywords |> Set.toList)
                tokenTypes <- tokenTypes @ [keywords]
                
            let scanner = ScannerFactory tokenTypes
            ParserFactory(scanner, operatorMap)
