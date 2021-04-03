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
		parse(tokens: TokenStream) -> ParseResult

	ParseResult
		Tree: AST

## AST/IR

	ASTNode:
		Token
		Children: List<ASTNode>

	ASTPass:
		dependsOn: List<ASTPass>
		transform(root: ASTNode) -> ASTNode

	IRList

## Backend

	TargetPlatform:
		codegen(IRList)
