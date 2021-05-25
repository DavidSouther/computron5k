module Production.Test

open FsUnit
open NUnit.Framework

open AST
open Scanner
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
    let makeScanner (input: string, production: Production) =
        let literals = match production.Literals with
                       | [] -> []
                       | _ -> [TokenType.Literal("literals", 0, production.Literals)]
        ScannerFactory(literals @ production.Terminals).Scan(input, "test")

    [<Test>]
    member _.test_ConstProduction () =
        let aProduction = ConstProduction "a" :> Production
        let input = makeScanner("a", aProduction)
        let matches = aProduction.Matches(input.Next.Value)
        matches |> should equal true
        let tree = aProduction.Consume (input.Next.Value, input)
        input.Next.Value.Type |> should equal BaseTokenTypes.EOF

    [<Test>]
    member _.test_SimpleProduction () =
        let sProduction = SimpleProduction simpleType :> Production
        let input = makeScanner("a", sProduction)
        let t = input.Next.Value
        let matches = sProduction.Matches t
        matches |> should equal true
        let tree = sProduction.Consume (t, input)
        input.Next.Value.Type |> should equal BaseTokenTypes.EOF


    [<Test>]
    member _.test_OptionalProduction () =
        let aProduction = ConstProduction "a" :> Production
        let oProduction = OptionalProduction aProduction :> Production
        let input = makeScanner("ab", aProduction)
        let t = input.Next.Value
        oProduction.Matches t |> should equal true
        oProduction.Consume (t, input) |> ignore

        let t = input.Next.Value
        t.Value |> should equal "b"
        oProduction.Matches t |> should equal true

        let tree = oProduction.Consume (t, input)
        tree |> should equal Tree<TreeData>.Empty
        input.Next.Value.Value |> should equal "b"

    [<Test>]
    member _.test_RepeatedProduction () =
        let aProduction = ConstProduction "a" :> Production
        let rProduction = RepeatedProduction("test", aProduction) :> Production
        let input = makeScanner("aab", rProduction)

        let t = input.Next.Value
        rProduction.Matches t |> should equal true
        let tree = rProduction.Consume (t, input)
        tree |> Tree.ToSExpression |> should equal "(test a a)"

        let t = input.Next.Value
        rProduction.Matches t |> should equal false 
        t.Value |> should equal "b"

