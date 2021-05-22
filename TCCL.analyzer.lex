%namespace ASTBuilder
%scannertype TCCLScanner
%visibility internal
%tokentype Token 

%option stack, minimize, parser, verbose, persistbuffer, noembedbuffers 

%{
//	public string yystringval;
	private StringBuilder stringval = new StringBuilder();
%}

LineTerminator  (\r|\n|\r\n)
InputCharacter  ([^\r\n])
WhiteSpace	    ({LineTerminator}|[ \t\f])

/* comments */
//CommentContent     ( [^*] | \*+ [^/*] )*
TraditionalComment   ("/*"[^*]*"*/"|"/*"("*")+"/")
EndOfLineComment    (("//")({InputCharacter})*{LineTerminator})
//DocumentationComment ("/**" {CommentContent} "*"+ "/")
Comment  ({TraditionalComment}|{EndOfLineComment}) // | {DocumentationComment})

Identifier [a-zA-Z][a-zA-Z0-9]*

DecIntegerLiteral (0|[1-9][0-9]*)

%s STRING

%%

/* keywords and special characters */
<INITIAL> {
"static"		{ return (int)Token.STATIC; }
"struct"		{ return (int)Token.STRUCT; }
"?" 			{ return (int)Token.QUESTION; }
"/" 			{ return (int)Token.RSLASH; }
"-" 			{ return (int)Token.MINUSOP; }
"null" 		{ return (int)Token.NULL; }
"int" 		{ return (int)Token.INT; }
"==" 		{ return (int)Token.OP_EQ; }
"<" 			{ return (int)Token.OP_LT; }
":" 			{ return (int)Token.COLON; }
"||" 		{ return (int)Token.OP_LOR; }
"else" 		{ return (int)Token.ELSE; }
"%" 			{ return (int)Token.PERCENT; }
"this" 		{ return (int)Token.THIS; }
"class" 		{ return (int)Token.CLASS; }
"|" 			{ return (int)Token.PIPE; }
"public" 	{ return (int)Token.PUBLIC; }
[\.] 		{ return (int)Token.PERIOD; }
"\^" 		{ return (int)Token.HAT; }
"," 			{ return (int)Token.COMMA; }
"void" 		{ return (int)Token.VOID; }
"~" 			{ return (int)Token.TILDE; }
"(" 			{ return (int)Token.LPAREN; }
")" 			{ return (int)Token.RPAREN; }
">=" 		{ return (int)Token.OP_GE; }
";" 			{ return (int)Token.SEMICOLON; }
"if" 		{ return (int)Token.IF; }
"new" 		{ return (int)Token.NEW; }
"while" 		{ return (int)Token.WHILE; }
"private" 	{ return (int)Token.PRIVATE; }
"!" 			{ return (int)Token.BANG; }
"<=" 		{ return (int)Token.OP_LE; }
"&" 			{ return (int)Token.AND; }
[\{] 		{ return (int)Token.LBRACE; }
[\}] 		{ return (int)Token.RBRACE; }
[\[] 		{ return (int)Token.LBRACKET; }
[\]] 		{ return (int)Token.RBRACKET; }
"boolean" 	{ return (int)Token.BOOLEAN; }
"instanceof" 	{ return (int)Token.INSTANCEOF; }
"*" 			{ return (int)Token.ASTERISK; }
"=" 			{ return (int)Token.EQUALS; }
"+" 			{ return (int)Token.PLUSOP; }
"return" 	{ return (int)Token.RETURN; }
">" 			{ return (int)Token.OP_GT; }
"!=" 		{ return (int)Token.OP_NE; }
"&&" 		{ return (int)Token.OP_LAND; }

}


<INITIAL> {
{Identifier}                  { yylval = new Identifier(yytext); return (int)Token.IDENTIFIER; }
 
{DecIntegerLiteral}            { yylval = new INT_CONST(yytext); return (int)Token.INT_NUMBER; }

\"                             { stringval.Length = 0; BEGIN(STRING); }

{Comment}			{ /* ignore */ }  // { Listing.get().echo(yytext()); }
  
{LineTerminator}		{ /* ignore */ } // { Listing.get().newLine(1); }
 
{WhiteSpace}                    { /* ignore */ } // { Listing.get().echo(yytext()); }
}



<STRING> {
  \"                             { BEGIN(INITIAL); 
									yylval = new STR_CONST(stringval.ToString());
                                   return (int)Token.STRING_LITERAL; }
  [^\n\r\"\\]+                   { stringval.Append(yytext); }
  \\t                            { stringval.Append('\t'); }
  \\n                            { stringval.Append('\n'); }

  \\r                            { stringval.Append('\r'); }
  \\\"                           { stringval.Append('\"'); }
  \\                             { stringval.Append('\\'); }
}



/* error fallback */
.|\n                             { Console.WriteLine("Illegal character <"+
                                                    yytext+">"); }
