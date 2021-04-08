module Scanner 


type TokenType (name: string, priority: int, toMatch: List<string>) =
    member _.Name = name
    member _.Priority = priority
    member _.ToMatch = toMatch

let LiteralTokenType (name: string, priority: int, toMatch: List<string>) =
    let toMatch = toMatch |> List.map System.Text.RegularExpressions.Regex.Escape
    TokenType(name, priority, toMatch)

type Position = { file: string; index: int; line: int; column: int; }

type Token = { tokenType: TokenType; value: string; position: Position }


module BaseTokenTypes =
    let EOF = TokenType("EOF", 0, [@"$"])
    let Error = TokenType("Error", -1, [@"."])

    let Identifier = TokenType("Identifier", 50, [
        @"[a-zA-Z_][a-zA-Z0-9_]*"
    ])

    let Value = TokenType("Value", 30, [
         @"-?[0-9_]+" // Integers
         @"-?([0-9_]+|0)\.[0-9_]+" // Decimals
         @"0x[0-9A-Fa-f_]+" // Hexademimal
         @"0b[01_]+" // Binary
         @"'(.*?[^\\]|)'" // ' Strings
    ])

    let Comment = TokenType("Comment", 30, [
        @"//[^\n]*" // Single line
        @"/\*.*?\*/" // Multiline, lazy 
    ])

    let Whitespace = TokenType("Whitespace", 30, [
        @"\s+" // Multiline, lazy 
    ])

    let ALL: List<TokenType> = [EOF; Error; Identifier; Value; Comment; Whitespace;]

    (*
[7:14 PM, 4/6/2021] David S: Generate a string token for each regex
[7:15 PM, 4/6/2021] David S: Make a capture group (?<_gen_>regex)
[7:16 PM, 4/6/2021] David S: Put \_gen_ to regex in a map
[7:16 PM, 4/6/2021] David S: Join all of them with |
[7:16 PM, 4/6/2021] David S: Match
[7:16 PM, 4/6/2021] David S: Check the matched groups
[7:17 PM, 4/6/2021] David S: Pull the regexes out of the map
    *)

let tokenTypeList (tokenType: TokenType) =
    let toMatchList (j: int) (toMatch: string) =
        let key = sprintf "__matcher_%s_%d_" tokenType.Name j 
        let regex =  sprintf "(?<%s>%s)" key toMatch
        ((key, tokenType), regex)
    tokenType.ToMatch |> List.mapi toMatchList

type Matcher (tokenTypes: List<TokenType>) =
    let (keys, regexes) =
        tokenTypes
        |> List.sortBy (fun t -> -t.Priority)
        |> List.map tokenTypeList
        |> List.collect (fun i -> i)
        |> List.unzip
    let merged = regexes |> String.concat "|"
    let regexStr = sprintf "\G%s" merged
    let regex = System.Text.RegularExpressions.Regex(regexStr, System.Text.RegularExpressions.RegexOptions.Singleline)
    let map = keys |> Map.ofList

    member this.MatchTokenType(contents: string, offset: int) =
        let matches = regex.Match(contents, offset)
        let groups = matches.Groups.Values
        groups
        |> Seq.filter(fun group -> group.Success)
        |> Seq.filter(fun group -> map.ContainsKey group.Name)
        |> Seq.map (fun group -> (map.[group.Name], group.Value))
        // TODO try commenting out this sortBy
        |> Seq.sortBy (fun (tokenType, _) -> -tokenType.Priority)
        |> Seq.take 1
        |> Seq.tryHead

type Scanner (matcher: Matcher, contents: string, file: string) = 
    // let mutable contents = contents
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

    let step (stride: string) =
        let d = stride.Length
        let q = stride.IndexOf("\n")
        if q > -1 then
            line <- line + 1
            column <- d - q
        else
            column <- column + d
        index <- index + d

    let errorToken () = (
        let token = makeToken BaseTokenTypes.Error contents
        tick()
        token
    )

    let filledToken tokenType value =
        let token = makeToken tokenType value 
        step value
        token

    member this.Next = next

    member this.Advance ()  =
        next <- Some(
            match matcher.MatchTokenType(contents, index) with
            | None -> errorToken()
            | Some((tokenType, matched)) -> filledToken tokenType matched
        )
        next
            
type ScannerFactory (tokenTypes: List<TokenType>) =
    let matcher = Matcher tokenTypes

    member this.Scan (contents: string, file: string) =
        Scanner(matcher, contents, file) 

    member this.from(path: string) =
        let contents = System.IO.File.ReadAllText(path)
        let file = System.IO.Path.GetFileName(path)
        Scanner(matcher, contents, file)

