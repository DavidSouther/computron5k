# cpsc5400 Hand Rolled parser

## Compiler
    Stream<T> = (Seq<T>)
        peek(): T  // IEnumerator.Current
        next(): Stream<T> // IEnumerator.MoveNext()

    Compiler Frontend * PassManager * Backend * SymbolTable
		AddFile(Path)
		AddFolder(Path, Extensions)

    Fontend ScannerFactory * ParserFactory
        scanner() -> ScannerFactory()
        parser() -> ParserFactory()

    ScannerFactory
        scanner() -> Scanner
    ParserFactory
        parser() -> Parser
    Scanner
		// Iterate the tokens in the stream
        scan(contents, filename) -> Stream<Token> 
    Parser
		// In-order bottom-up iteration of the syntax tree
        parse(contents, filename) -> Stream<SyntaxElement> 

    PassManager
        registerASTPass ASTTransformer
        astToIr ASTToIRTransformer
        registerIRPass IRTransformer

    ASTTransformer
        // Transforms an AST+scope into AST+scope with type checking
        // information and errors

    IRTranformer
        // Transforms an IR Tree into an IR Tree with control flow analysis,
        // optimizations, and register selection based on Backend's register
        // tables.

    ASTToIRTransformer
        // Transforms an AST into an IR tree.

    Backend
        // Instruction selection and call frames
        target() -> ArchFactory()    

    SymbolTable
        register(Symbol, ...)
        lookup(...)

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

    Literal
    Identifier
    Value
    Comment
    Whitespace

#### Parser

    Matcher:
        matches(token: Token)
    
    ParserFactory
        Statements: List<Production>
        parse TokenStream -> Sequence<AST>

    SyntaxElement: TreeData 
        tokens: List<Token|SyntaxElement> // the complete parse tree
        children: List<SyntaxElement> // the AST children

    Production
        Matches: Token -> bool // Functional matching
        Consume: (Token, Seq<Token>) -> (Seq<Token>, Tree<TreeData>)

    ConstProduction: Production
        Value: string

    SimpleProduction: Production
        Matcher: TokenType // Regex matching

    Statement: Production (List<Production>)
        A statement takes a list of strings, token types, & productions, and
        creates a syntax element with children from the Productions. string
        and token values are treated as literal expected tokens, Productions
        are treated as expected next parse types. Literal tokens are stored
        in the parse tree, only SyntaxElements are included in the children.

    Optional: Production (Production)
        A production that may be matched, but will have an empty Follow if the
        token does not meet the First set.

    Oneof: Production (List<Production>)
        A production that will match the first Production in the list, or will
        error if no productions match. By implication, the first Optional in a
        Oneof will match. Therefore, an Optional should only be in the last
        position of a Oneof.

    Repeated: Production (Production, atLeastOnce: boolean = true)
        The production can me matched any number of times. If atLeastOnce is
        true, one instance must be matched. Otherwise, it is allowed to match
        zero times.

    Expression: Production (Set<TokenType>, Set<Operator>)
        Expression consumes terminals and operators to build a
        complete expression. The expression hierarchy is determined by the set
        of operators.
    Operator
        symbol: string
        precedence: number > 0
        fix: Prefix|Infix|Postfix
        arity: Unary|Binary
    InfixOperator
        fix: Infix
        arity: Binary
    PrefixOperator
        fix: Prefix
        arity: Unary
    PostfixOperator
        fix: Postfix
        arity: Unary

### AST/IR
    Tree<'T>: 'T * List<Tree<'T>>
    
    // A depth first transformer which reuses structure as
    // much as possible when the transform function does
    // not modify the node.
    Transformer (Tree<'T> -> Tree<'T>)
        member Transform: Tree<'T> -> Tree<'T>

    IRElement
        Optional<SyntaxElement>

    Value
        Label|Mem|Temp|Const
    Operation
        Operator, Expression, Expression
    Call
        label: Expression
        arguments: List<Value>
    ExpressionSequence
        sequence: List<Statement>
        result: Expression

    Expression: Value | Operation | Call | ExpressionSequence

    Move
        target: Value.Mem | Value.Temp 
        source: Expression
    Label
        name: string
    Jump
        target: Value.Label|Expression
        locations: List<Label>
    ConditionalJump
        condition: Comparator
        a: Expression
        b: Expression
        trueTarget: Value.Label
        falseTarget: Value.Label
    Sequence
        head: Statment
        tail: Statment
     
    Statement: Expression | Move | Label | Jump | ConditionalJump | Sequence

    Operator: PLUS | MINUS | MULTIPLY | DIVIDE | AND | OR | LSHIFT | RSHIFT | ARSHIFT | XOR
    Comparator: EQUAL | NOT_EQUAL | LESS_THAN | GREATER_THAN | LESS_THAN_EQUAL | GREATER_THAN_EQUAL | 
				UNSIGNED_LESS_THAN | UNSIGNED_GREATER_THAN | UNSIGNED_LESS_THAN_EQUAL | UNSIGNED_GREATER_THAN_EQUAL

    Tree<T> T, Children
    AST = Tree<SyntaxElement>
    IR = Tree<IRElement>

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

    ///

    Identifier = TokenType "[a-eghj-oq-z]"
    Value = TokenType "(\d+|0)(.\d+)?"
    Expression = Operator(
        [Identifier; Value]
		[
			InfixOperator("+", 10),
			InfixOperator("-", 10),
		]
	)
    Statement = Oneof([
        Production.Statement [Identifier; "="; Expression]
        Production.Statement ["p"; Identifier]
    ])
    Declaration = OneOf([
        Production.Statement ["i"; Identifier]
        Production.Statement ["f"; Identifier]
    ])

    Program = Repeated(OneOf([Statement; Declaration])

### Lisp

    Program > SExpression+
    SExpression > Atom 
                | "(" SExpression* ")"
    Atom > id | value
    id = [a-zA-Z'][^()"]*
    value = (\d+|0)(.\d+)?
          = "[^"]*"

    id = [a-zA-Z'][^()"]*
    value = (\d+|0)(.\d+)?
          = "[^"]*"

    /// 

    Identifier = """[a-zA-Z'][^()"]*"""
    Value = "(\d+|0)(.\d+)"|""""[^"]*""""
    Atom = OneOf [
        Identifier
        Value
    ]
    SExpression = OneOf [
        Atom
        Statement ["("; SExpression ;")"
	]
    Program = OneOrMore [SExpression]

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

    
