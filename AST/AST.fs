module AST

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

type Transformer<'T when 'T: equality> (transform: Tree<'T> -> Tree<'T>) =
    member this.Transform (tree: Tree<'T>) =
        match tree with
        | Node (t, c) ->
            let c2 = c |> List.map(fun t -> this.Transform(t))
            let sameChildren = List.forall2 LanguagePrimitives.PhysicalEquality c c2
            if sameChildren then transform(tree) else transform(Node(t, c2))

