---
marp: true
---

# TypeChecking in F#
## David Souther

---

# Functional Programming

In a programming language:
* Functions are stand-alone, first class concepts 
  * `const square = (n: number) => n * n;`
  * `set.iter().filter(|i| i >= 32);`
* Functions are not procedures
  * A function takes data in and returns data out
  * A procedure takes data in and gives nothing back
* Functions are sometimes closures
  * A closure is a function which captures some data
  * The data will be used when the function is later executed

---

# Functional Programming

In mathematics:
* A function `f` is a mapping of elements `x` from a set `X` to elements `y` in a set `Y`
* Functions can be composed
    * `f(x) -> y` and `g(y) -> z` can be combined as `h(x) -> z = g(f(x)) -> z`
* Functions can take multiple values as tuples
    * `(3, "A")` is a valid element of some set
* Functions can take elements that are themselves functions
* Functions can return elements that are themselves functions
* This branch of mathematics is Category Theory

---

# Functional Programming

is an approach to software engineering which leverages the rich mathematical understanding of category theory in order to constrain complexity while improving quality of the solution.

---

# Functional Programming

Sounds fancy, but with the right tools (Rust or F#) can feel intuitive.

---

# Immutable Data Structures

* Once created, never changes
* Mutation happens through new creation
* Spooky action at a distance can never happen
    * This is why functional adherents swear by immutable data structures

---

# Data Structure Review: List

* A List is a linear, ordered sequence of data.
* ArrayList implements a List using a contiguous slice of memory.
* LinkedList implements a List using a data cell with the data and a pointer to the next data item.

|      | ArrayList         | LinkedList                          |
| ---- | ----------------- | ----------------------------------- |
| Pros | O(1) index access | O(1) Iteration                      |
|      |                   | O(1) Insertion (at iteration point) |
| Cons | O(N) insertion    | O(N) random access                  |

---

# ArrayList

    ╔═══════╤════════╤═══════╗
    ║ First │ Second │ Third ║
    ╚═══════╧════════╧═══════╝

# LinkedList

    ╔═══════╤══╗  ╔════════╤══╗  ╔═══════╤═╗
    ║ First │ ─╫─>║ Second │ ─╫─>║ Third │ ║
    ╚═══════╧══╝  ╚════════╧══╝  ╚═══════╧═╝

---

# Data Structure Review: Tree

* A tree is a nested data structure
* Tree Nodes explicitly have ordered children
* Tree Leaves are nodes with zero children
* Data may be stored only at the leaves, or in all nodes

| BinaryTree           | ListTree         |
| -------------------- | ---------------- |
| Left and Right child | List of children |
| 0, 1, or 2 children  | n >= 0 children  |

---

# Binary and List Trees

     ╔══════════════╗              ╔════════╤══╗  ╔═══╤══╗  ╔═══╤══╗  ╔═══╤═╗
     ║    Parent    ║              ║ Parent │ ─╫─>║   │ ─╫─>║   │ ─╫─>║   │ ║
     ╟──────┬───────╢              ╚════════╧══╝  ╚═╪═╧══╝  ╚═╪═╧══╝  ╚═╪═╧═╝
     ║      │       ║                              ┌┘         │         └┐
     ╚══╪═══╧════╪══╝                              │          │          | 
        │        │                                 V          V          V
        V        V                             ╔═══════╤═╗ ╔════════╤═╗ ╔═══════╤═╗
    ╔══════╗   ╔═══════╗                       ║ First │ ║ ║ Second │ ║ ║ Third │ ║
    ║ Left ║   ║ Right ║                       ╚═══════╧═╝ ╚════════╧═╝ ╚═══════╧═╝
    ╟───┬──╢   ╟───┬───╢
    ╚═══╧══╝   ╚═══╧═══╝
---

# Linked Trees are Nested Linked Lists

    ╔═══════╤══╗  ╔════════╤══╗  ╔═══════╤═╗
    ║ First │ ─╫─>║ Second │ ─╫─>║ Third │ ║
    ╚═══════╧══╝  ╚════════╧══╝  ╚═══════╧═╝

    ╔════════╤══╗  ╔═══╤══╗  ╔═══╤══╗  ╔═══╤═╗
    ║ Parent │ ─╫─>║   │ ─╫─>║   │ ─╫─>║   │ ║
    ╚════════╧══╝  ╚═╪═╧══╝  ╚═╪═╧══╝  ╚═╪═╧═╝
                    ┌┘         │         └┐
                    │          │          | 
                    V          V          V
                ╔═══════╤═╗ ╔════════╤═╗ ╔═══════╤═╗
                ║ First │ ║ ║ Second │ ║ ║ Third │ ║
                ╚═══════╧═╝ ╚════════╧═╝ ╚═══════╧═╝

---

# Parts of a linked list

When you have a node in a linked list, it has two parts
* `Head` - The value in the current node
* `Tail` - The next node in the list

# Making a linked list

```fsharp
let prepend list value =
    { Head: value; Tail: list }

let append list value =
    if list = null
    then { Head: value; Tail: null }
    else { Head: list.Head; Tail: prepend list.Tail value }
```
 
---

# Transforming a linked list

Visit every item in the list, generating a new value, and returning a list with those values

```fsharp
let map fn list =
    if list = null
    then null
    else {
        Head: fn list.Head
        Tail: map fn list.Tail
    }
```

---

# Modifying a linked list

Return a new list with only those values that match a predicate function

```fsharp
let filter predicate list =
    if list = null
    then null
    else
        if predicate list
        then { Head: list.Head; Tail: filter predicate list.Tail }
        else filter fn list.Tail

let evens = filter (fun x -> x % 2 = 0) [0..7]

// New list [0; 2; 4; 6]
```

---

# F# Language Features

* Discriminated Union
  * Describes the shape of data
* Match Statements
  * Control flow on the shape of data

---

# Discriminated Union

```fsharp
type Tree<'T when 'T : equality> =
    | Node of T: 'T * Children: List<Tree<'T>>
    | Empty
```

* A tuple with some data and a list of children.
* Empty serves as a sigil for some algorithms
* `'T` must be equality (simple object comparison) to support structural checking.
    * Simple object comparison is fine, because data is immutable!

---

# Pattern Matching

```fsharp
match tree with
| Empty -> // There is no data in this tree at all!
| Node(d, []) -> // d is a leaf node, because there are no children
| Node(d, c) -> // d is the data, and it has 1 or more children
```

---

# Tree Transformation

```fsharp
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
```

---

# Functional Type Checking on an Immutable AST

* Immutable Symbol Table
    * Methods on immutable classes return new (with structural sharing) instances
    * "Reverse" tree (tracks parents, not children)

```fsharp
type Attributes = Map<string, obj>;

type Symbol =
    { Name: string;
      Attributes: Attributes; }

type Scope =
    abstract Parent: Option<Scope>
    abstract New: unit -> Scope
    abstract Declare: string * Attributes -> Result<Scope, string>
    abstract Lookup: string -> Option<Symbol>
```

---

# Typechecking Passes: Scope

* Create a new Scope for each block level
* Attach scope to the parent node
* Create a closure with access to the appropriate scope in later passes

