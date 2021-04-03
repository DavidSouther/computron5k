module FrontEnd.Test

open NUnit.Framework
open FrontEnd

[<TestFixture>]
type TestLiteralToken () =
    [<Test>]
    member this.literalTokenEmptyInput() =
        let tokenType = LiteralTokenType(Set.empty) :> TokenType
        let input = ""
        let actual = tokenType.matches input
        let expected = None 
        Assert.That(actual, Is.EqualTo(expected))

    [<Test>]
    member this.literalTokenMatchingInput() =
        let tokenType = LiteralTokenType(Set.empty.Add("if").Add("else")) :> TokenType
        let input = "if"
        let actual = tokenType.matches input
        let expected = Some "if"
        Assert.That(actual, Is.EqualTo(expected))
        
    [<Test>]
    member this.literalTokenSeveralMatching() =
        let tokenType = LiteralTokenType(Set.empty.Add("if").Add("else").Add("elseif")) :> TokenType
        let input = "elseif"
        let actual = tokenType.matches input
        let expected = Some "elseif"
        Assert.That(actual, Is.EqualTo(expected))
