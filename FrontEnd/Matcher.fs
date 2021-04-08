module Matcher

type State<'T> =
    abstract terminals: Set<'T>
    abstract edges: Map<CRange, DFA<'T>>

type Matcher<'T> () =
    let mutable map: Map<string, 'T> = Map.empty
    member Register (regex: string, forT: 'T) =
        ()
