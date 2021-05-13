module FrontEnd

open AST
open Scanner
open Production

// Build a front end starting from a single Production. Handles creating
// scanners, rules, etc and providing a method to read a string or a file
// handle into an AST.
type FrontEnd (program: ProgramProduction, ?toSkip: List<TokenType>) =
    let toSkip = defaultArg toSkip [BaseTokenTypes.Whitespace; BaseTokenTypes.Comment];
    let program = program :> Production
    let literals = TokenType.Literal("literals", 100, program.Literals)
    member t.ScannerFactory =
        ScannerFactory(literals :: program.Terminals, toSkip)
    member t.Parse (scanner: Scanner) =
        if program.Matches scanner.Next.Value
        then program.Consume(scanner.Next.Value, scanner)
        else
            let message = "Did not match program start" 
            TreeLeaf scanner.Next.Value |> TreeErrors.Add message
    member t.Parse (contents: string, file: string) =
        t.Parse(t.ScannerFactory.Scan(contents, file))
    member t.Parse (file: string) =
        t.Parse(t.ScannerFactory.From(file))
