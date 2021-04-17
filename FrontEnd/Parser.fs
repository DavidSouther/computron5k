module Parser

open AST
open Scanner

type TreeData =
    { Token: Token; Data: Map<string, obj> }
    override t.ToString () = t.Token.ToString()

let TreeNode (token: Token) (children: List<Tree<TreeData>>): Tree<TreeData> =
    Tree.Node({ TreeData.Token = token; Data = Map.empty }, children)
let TreeLeaf (token: Token): Tree<TreeData> = TreeNode token []

type Operator =
    abstract Token: string
    abstract bindingPower: int
    abstract leftAction: Parser -> (* left *) Tree<TreeData> -> Token -> Tree<TreeData>
    abstract nullAction: Parser -> Token -> Tree<TreeData>
and Parser =
    abstract expression: (* mbp *) int  -> Tree<TreeData> 
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
       { TokenType.Name = "ParseError";
         Priority = -1;
         ToMatch = [];
         Error = true }}

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
            TreeLeaf(errorToken($"Expected {t.Token} on left side", token.Position))
        member t.leftAction (parser: Parser) (left: Tree<TreeData>) (token: Token) =
            let right = parser.expression nextBindingPower
            if close.IsSome then parser.expect close.Value |> ignore
            let (Node (rData, rChildren)) = right 
            let (Node (lData, lChildren)) = left
            if continuation &&
                token.Value = t.Token &&
                lData.Token.Value = token.Value
            then
                if rData.Token.Value = token.Value
                then TreeNode token (lChildren @ rChildren)
                else TreeNode token (lChildren @ [right])
            else TreeNode token [left; right]

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
            TreeNode token [right]
        member _.leftAction (parser: Parser) (left: Tree<TreeData>) (token: Token) =
            let right = parser.expression nextBindingPower
            TreeNode token [left; right]

type RightOperator (token: string, bindingPower: int, ?nullAction0: Parser -> Token -> Tree<TreeData>) =
    member _.Token = token
    interface Operator with
        member _.Token = token
        member _.bindingPower = bindingPower
        member t.nullAction (parser: Parser) (token: Token) =
            match nullAction0 with
            | Some action -> action parser token
            | None ->
                let right = parser.expression bindingPower
                TreeNode token [right]
        member _.leftAction (parser: Parser) (left: Tree<TreeData>) (token: Token) =
            TreeLeaf(errorToken($"Expected {token.Value} on right side", token.Position))

type GroupOperator (token: string, close: string, ?scope0: bool) =
    let scope = defaultArg scope0 false
    interface Operator with
        member _.Token = token
        member _.bindingPower = 0
        member _.nullAction (parser: Parser) (token: Token) =
            let group = parser.expression 0
            let errorToken = parser.expect close
            if errorToken.Type.Name = "ParseError"
            then TreeNode token [group; TreeLeaf errorToken]
            else
                if scope
                then TreeNode token [group]
                else group
        member _.leftAction (parser: Parser) (left: Tree<TreeData>) (token: Token) =
            let error = errorToken($"Unexpected {token.Value} in infix position", token.Position)
            TreeNode error [left]

type ParserFactory (scannerFactory: ScannerFactory, operatorMap: Map<string, Operator>, ?toSkip0: Set<string>) =
    let toSkip = defaultArg toSkip0 defaultToSkip

    let doParse (scanner: Scanner): Tree<TreeData> = 
        let shouldSkip () =
            match scanner.Next with
            | None -> true
            | Some t -> toSkip.Contains t.Type.Name

        let consume () = while shouldSkip() do scanner.Advance() |> ignore
        let Peek () =
            consume()
            match scanner.Next with
            | None -> errorToken("UNEXPECTED_EOF", errorPosition)
            | Some t -> t

        let Advance () =
            let peek = Peek()
            scanner.Advance() |> ignore
            peek

        let mutable parser: Parser =
            { new Parser with
                member _.expression (mbp: int) =
                    TreeLeaf(errorToken("UNIMPLEMENTED_PARSER", errorPosition))
                member _.expect (token: string) =
                    errorToken("UNIMPLEMENTED_PARSER", errorPosition)
                member _.next () =
                    errorToken("UNIMPLEMENTED_PARSER", errorPosition)
                    }

        let nud (token: Token) =
            if operatorMap.ContainsKey token.Value
            then operatorMap.[token.Value].nullAction parser token
            else TreeLeaf token

        let led (token: Token) (left: Tree<TreeData>) =
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
                  ToMatch = []
                  Error = false}
              Position =
                { Position.File = scanner.File
                  Line = 1
                  Column = 1
                  Index = 0 }}

        let isEOF (next: Token) =
            next.Type.Name = BaseTokenTypes.EOF.Name

        // Parse until the scanner is empty
        let mutable expressions: List<Tree<TreeData>> = List.empty
        while not(isEOF(Peek())) do
            expressions <- expressions @ [parser.expression 0]
        TreeNode rootToken expressions
        
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
