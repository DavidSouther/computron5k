module Production

open AST 
open Scanner
open Parser

type Matcher =
    abstract matches: Token -> bool

type Production =
    abstract Matches: Token -> bool
    abstract Consume: Token * Scanner -> Tree<TreeData>

type ConstProduction (value: string) =
    member _.Value = value
    interface Production with
        override t.Matches (token: Token) =
            token.Value = t.Value
        override _.Consume (token: Token, scanner: Scanner) =
            scanner.Advance() |> ignore
            TreeLeaf(token)

type SimpleProduction (matcher: TokenType) =
    member _.Matcher = matcher
    interface Production with
        override t.Matches (token: Token) =
            token.Type = t.Matcher
        override _.Consume (token: Token, scanner: Scanner) =
            scanner.Advance() |> ignore
            TreeLeaf(token)

type OptionalProduction (production: Production) =
    interface Production with
        override _.Matches (_: Token) = true
        override _.Consume (token: Token, scanner: Scanner) = 
            if production.Matches token
            then production.Consume (token, scanner)
            else Tree<TreeData>.Empty

type OneOfProduction (productions: List<Production>) =
    interface Production with
        override _.Matches (_: Token) = true
        override _.Consume (token: Token, scanner: Scanner) =
            match productions |> List.tryFind(fun p -> p.Matches token) with
            | Some(production) -> production.Consume(token, scanner)
            | None -> 
                let message = $"Could not match oneof with {token.Value}"
                let error = TreeLeaf token |> TreeErrors.Add message
                scanner.Advance () |> ignore
                error

type RepeatedProduction (name: string, production: Production) =
    let repeatedTokenType = { TokenType.Name = "::RepeatedProduction::";
                               ToMatch = [];
                               Priority = -1;
                               Error = false; }
    let tokenAt (position: Position) =
        { Token.Value = name;
          Position = position;
          Type = repeatedTokenType; }
    interface Production with
        override _.Matches (token: Token) =
             production.Matches token
        override _.Consume (t: Token, scanner: Scanner) =
            let mutable tree: Option<Tree<TreeData>> = None
            let errPosition = t.Position
            let mutable token = Some(t)
            while token.IsSome && production.Matches token.Value do
                let t = production.Consume (token.Value, scanner)
                tree <- match tree with
                        | Some(p) -> p |> Tree.AppendChild t
                        | _ -> TreeNode(tokenAt(token.Value.Position))([t])
                        |> Some
                token <- scanner.Next
            match tree with
            | Some(t) -> t
            | None ->
                let message = $"Could not consume repeated production after matching {token.Value}\n\t(This should never happen)"
                let error = 
                    match token with
                    | Some(tk) -> tk 
                    | None -> errorToken("Scanner ended unexpectedly", errPosition)
                    |> TreeLeaf
                    |> TreeErrors.Add message
                scanner.Advance() |> ignore
                error

type StatementProduction (name: string, productions: List<Production>) =
    let first = productions.Head
    let tokenType = {
        TokenType.Name = "::StatementProduction::";
        Priority = 0;
        ToMatch = [];
        Error = false; }
    interface Production with
        override _.Matches (token: Token) =
            first.Matches token
        override _.Consume (t: Token, scanner: Scanner) =
            let errPosition = t.Position
            let mutable token = Some t
            let mutable tree = TreeLeaf {
                                Token.Value = name;
                                Position = t.Position;
                                Type = tokenType; }
            for production in productions do
                if token.IsSome && production.Matches token.Value
                then
                    let t = production.Consume (token.Value, scanner)
                    token <- scanner.Advance()
                    tree <- tree |> Tree.AppendChild t
                else
                    let message = $"Could not find production in statement"
                    tree <- tree |> TreeErrors.Add message
            tree

type ExpressionProduction (operators: List<Operator>) =
    let operatorMap = makeOperatorParts operators
    let parser = ParserFactory ()
