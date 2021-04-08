module Scanner.Test

open NUnit.Framework
open FsUnit
open Scanner 
open System.Text.RegularExpressions

[<TestFixture>]
type BaseTokenTypes () =
    let matched (tokenType: TokenType) (token: string) =
        let matched =
            tokenType.ToMatch
            |> List.map(fun s -> Regex(sprintf("^%s")(s), RegexOptions.Singleline))
            |> List.map(fun r -> r.Match token)
            |> List.filter(fun m -> m.Success)
            |> List.sortBy(fun m -> -m.Value.Length)
            |> List.tryHead
        if matched.IsSome && matched.Value.Success
        then Some matched.Value.Value
        else None

    let matches (tokenType: TokenType) (token: string) : bool =
        (matched tokenType token).IsSome

    let matchAll (tokenType: TokenType) (tokens: List<string>) =
        for token in tokens do 
            match matched tokenType token with
            | None -> Assert.Fail(sprintf("Could not match %s")(token))
            | Some (actual) -> Assert.AreEqual(token, actual)
            
    let matchNone (tokenType: TokenType) (tokens: List<string>) =
        for token in tokens do 
            match matched tokenType token with
            | None -> ()
            | Some (a) -> Assert.That(token.StartsWith(a), Is.False, token)

    [<Test>]
    member _.EOF () =
        matches BaseTokenTypes.EOF "" |> should be True
        matches BaseTokenTypes.EOF "hello" |> should be False 

    [<Test>]
    member _.Error () =
        matches BaseTokenTypes.Error "abc" |> should be True 
        matches BaseTokenTypes.Error "" |> should be False 

    [<Test>]
    member _.Identifier () =
        matchAll BaseTokenTypes.Identifier [
            "abc"
            "ab1"
            "camel_case"
            "snakeCase"
            "_privateCase"
            "PascalCase"
            "UPPER_CASE"
        ]

        matchNone BaseTokenTypes.Identifier [
            "123"
            "+-()"
        ]

        let matched = matched BaseTokenTypes.Identifier "dash-case"
        matched.IsSome |> should be True
        matched.Value |> should equal "dash"

    [<Test>]
    member _.Value () =
        matchAll BaseTokenTypes.Value [
            "123"
            "123.456"
            "-123" 
            "-123.456"
            "-123._456"
            "-0.4_56"
            "0xDeAdBeAF01"
            "0b1100_0101"
        ]

        matchAll BaseTokenTypes.Value [
            @"'hello world'"
            @"'hello \' world'"
            @"''"
        ]

        matchNone BaseTokenTypes.Value [
            "abc"
            "-.45_6" 
            @"'hello world"
            @"""hello world"
        ]

    [<Test>]
    member _.Comment () =
        matchAll BaseTokenTypes.Comment [
            "// comment"
            "// comment 'whoo'"
            "/* a long comment */"
            "/** A long\n * comment with\n * lines. */"
        ]

[<TestFixture>]
type TestMatcher () =
    let matcher = Matcher BaseTokenTypes.ALL
    let expect (contents: string, offset: int) (value: string, tokenType: TokenType) =
        match matcher.MatchTokenType(contents, offset) with
        | None -> Assert.Fail(sprintf("Expected %s got None")(value))
        | Some (matchedType, matched) ->
            Assert.AreEqual(matchedType.Name, tokenType.Name)
            Assert.AreEqual(value, matched)

    [<Test>]
    member _.MatchTokenType () =
        let contents = "alpha 12345 // comment"
        expect (contents, 0) ("alpha", BaseTokenTypes.Identifier)
        expect (contents, 5) (" ", BaseTokenTypes.Whitespace)
        expect (contents, 6) ("12345", BaseTokenTypes.Value)
        expect (contents, 11) (" ", BaseTokenTypes.Whitespace)
        expect (contents, 12) ("// comment", BaseTokenTypes.Comment)
        expect (contents, 22) ("", BaseTokenTypes.EOF)

[<TestFixture>]
type TestScanner () =
    let keywordTokenType = TokenType("Keyword", 100, [
        "if"
        "then"
        "else"
    ])

    let operatorTokenType = LiteralTokenType("Operator", 100, [
        "<"
    ])

    let makeScanner input =
        let tokenTypes: List<TokenType> = BaseTokenTypes.ALL @ [
            keywordTokenType
            operatorTokenType
        ]
        ScannerFactory(tokenTypes).Scan(input, "test")

    let expectToken (token: Token) ((tokenType: string, value: string, line: int, column: int))=
        token.tokenType.Name |> should equal tokenType
        token.value |> should equal value
        token.position.line |> should equal line
        token.position.column |> should equal column
        
    [<Test>]
    member this.advancesThough () =
        let scanner = makeScanner "if a < b\nthen b\nelse a"
        let expectedTokens: List<string*string*int*int> = [
            ("Keyword", "if", 1, 1)
            ("Whitespace", " ", 1, 3)
            ("Identifier", "a", 1, 4)
            ("Whitespace", " ", 1, 5)
            ("Operator", "<", 1, 6)
            ("Whitespace", " ", 1, 7)
            ("Identifier", "b", 1, 8)
            ("Whitespace", "\n", 1, 9)
            ("Keyword", "then", 2, 1)
            ("Whitespace", " ", 2, 5)
            ("Identifier", "b", 2, 6)
            ("Whitespace", "\n", 2, 7)
            ("Keyword", "else", 3, 1)
            ("Whitespace", " ", 3, 5)
            ("Identifier", "a", 3, 6)
            ("EOF", "", 3, 7)
        ] 
        for expected in expectedTokens do
            let token = scanner.Advance ()
            expectToken token.Value expected

