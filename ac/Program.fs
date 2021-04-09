open System

open Scanner

let operators = TokenType.Literal("Operators", 20, ["+"; "-"; "="; "i"; "f"; "p"])
let identifiers = TokenType.From("Identifiers", 20, ["[a-eghj-oq-z]"])
let whitespace = BaseTokenTypes.Whitespace

let scanner = ScannerFactory [operators; identifiers; BaseTokenTypes.Value; BaseTokenTypes.Whitespace; BaseTokenTypes.EOF;]

let isEOF (next: Option<Token>) =
    next.IsSome && next.Value.Type.Name = BaseTokenTypes.EOF.Name

[<EntryPoint>]
let main argv =
    let file = argv.[0]
    let scanner = scanner.From file
    while not(isEOF(scanner.Advance())) do
        Console.WriteLine scanner.Next
    0 // return an integer exit code