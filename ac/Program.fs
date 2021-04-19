open System

open AST
open Analysis
open ac

let scan (file: string) = 
    let scanner = scanner.From file
    while not(ac.isEOF(scanner.Advance())) do
        scanner.Next.Value.ToRepl()
        |> Console.WriteLine
    ()

let parse (file: string) =
    parser.ParseFile file
    |> Tree.ToSExpression
    |> System.Console.WriteLine

let rec Output (tree: Tree<TreeData>) : List<string> =
    let (Node(d, c)) = tree
    let output =
        c
        |> Microsoft.FSharp.Collections.List.map Output
        |> Microsoft.FSharp.Collections.List.collect id
    if d.Data.ContainsKey "out"
    then output @ [d.Data.["out"] :?> string]
    else output

[<EntryPoint>]
let main argv =
    let file = argv.[0]
    let op = if argv.Length > 1 then argv.[1] else "interpret"
    match op with
    | "scan" ->
        scan file 
        0
    | "parse" ->
        parse file
        0
    | "interpret" ->
        let ast = parser.ParseFile file

        let analyzer = PassManager.Empty.AddPass(Passes.ScopePass).AddPass(ac.DeclPass).AddPass(ac.InterpretPass)
        let ast = analyzer.Run ast

        let errs = TreeErrors.List ast
        let vals = Output ast
        if not errs.IsEmpty
        then
            errs
            |> String.concat "\n"
            |> System.Console.WriteLine
            System.Console.WriteLine "\nErrors were encountered; output follows"
        else ()
        vals |> String.concat "\n" |> System.Console.WriteLine
        0
    | _ ->
        System.Console.Error.WriteLine $"Unrecognized command {op}"
        1
