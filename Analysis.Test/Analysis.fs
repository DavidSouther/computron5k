module Analysis.Test

open FsUnit
open NUnit.Framework

open Analysis

[<TestFixture>]
type TestSymbolTable () =

    [<Test>]
    member _.Empty () =
        let scope = SymbolTable.Empty
        scope.Lookup "a" |> should equal None

    member _.Declare () = ()
