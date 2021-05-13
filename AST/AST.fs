module AST

type TokenType =
    { Name: string;
      Priority: int;
      ToMatch: List<string>;
      Error: bool; }

    override t.ToString () = t.Name

    static member From (name: string, priority: int, toMatch: List<string>) =
        { TokenType.Name = name;
          Priority = priority;
          ToMatch = toMatch;
          Error = false }
    
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
    | Empty

    override t.ToString () = Tree.ToSExpression t

    static member AppendChild (child: Tree<'T>) (parent: Tree<'T>): Tree<'T> =
        match parent with
        | Node(d, c) -> Tree.Node (d, c @ [child])
        | Empty -> child

    static member Leaf (t: 'T) = Tree.Node (t, List.Empty)

    static member BinOp (tree: Tree<'T>) =
        match tree with
        | Node(_, c) ->
            match c with
            | (Node(lhs, _)) :: (Node(rhs, _)) :: _ -> Some (lhs, rhs)
            | _ :: _ -> None
            | [] -> None
        | Empty -> None

    static member ToSExpression (tree: Tree<'T>) =
        match tree with
        | Empty -> ""
        | Node (t, []) -> t.ToString()
        | Node (t, children) ->
            let car = t.ToString()
            let cdr =
                children
                |> List.map(Tree.ToSExpression)
                |> String.concat(" ")
            $"({car} {cdr})"

    static member AsAscii (tree: Tree<'T>) =
        let rec asAscii (tree: Tree<'T>, prefix: string) =
            let rec listPart (list: List<Tree<'T>>) =
                match list with
                | [] -> ""
                | c :: [] -> prefix + "└─ " + asAscii(c, prefix + "  ")
                | c :: rest -> prefix + "├─ " + asAscii(c, prefix + "  ") + listPart(rest)
            match tree with
            | Empty -> ""
            | Node (d, c) -> d.ToString() + listPart c

        asAscii(tree, "")

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
        match tree with
        | Node(t, c) ->
            let c2 = c |> List.map this.Transform
            let sameChildren = List.forall2 LanguagePrimitives.PhysicalEquality c c2
            let tree = if sameChildren then transform(tree) else transform(Node(t, c2))
            outAction tree
        | Empty -> Empty

module TreeErrors =
    let Get (tree: Tree<TreeData>): List<string> =
        match tree with
        | Node(d, _) ->
            if d.Data.ContainsKey("errors")
            then d.Data.["errors"] :?> List<string>
            else []
        | Empty -> []

    let Add (error: string) (tree: Tree<TreeData>): Tree<TreeData> =
        match tree with
        | Node(d, c) ->
            let errors: List<string> = Get(tree) @ [error]
            Tree.Node({d with Data = d.Data.Add("errors", errors)}, c)
        | Empty -> Empty

    let rec List (tree: Tree<TreeData>): List<string> =
        match tree with
        | Node(t, c) ->
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
        | Empty -> [""]
