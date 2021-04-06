module Scanner 

type TokenType =
    abstract member Priority : int
    abstract member Matches : string -> Option<string>

type Position = { file: string; index: int; line: int; column: int; }

type Token = { tokenType: TokenType; value: string; position: Position }

type LiteralTokenType (tokens: List<string>, ?priority0: int) =
    let priority = defaultArg priority0 20
    member this.tokens =
        tokens
        |> List.sortBy(fun token -> token.Length)
        |> List.rev
    interface TokenType with
        member this.Priority = priority
        member this.Matches input = 
            let matchingTokens =
                this.tokens
                |> List.filter(fun token -> input.StartsWith(token))
            match matchingTokens with
                | [] -> None
                | head :: _ -> Some head

type RegexTokenType (matchers: List<System.Text.RegularExpressions.Regex>, ?priority0: int) =
    let priority = defaultArg priority0 10
    let (anchored, free) =
        matchers
        |> List.partition(fun regex -> regex.ToString().StartsWith('^'))

    member this.errors: List<string> =
        free
        |> List.map(fun regex -> sprintf("Unanchored regex token: %s")(regex.ToString()))
    member this.matchers = anchored

    interface TokenType with
        member this.Priority = priority
        member this.Matches input =
            let matchingTokens =
                this.matchers
                |> List.map(fun regex -> regex.Match(input))
                |> List.filter(fun matched -> matched.Success)
                |> List.map(fun matched -> matched.Value)
                |> List.sortBy(fun token -> token.Length)
                |> List.rev
            match matchingTokens with
                | [] -> None
                | head :: _ -> Some head

type EOFTokenType () =
    interface TokenType with
        member this.Priority = 0
        member this.Matches input =
            if input.Length = 0 then Some "" else None

type ErrorTokenType () =
    interface TokenType with
        member this.Priority = -1
        member this.Matches _ = None

module BaseTokenTypes =
    let EOFTokenType = EOFTokenType  ()
    let ErrorTokenType = ErrorTokenType ()
    let WhiteSpaceTokenType = RegexTokenType([System.Text.RegularExpressions.Regex("^\s+")], 4)
    let NewlineTokenType = RegexTokenType([System.Text.RegularExpressions.Regex("^\n")], 6)

type Scanner (contents: string, file: string) = 
    let mutable contents = contents
    let mutable index = 0
    let mutable line = 1
    let mutable column = 1
    let mutable tokenTypes: List<TokenType> = []
    let mutable next: Option<Token> = None

    let position () = {
        Position.file = file;
        index = index
        line = line;
        column = column;
    }

    let makeToken (tokenType: TokenType) (value: string) = {
        Token.tokenType = tokenType;
        value = value;
        position = position();
    }

    let tick () =
        if contents.StartsWith "\n" then
            column <- 1
            line <- line + 1
        else
            column <- column + 1
        index <- index + 1
        contents <- contents.Substring(1)

    let step (distance: string) =
        let q = distance.IndexOf("\n")
        if q > -1 then
            line <- line + 1
            column <- distance.Length - q
        else
            column <- column + distance.Length
        index <- index + 1
        contents <- contents.Substring(distance.Length)

    let errorToken () = (
        let token = makeToken BaseTokenTypes.ErrorTokenType contents
        tick()
        token
    )

    let emptyToken tokenType =
        let token = makeToken tokenType "" 
        tick()
        token

    let filledToken tokenType value =
        let token = makeToken tokenType value 
        step value
        token

    member this.TokenTypes = tokenTypes
    member this.Next = next

    static member from(path: string) =
        let contents = System.IO.File.ReadAllText(path)
        let file = System.IO.Path.GetFileName(path)
        Scanner(contents, file)

    member this.AddTokenTypes(types: List<TokenType>) =
        tokenTypes <- this.TokenTypes @ types
        this

    member this.MatchTokenType() =
        let matchingTypes =
            this.TokenTypes
            |> List.map(fun tokenType -> (tokenType, tokenType.Matches(contents)))
            |> List.filter(fun (_, matches) -> matches.IsSome)
            |> List.sortBy(fun (tokenType, _) -> -tokenType.Priority)
        match matchingTypes with
            | [] -> None
            | head :: _ -> Some head

    member this.Advance ()  =
        next <- Some(
            match this.MatchTokenType() with
            | None -> errorToken()
            | Some((tokenType, None)) -> emptyToken tokenType
            | Some((tokenType, Some(matched))) -> filledToken tokenType matched
        )
        next
            

