# cpsc5400 Hand Rolled parser

## Compiler

    AddFile(Path)
    AddFolder(Path, Extensions)

    Fontend
        scanner() -> ScannerFactory()
        parser() -> ParserFactory()

    PassManager
        registerASTPass();
        registerIRPass();

    Backend
        target() -> ArchFactory()    

    SymbolTable
        register(Symbol, ...)
        lookup(...)

    SyntaxElement
        Kind: string
        
    Symbol
        Name
        Declared
        Initialized
        Value

    Value
        Type
        Value

### Front end

    FrontendError
        Message: String
        Token: Token(type = ErrorToken)

#### Scanner

    RegexStr:
        a literal
        a? zero or one
        a* zero or more
        a+ one or more
        a|b Alternate
        [a-z] range
        (abc) grouping

    Scanner
        Register (RegexStr, TokenType)

    Token 
        Type: TokenType 
        Value: String 
        Position: (file, line, column)

    RegexTokenType: TokenType
        pattern: String

    TokenStream
        +from(file: System.IO.Path) -> TokenStream;
        +from(source: String, file: String) -> TokenStream;
        peek(): Token;
        advance();

#### Parser
    
    ParserFactory
        Operators: token, bindingPower, leftAction, rightAction
        Statements: List<Value|Kind|BP>
        parse TokenStream -> Tree<Token>

### AST/IR
    Tree<'T>: 'T * List<Tree<'T>>
    
    // A depth first transformer which reuses structure as
    // much as possible when the transform function does
    // not modify the node.
    Transformer (Tree<'T> -> Tree<'T>)
        member Transform: Tree<'T> -> Tree<'T>

    SyntaxElement
        static productions: List<Production>
        productions[] // the parse tree
        children // the AST
    Production
        Matcher // Something that can be reduced to a graph edge
        SyntaxElement
    OptionalProduction: Production (Production)
    OneofProduction: Production (List<Production>)
    RepeatedProduction: Production (Production)
    ExpressionProduction: Production (Set<Operator>)
    Operator
        symbol: string
        precedence: number > 0
        fix: Prefix|Infix|Postfix
        arity: Unary|Binary
    InOperator
        fix: Infix
        arity: Binary
    PrefixOperator
        fix: Prefix
        arity: Unary
    PostfixOperator
        fix: Postfix
        arity: Unary

    Tree<T> T, Children
    AST = Tree<SyntaxElement>
    IR = Tree<IRElement>
    IRList = List<IRElement>

    // A 
    ASTPass:
        dependsOn: List<ASTPass>
        transform: Tree -> Tree 

### Backend

    TargetPlatform:
        codegen(IRList)

## Languages

### ac

    Program > Thing*
    Thing > Declaration
          | Statement
    Declaration > "i" id
                | "f" id
    Statement > id "=" Expression
              | "p" id
    Expression: Binop(+- 10 left)
            id | value
    id = [a-eghj-oq-z]
    value = (\d+|0)(.\d+)?

### Lisp

    Program > SExpression+
    SExpression > Atom 
                | "(" SExpression* ")"
    Atom > id | value
    id = [a-zA-Z'][^()"]*
    value = (\d+|0)(.\d+)?
          = "[^"]*"

### Tiger 

    Program > Declations
            | Expression

    Declarations > Declaration*
    Declaration > "type" id "=" Type
                | "class" id ("extends" type-id)? "{" ClassFields "}"
                | VariableDeclaration
                | "function" id "(" TypeFields ")" TypeAnnotation? = Expression
                | "primitive" id "(" TypeFields ")" TypeAnnotation?
                | "import" string

    TypeFields > id ":" type-id ("," id TypeAnnotation)*
    ClassFields > ClassField*
    ClassField > VariableDeclaration
               | "method" id "(" TypeFields ") TypeAnnotation?
    VariableDeclaration > "var" id TypeAnnotation? ":=" Expression
    TypeAnnotation > ":" type-id
    Type > type-id
         | "{" TypeFields "}"
     | "array" "of" type-id
     | "class" ("extends" type-id)? "{" ClassFields "}"

    Expression > "nil"
               | integer
               | string
               | type-id "[" Expression "]" "of" Expression
               | type-id "{" ( id "=" Expression ("," id "=" Expression)*)? "}"
               | "new" type-id
               | LValue
               | id "(" (Expression ("," Expression)*)? ")"
               | LValue "." id "(" (Expression ("," Expression)*)? ")"
               | "-" Expression
               | Expression Operation Expression
               | LValue := Expression
               | "if" Expression "then" Expression ("else" Expression)?
               | "while" Expression "do" Expression
               | "for" id ":=" Expression "to" Expression "do" Expression
               | "break"
               | "let" Declarations "in" Expression "end"

    LValue > id
           | LValue "." id
           | LValue "[" Expression "]"

    Operation: * / Left 50
               + - Left 40
               - Unary 40
               >= <= = <> < > Left 30
               & Left 20
               | Left 10
           := right 5

### Jack

    Program > Class*
    Class > "class" id "{" ClassMemberDeclaration* "}"
    ClassMemberDeclaration > ("static"|"field") Type id ("," id)* ';'
                           | ("constructor"|"function"|"method") ("void"|Type) id "(" ParameterList? ")" SubroutineBody
    ParameterList > Type id ("," Type id)?
    SubroutineBody > "{" Declaration* Statement Statement* "}"
    Declaration > "var" Type id ("," Type id)* ";"
    Statement > "let" id ("[" Expression "]")? "=" Expression ";"
              | "if" "(" Expression ")" "{" Statement* "}" ("else" "{" Statement* "}")?
              | "while" "(" Expression ")" "{" Statement* "}"
              | "return" Expression? ";"
              | Expression ";"

    Operation: * / Left 50
               + - Left 40
               - Unary 40
               ~ Unary 35
               = < > Left 30
               & Left 20
               | Left 10
               . Right 7
               , Right 5
               { Right 0 Close } Keep
               ( Right 0 Close ) Keep
               ( Unary 0 Close ) Drop

### MinCaml

### MiniDot

    Graph > "graph" GraphLevel
    GraphLevel > "{" GraphMember* "}"
    GraphMember > Node
                | Edge
    Node > id AttributeList? NodeChildren
    NodeChildren > GraphLevel
                 | ";" 
    Edge > id "->" id AttributeList? ";"
    AttributeList > "[" Attribute+ "]"
    Attribute > name "=" value ";"

### TCCL

    
