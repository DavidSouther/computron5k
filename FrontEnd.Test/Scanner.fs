module Scanner.Test

open System.Text.RegularExpressions
open NUnit.Framework
open Scanner 
open Scanner.BaseTokenTypes

[<TestFixture>]
type TestRegexTokenType() =

    [<Test>]
    member this.matchesLongestRegex() =
        let regexes = [
            Regex @"^fo+"
            Regex @"^ba+r"
            Regex @"^ba+z"
        ]
        let tokenType = RegexTokenType(regexes) :> TokenType
        let input = "foo bar"
        let actual = tokenType.Matches(input)
        let expected = Some "foo"
        Assert.That(actual, Is.EqualTo(expected))

    [<Test>]
    member this.partitionsUnanchoredRegexes() =
        let regexes = [
            Regex @"^fo+"
            Regex @"^ba+r"
            Regex @"ba+z"
        ]
        let tokenType = RegexTokenType(regexes)
        Assert.That(tokenType.errors.Length, Is.EqualTo(1))
        Assert.That(tokenType.errors.Head.Contains("ba+z"))

[<TestFixture>]
type TestLiteralTokenType () =
    [<Test>]
    member this.emptyInput() =
        let tokenType = LiteralTokenType(List.empty) :> TokenType
        let input = ""
        let actual = tokenType.Matches input
        let expected = None 
        Assert.That(actual, Is.EqualTo(expected))

    [<Test>]
    member this.matchingInput() =
        let tokenType = LiteralTokenType(["if"; "else"]) :> TokenType
        let input = "if"
        let actual = tokenType.Matches input
        let expected = Some "if"
        Assert.That(actual, Is.EqualTo(expected))
        
    [<Test>]
    member this.severalMatching() =
        let tokenType = LiteralTokenType(["if"; "else"; "elseif"]) :> TokenType
        let input = "elseif"
        let actual = tokenType.Matches input
        let expected = Some "elseif"
        Assert.That(actual, Is.EqualTo(expected))

    [<Test>]
    member this.severalMatchingReorder() =
        let tokenType = LiteralTokenType(["if"; "elseif"; "else"]) :> TokenType
        let input = "elseif"
        let actual = tokenType.Matches input
        let expected = Some "elseif"
        Assert.That(actual, Is.EqualTo(expected))

    [<Test>]
    member this.matchesTokenOnly() =
        let tokenType = LiteralTokenType(["if"; "else"; "elseif"]) :> TokenType
        let input = "if a = b"
        let actual = tokenType.Matches input
        let expected = Some "if"
        Assert.That(actual, Is.EqualTo(expected))

[<TestFixture>]
type TestScanner () =
    let keywordTokenType = LiteralTokenType [
        "if"
        "then"
        "else"
    ]

    let operatorTokenType = LiteralTokenType [
        "<"
    ]

    let identifierTokenType =
        RegexTokenType [Regex @"^[a-zA-Z_][a-zA-Z_0-9]*"]

    let makeScanner input =
        Scanner(input, "test").AddTokenTypes([
            EOFTokenType
            ErrorTokenType
            WhiteSpaceTokenType
            NewlineTokenType
            keywordTokenType
            operatorTokenType
            identifierTokenType
        ])

    let expectToken (token: Token) ((tokenType: TokenType, value: string, line: int, column: int))=
        Assert.That(token.tokenType, Is.EqualTo(tokenType))
        Assert.That(token.value, Is.EqualTo(value))
        Assert.That(token.position.line, Is.EqualTo(line))
        Assert.That(token.position.column, Is.EqualTo(column))
        
    [<Test>]
    member this.matchesTokenTypes () =
        let scanner = makeScanner "if a < 0 then 1 else 2"
        let matched = scanner.MatchTokenType()
        match matched with
        | None -> Assert.Fail "Expected some token"
        | Some(tokenType, matched) -> 
            Assert.That(tokenType, Is.EqualTo(keywordTokenType))
            Assert.That(matched.Value, Is.EqualTo("if"))

    [<Test>]
    member this.advancesThough () =
        let scanner = makeScanner "if a < b\nthen b\nelse a"
        let expectedTokens: List<TokenType*string*int*int> = [
            (keywordTokenType :> TokenType, "if", 1, 1)
            (WhiteSpaceTokenType :> TokenType, " ", 1, 3)
            (identifierTokenType :> TokenType, "a", 1, 4)
            (WhiteSpaceTokenType :> TokenType, " ", 1, 5)
            (operatorTokenType :> TokenType, "<", 1, 6)
            (WhiteSpaceTokenType :> TokenType, " ", 1, 7)
            (identifierTokenType :> TokenType, "b", 1, 8)
            (NewlineTokenType :> TokenType, "\n", 1, 9)
            (keywordTokenType :> TokenType, "then", 2, 1)
            (WhiteSpaceTokenType :> TokenType, " ", 2, 5)
            (identifierTokenType :> TokenType, "b", 2, 6)
            (NewlineTokenType :> TokenType, "\n", 2, 7)
            (keywordTokenType :> TokenType, "else", 3, 1)
            (WhiteSpaceTokenType :> TokenType, " ", 3, 5)
            (identifierTokenType :> TokenType, "a", 3, 6)
            (EOFTokenType :> TokenType, "", 3, 7)
        ] 
        for expected in expectedTokens do
            let token = scanner.Advance ()
            expectToken token.Value expected


