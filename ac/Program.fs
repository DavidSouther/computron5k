open System

open AST
open ac

let scan (file: string) = 
    let scanner = scanner.From file
    while not(ac.isEOF(scanner.Advance())) do
        scanner.Next.Value.ToRepl()
        |> Console.WriteLine

let parse (file: string) =
    parser.ParseFile file
    |> Tree.ToSExpression
    |> System.Console.WriteLine

[<EntryPoint>]
let main argv =
    let file = argv.[0]
    let op = if argv.Length > 1 then argv.[1] else "parse"
    match op with
    | "scan" -> scan file 
    | "parse" -> parse file
    | _ -> parse file

    0 // return an integer exit code