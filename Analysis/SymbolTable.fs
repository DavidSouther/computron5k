namespace Analysis

open AST

type Symbol =
    { Name: string;
      Declared: Position;
      Initialized: Option<Position>;
      Data: Map<string, obj>;
      }

type Scope =
    abstract Parent: Option<Scope>
    abstract New: unit -> Scope
    abstract Set: string * Symbol -> Scope
    abstract Lookup: string -> Option<Symbol>
    abstract Declare: string * Position * Option<Map<string, obj>> -> Result<Scope, string>
    abstract Initialize: string * Position -> Result<Scope, string>

type SymbolTable (?parent: Scope, ?symbols0: Map<string, Symbol>) =
    static member Empty: Scope = SymbolTable() :> Scope
    member _.symbols: Map<string, Symbol> = defaultArg symbols0 Map.empty 
    interface Scope with
        member _.Parent = parent
        member t.New () = SymbolTable t :> Scope
        member t.Lookup name =
            if t.symbols.ContainsKey name
            then Some t.symbols.[name]
            else
                match (t :> Scope).Parent with
                | Some p -> p.Lookup name
                | None -> None
        member t.Set (name, symbol) =
            let updatedMap = t.symbols.Add(name, symbol)
            match (t :> Scope).Parent with
            | Some p -> SymbolTable(p, updatedMap)
            | None -> SymbolTable(symbols0=updatedMap)
            :> Scope
        member t.Declare (name, position, data) =
            if t.symbols.ContainsKey name
            then
                Error $"Cannot redeclare symbol {name}"
            else
                let symbol =
                    { Symbol.Name = name;
                      Declared = position;
                      Initialized = None;
                      Data = if data.IsSome then data.Value else Map.empty; }
                (t :> Scope).Set(name, symbol) |> Ok
        member t.Initialize (name, position) =
            if not(t.symbols.ContainsKey(name))
            then
                Error $"Symbol {name} is not declared"
            else
                let symbol = t.symbols.[name]
                if symbol.Initialized.IsSome
                then Ok(t :> Scope) // Already initialized, no work to do
                else
                    let symbol = { t.symbols.[name] with Initialized = Some position }
                    (t :> Scope).Set(name, symbol) |> Ok

