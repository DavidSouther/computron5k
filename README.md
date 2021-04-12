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

### Scanner

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

### Parser
	
	Parser
		Operators: token, bindingPower, leftAction, rightAction
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
