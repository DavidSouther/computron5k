module FrontEnd

type TokenType =
    abstract member matches : string -> Option<string>

type LiteralTokenType(tokens: Set<string>) =
    interface TokenType with
        member this.matches input = 
            let matchingTokens = tokens |> Set.filter(fun token -> input.StartsWith(token))
            match matchingTokens.Count with
                | 0 -> None
                | _ -> Some input

type Position = { file: string; line: int; column: int; }

type Token = { tokenType: TokenType; value: string; position: Position }


