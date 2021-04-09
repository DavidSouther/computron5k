module Parser

open AST

open Scanner

type InfixOperatorMap = Map<string, int * int * string>
type PrefixOperatorMap = Map<string, int * string>
type PostfixOperatorMap = Map<string, int * string>
type Operators = InfixOperatorMap * PrefixOperatorMap * PostfixOperatorMap
type Identifiers = TokenType
type Values = TokenType
type Whitespace = TokenType
type Comments = TokenType
type Keywords = Set<string>

type Parser (scanner: Scanner) =
    class end

type ParserFactory = Keywords * Operators * Identifiers * Values * Comments * Whitespace -> Parser
    
