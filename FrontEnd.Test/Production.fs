module Production.Test

open FsUnit
open NUnit.Framework

open AST
open Production

let simpleType = {
    TokenType.Name = "simple";
    Priority = 0;
    ToMatch = ["a"];
    Error = false;}

let simplePosition = {
    Position.Line = 1;
    Column = 1;
    File = "simple";
    Index = 1;}

[<TestFixture>]
type ProductionTest () =
    let simpleToken (c: char) =
        { Token.Value = $"{c}";
            Position = simplePosition;
            Type =  simpleType }

    [<Test>]
    member _.test_ConstProduction () =
        let aProduction = ConstProduction "a" :> Production
        let input = "a" |> Seq.map simpleToken
        let matches = aProduction.Matches(Seq.head(input))
        matches |> should equal true
        let (input, tree) = aProduction.Consume (Seq.head(input), input)
        input |>  should equal Seq.empty

    [<Test>]
    member _.test_SimpleProduction () =
        let sProduction = SimpleProduction simpleType :> Production
        let input = "a" |> Seq.map simpleToken
        let t = Seq.head(input)
        let matches = sProduction.Matches t
        matches |> should equal true
        let (input, tree) = sProduction.Consume (t, input)
        input |> should equal Seq.empty


    [<Test>]
    member _.test_OptionalProduction () =
        let aProduction = ConstProduction "a" :> Production
        let oProduction = OptionalProduction aProduction :> Production
        let input = "ab" |> Seq.map simpleToken

        let t = Seq.head input
        oProduction.Matches t |> should equal true
        let (input, _) = oProduction.Consume (t, input)

        let t = Seq.head input
        t.Value |> should equal "b"
        oProduction.Matches t |> should equal true

        let (input, tree) = oProduction.Consume (t, input)
        tree |> should equal Tree<TreeData>.Empty
        input |> Seq.map(fun t -> t.Value) |> should equal ["b"]

    [<Test>]
    member _.test_RepeatedProduction () =
        let aProduction = ConstProduction "a" :> Production
        let rProduction = RepeatedProduction aProduction :> Production
        let input = "aab" |> Seq.map simpleToken

        let t = Seq.head input
        rProduction.Matches t |> should equal true
        let (input, tree) = rProduction.Consume (t, input)
        tree |> Tree.ToSExpression |> should equal "((a a))"

        let t = Seq.head input
        rProduction.Matches t |> should equal false 
        input |> Seq.map(fun t -> t.Value) |> should equal ["b"]

