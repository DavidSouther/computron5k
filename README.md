# cpsc5400 Hand Rolled parser

## Compiler

	AddFile(Path)
	AddFolder(Path, Extensions)

	Fontend
		scanner() -> ScannerFactory()
		parser() -> ParserFactory()

	PassManager
		registerASTPass();
		toIR();
		registerIRPass();

	Backend
		target() -> ArchFactory()	

	SymbolTable
		register(Symbol, ...)
		lookup(...)

## Front end

	FrontendError
		Message: String
		Token: Token(type = ErrorToken)

### Lexer

	Token 
		Type: TokenType 
		Value: String 
		Position: (file, line, column)

	TokenType
		match(input: String) -> String;

	RegexTokenType: TokenType
		pattern: String

	TokenStream
		+from(file: System.IO.Path) -> TokenStream;
		+from(source: String, file: String) -> TokenStream;
		peek(): Token;
		advance();

### Parser
	
	Parser
		AddInfixOperator (lbp, rbp, Set<string>, Option<Set<string>>)
		AddPostfixOperator (rbp, Set<string>, Option<Set<string>>)
		AddPrefixOperator (lbp, Set<string>, Option<Set<string>>)
		parse TokenStream -> Tree<Token>

## AST/IR
	Tree<'T>: 'T * List<Tree<'T>>
	
	// A depth first transformer which reuses structure as
	// much as possible when the transform function does
	// not modify the node.
	Transformer (Tree<'T> -> Tree<'T>)
		member Transform: Tree<'T> -> Tree<'T>

	// A 
	ASTPass:
		dependsOn: List<ASTPass>
		transform: Tree -> Tree 

	IRList

## Backend

	TargetPlatform:
		codegen(IRList)
