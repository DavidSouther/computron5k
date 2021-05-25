module Scanner 

open AST

module BaseTokenTypes =
    let EOF = TokenType.From("EOF", 0, [@"$"])
    let Error = TokenType.From("Unknown", -1, [@"."])

    let Identifier = TokenType.From("Identifier", 50, [
        @"[a-zA-Z_][a-zA-Z0-9_]*"
    ])

    let Value = TokenType.From("Value", 30, [
         @"-?([0-9_]+|0)\.[0-9_]+" // Decimals
         @"-?[0-9_]+" // Integers
         @"0x[0-9A-Fa-f_]+" // Hexademimal
         @"0b[01_]+" // Binary
         @"'(.*?[^\\]|)'" // ' Strings
    ])

    let Comment = TokenType.From("Comment", 30, [
        @"//[^\n]*" // Single line
        @"/\*.*?\*/" // Multiline, lazy 
    ])

    let Whitespace = TokenType.From("Whitespace", 30, [
        @"\r?\n\s+" // Newline and indentation, explicitly as its own class 
        @"\s+" // Multiline, lazy 
    ])

    let ALL: List<TokenType> = [EOF; Error; Identifier; Value; Comment; Whitespace;]


let tokenTypeList (tokenType: TokenType) =
    let key = $"__matcher_{tokenType.Name}_"
    let options = String.concat "|" tokenType.ToMatch
    (key, tokenType), $"(?<{key}>{options})"

// Given a list of TokenTypes, a Matcher will return the next token type
// that matches a given input string, starting at some offset.
type Matcher (tokenTypes: List<TokenType>) =
    // TODO error when TokenType Names collapse
    // TODO develop a subset of regex & write a custom DFA
    // TODO compare this approach to matching against the list of regexes

    (* Treat each input token ToMatch as a regex subexpression. Literal
    types are valid regex literals (after being escaped). All regexes
    in a ToMatch list are joined using alternation, and put in a named
    matching group using the TokenType name. The regex subexpressions for
    all the TokenTypes are joined using alternation, with a leading
    previos match anchor to ensure the Matcher is anchored to the next
    point of interest in the text. The Map then goes from each subexpression
    name back to the TokenType that it came from. *)
    let (keys, regexes) =
        tokenTypes
        |> List.sortBy (fun t -> -t.Priority)
        |> List.map tokenTypeList
        |> List.unzip
    let merged = String.concat "|" regexes
    let regexStr = $"\G{merged}"
    let Regex = System.Text.RegularExpressions.Regex(regexStr, System.Text.RegularExpressions.RegexOptions.Singleline)
    let Map = keys |> Map.ofList

    member this.MatchTokenType(contents: string, offset: int) =
        if offset >= contents.Length
        then
            Some (BaseTokenTypes.EOF, "")
        else
            let matches = Regex.Match(contents, offset)
            let groups = matches.Groups.Values
            groups
            |> Seq.filter(fun group -> group.Success)
            |> Seq.filter(fun group -> Map.ContainsKey group.Name)
            |> Seq.map (fun group -> (Map.[group.Name], group.Value))
            |> Seq.tryHead

type Scanner =
    abstract Next: Option<Token>
    abstract Advance: unit -> Option<Token>

// Scanner takes a Matcher, the string contents of a file, and a file
// name (to use in error messaging). One scanner instance encloses the
// mutable state of scanning through that file. It maintains a Next
// property which is the Token at the cursor, and an Advance method to
// move the cursor to the next token (and update next to point at the
// token it just crossed over).
type BaseScanner (matcher: Matcher, contents: string, file: string) = 
    let mutable index = 0
    let mutable line = 1
    let mutable column = 1
    let mutable next: Option<Token> = None

    let position () =
        { Position.File = file;
          Index = index
          Line = line;
          Column = column; }

    let makeToken (tokenType: TokenType) (value: string) =
        { Token.Type = tokenType;
          Value = value;
          Position = position(); }

    let step (stride: string) =
        let d = stride.Length
        let carriageNewline = stride.Contains("\r\n")
        let mutable q = stride.IndexOf("\n")
        if q = -1 then q <- stride.IndexOf("\r\n")
        if q > -1 then
            if carriageNewline then q <- q + 1
            line <- line + 1
            column <- d - q
        else
            column <- column + d
        index <- index + d

    let errorToken () =
        let errChar = contents.Substring(index, 1)
        let token = makeToken BaseTokenTypes.Error errChar 
        step errChar
        token

    let filledToken tokenType value =
        let token = makeToken tokenType value 
        step value
        token

    let advance () = 
        match matcher.MatchTokenType(contents, index) with
        | Some(tokenType, matched) -> filledToken tokenType matched
        | None -> errorToken ()
        |> Some

    member _.File = file

    interface Scanner with
        override _.Next = next

        override t.Advance () =
            next <- advance ()
            next
            
type SkipScanner (scanner: Scanner, toSkip: List<TokenType>) =
    let shouldSkip () =
        match scanner.Next with
        | Some t -> toSkip |> List.contains t.Type
        | None -> true 

    interface Scanner with
        member _.Next =
            match scanner.Next with
            | None -> { Token.Type = BaseTokenTypes.Error;
                        Value = "UNEXPECTED_EOF";
                        Position = { Position.File = "";
                                     Line = -1;
                                     Column = -1;
                                     Index = -1; } } |> Some
            | Some _ -> scanner.Next

        member t.Advance () =
            scanner.Advance() |> ignore
            while shouldSkip() do
                scanner.Advance() |> ignore
            (t:>Scanner).Next

// To simplify scanning over multiple files, the ScannerFactory takes
// token types, and passes a Matcher for those TokenTypes to a Scanner
// for any file. If given a string path, performs IO to load the file.
type ScannerFactory (tokenTypes: List<TokenType>, ?toSkip0: List<TokenType>) =
    let toSkip = defaultArg toSkip0 []
    let tokenTypes = tokenTypes @ toSkip
    let matcher = Matcher tokenTypes

    member this.Scan (contents: string, file: string): Scanner =
        let baseScanner = BaseScanner(matcher, contents, file) 
        let scanner = match toSkip0 with
                      | Some(skip) -> SkipScanner(baseScanner, skip) :> Scanner
                      | None -> baseScanner :> Scanner
        scanner.Advance () |> ignore // Start the scanner
        scanner

    member this.From(path: string) =
        let contents = System.IO.File.ReadAllText(path)
        let file = System.IO.Path.GetFileName(path)
        this.Scan(contents, file)

