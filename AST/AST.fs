module AST

type TokenType =
    { Name: string;
      Priority: int;
      ToMatch: List<string>;
      Error: bool; }

    override t.ToString () = t.Name

    static member From (name: string, priority: int, toMatch: List<string>) =
        { TokenType.Name = name; Priority = priority; ToMatch = toMatch; Error = false }
    
    static member Literal (name: string, priority: int, toMatch: List<string>) =
        let toMatch =
            toMatch
            |> List.map System.Text.RegularExpressions.Regex.Escape
            // Sort by longest first, so that longer literals match before shorter literals
            |> List.sortBy (fun s -> -s.Length)
        TokenType.From(name, priority, toMatch)

type Position =
    { File: string;
      Index: int;
      Line: int;
      Column: int; }

    override t.ToString () = $"{t.File}({t.Line},{t.Column})"

type Token =
    { Type: TokenType;
      Value: string;
      Position: Position; }

    override t.ToString () =
        if t.Type.Error
        then $"~Err:{t.Value}~"
        else t.Value

    member t.ToRepl () =
        let value = t.Value.Replace("\r", "").Replace("\n", "\\n")
        $"'{value}' ({t.Type} at {t.Position})"

type TreeData =
    { Token: Token; Data: Map<string, obj> }
    override t.ToString () = t.Token.ToString()

type Tree<'T when 'T : equality> =
    | Node of T: 'T * Children: List<Tree<'T>>

    override t.ToString () = Tree.ToSExpression t

    static member Leaf (t: 'T) = Tree.Node (t, List.Empty)

    static member ToSExpression (tree: Tree<'T>) =
        match tree with
        | Node (t, []) -> t.ToString()
        | Node (t, children) ->
            let car = t.ToString()
            let cdr =
                children
                |> List.map(Tree.ToSExpression)
                |> String.concat(" ")
            $"({car} {cdr})"

let TreeNode (token: Token) (children: List<Tree<TreeData>>): Tree<TreeData> =
    Tree.Node({ TreeData.Token = token; Data = Map.empty }, children)
let TreeLeaf (token: Token): Tree<TreeData> = TreeNode token []

type Transformer<'T when 'T: equality>
    ( transform: Tree<'T> -> Tree<'T>,
      ?inAction0: Tree<'T> -> Tree<'T>,
      ?outAction0: Tree<'T> -> Tree<'T>) =
    let inAction = defaultArg inAction0 id
    let outAction = defaultArg outAction0 id
    member this.Transform (tree: Tree<'T>) =
        let tree = inAction tree
        let tree =
            match tree with
            | Node (t, c) ->
                let c2 = c |> List.map(fun t -> this.Transform(t))
                let sameChildren = List.forall2 LanguagePrimitives.PhysicalEquality c c2
                if sameChildren then transform(tree) else transform(Node(t, c2))
        outAction tree

module TreeErrors =
    let Get (tree: Tree<TreeData>): List<string> =
        let (Node(d, _)) = tree
        if d.Data.ContainsKey("errors")
        then d.Data.["errors"] :?> List<string>
        else []

    let Add (tree: Tree<TreeData>) (error: string): Tree<TreeData> =
        let (Node(d, c)) = tree
        let errors: List<string> = Get(tree) @ [error]
        Tree.Node({d with Data = d.Data.Add("errors", errors)}, c)

    let rec List (tree: Tree<TreeData>): List<string> =
        let (Node(t, c)) = tree
        let errors =
            c
            |> Microsoft.FSharp.Collections.List.map List
            |> Microsoft.FSharp.Collections.List.collect id
        let errors =
            if t.Token.Type.Error
            then t.Token.ToString() :: errors
            else errors
        match Get tree with
        | [] -> errors
        | errs ->
            let errs = errs |> String.concat "\n\t"
            $"Err: {t.Token.Value}@{t.Token.Position} ::\n\t{errs}" :: errors

