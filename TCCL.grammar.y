%namespace ASTBuilder
%partial
%parsertype TCCLParser
%visibility internal
%tokentype Token
%YYSTYPE AbstractNode


%start CompilationUnit

%token AND ASTERISK BANG BOOLEAN CLASS
%token COLON COMMA ELSE ASSIGN HAT
%token IDENTIFIER IF INSTANCEOF INT INT_NUMBER
%token LBRACE LBRACKET LPAREN MINUSOP
%token NEW NULL OP_EQ OP_GE OP_GT
%token OP_LAND OP_LE OP_LOR OP_LT OP_NE
%token PERCENT PERIOD PIPE PLUSOP PRIVATE
%token PUBLIC QUESTION RBRACE RBRACKET RETURN
%token RPAREN RSLASH SEMICOLON STATIC STRING_LITERAL
%token SUPER STRUCT THIS TILDE VOID WHILE


%right ASSIGN
%left  OP_LOR
%left  OP_LAND
%left  PIPE
%left  HAT
%left  AND
%left  OP_EQ, OP_NE
%left  OP_GT, OP_LT, OP_LE, OP_GE
%left  PLUSOP, MINUSOP
%left  ASTERISK, RSLASH, PERCENT
%left  UNARY 

%%

CompilationUnit     :   ClassDeclaration                                            { $$ = new CompilationUnit($1);} 
                    ;
ClassDeclaration    :   Modifiers CLASS IDENTIFIER ClassBody                        { $$ = new ClassDeclaration($1, $3, $4);} 
                    ;

Modifiers           :   PUBLIC                              { $$ = new Modifiers(ModifierType.PUBLIC);}
                    |   PRIVATE                             { $$ = new Modifiers(ModifierType.PRIVATE);}
                    |   STATIC                              { $$ = new Modifiers(ModifierType.STATIC);}
                    |   Modifiers PUBLIC                    { ((Modifiers)$1).AddModType(ModifierType.PUBLIC); $$ = $1; }
                    |   Modifiers PRIVATE                   { ((Modifiers)$1).AddModType(ModifierType.PRIVATE); $$ = $1; }
                    |   Modifiers STATIC                    { ((Modifiers)$1).AddModType(ModifierType.STATIC); $$ = $1; }
                    ;

ClassBody           :   LBRACE MemberDeclarations RBRACE    { $$ = new ClassBody($2); }
                    |   LBRACE RBRACE                       { $$ = new ClassBody(); }
                    ;

MemberDeclarations  :   MemberDeclaration                    { $$ = $1; }
                    |   MemberDeclarations MemberDeclaration { $1.makeSibling($2); $$ = $1; }
                    ;

MemberDeclaration   :   FieldDeclaration SEMICOLON           { $$ = $1; }
                    |   MethodDeclaration                    { $$ = $1; }
                    |   ConstructorDeclaration               { $$ = $1; }
                    |   StaticInitializer                    { $$ = $1; }
                    |   StructDeclaration                    { $$ = $1; }
                    ;

MethodDeclaration   :   Modifiers TypeSpecifier MethodSignature MethodBody          {$$ = new MethodDeclaration($1, $2, $3, $4); }
                    ;

MethodSignature     :   IDENTIFIER LPAREN ParameterList RPAREN                      { $$ = new MethodSignature($1, $3); }
                    |   IDENTIFIER LPAREN RPAREN                                    { $$ = new MethodSignature($1); }
                    ;

ParameterList       :   Parameter                                                   { $$ = new ParameterList($1); }
                    |   ParameterList COMMA Parameter                               { $1.makeSibling($3); $$ = $1;}  
                    ;

Parameter           :   TypeSpecifier IDENTIFIER                                    { $$ = new Parameter($1, $2); }
                    ;

MethodBody          :   Block                                   { $$ = new MethodBody($1); }
                    ;


StructDeclaration   :   Modifiers STRUCT IDENTIFIER ClassBody   { $$ = new StructDeclaration($1, $3, $4); }
                    ;

FieldDeclaration    :   Modifiers TypeSpecifier FieldNames      { $$ = new FieldDeclaration($1, $2, $3); }
                            ;

TypeSpecifier               :   TypeName                        { $$ = $1; }
                            |   ArraySpecifier                                              
                            ;

TypeName                    :   PrimitiveType                   { $$ = $1; }
                            |   QualifiedName                   { $$ = $1; }
                            ;

ArraySpecifier              :   TypeName LBRACKET RBRACKET      
                            ;
                            
PrimitiveType               :   BOOLEAN                         { $$ = new PrimitiveType(EnumPrimitiveType.BOOLEAN); }
                            |   INT                             { $$ = new PrimitiveType(EnumPrimitiveType.INT); }
                            |   VOID                            { $$ = new PrimitiveType(EnumPrimitiveType.VOID); }
                            ;

FieldNames                  :   NameList                        { $$ = $1; }
                            ;
                            
NameList                    :   IDENTIFIER                      { $$ = new NameList($1); }
                            |   NameList COMMA IDENTIFIER       { $1.Child.makeSibling($3); $$ = $1;}
                            ;


ConstructorDeclaration      :   Modifiers MethodSignature Block { $$ = $1; } 
                            ;

StaticInitializer           :   STATIC Block                     { $$ = new StaticInitializer($2); }
                            ;
        
Block                       :   LBRACE LocalItems RBRACE              { $$ = new Block($2);} 
                            |   LBRACE RBRACE
                            ;

LocalItems	   	            :   LocalItem						      { $$ = $1;}                            
                            |   LocalItems LocalItem                  { $1.makeSibling($2); $$ = $1; }
                            ;    

LocalItem                   :   LocalVariableDeclaration			 { $$ = $1; } 
                            |   Statement                            { $$ = $1; }
                            ;

LocalVariableDeclaration	:   TypeSpecifier LocalVariableNames SEMICOLON { $$ = new LocalVariableDeclaration($1, $2); }
                            |   StructDeclaration                          { $$ = $1; } 
                            ;

LocalVariableNames          :   NameList { $$ = $1; }
                            ;

                            
Statement                   :   EmptyStatement                      { $$ = $1; }                       
                            |   ExpressionStatement                 { $$ = $1; }
                            |   SelectionStatement                  { $$ = $1; }
                            |   IterationStatement                  { $$ = $1; }
                            |   ReturnStatement                     { $$ = $1; }
                            |   Block                               { $$ = $1; } 
                            ;

EmptyStatement              :   SEMICOLON                           { $$ = new Noop(); }                       
                            ;

ExpressionStatement         :   Expression  SEMICOLON               { $$ = $1;}                                                       
                            ;

SelectionStatement          :   IF LPAREN Expression RPAREN Statement ELSE Statement { $$ = new SelectionStatement($3, $5, $7); }
                            |   IF LPAREN Expression RPAREN Statement                { $$ = new SelectionStatement($3, $5); }
                            ;


IterationStatement          :   WHILE LPAREN Expression RPAREN Statement             { $$ = new IterationStatement($3, $5); }
                            ;

ReturnStatement             :   RETURN Expression SEMICOLON                          { $$ = new ReturnStatement($2); }
                            |   RETURN            SEMICOLON                          { $$ = new ReturnStatement(); }
                            ;

ArgumentList                :   Expression                     { $$ = $1;  }
                            |   ArgumentList COMMA Expression  { $1.makeSibling($3); $$ = $1; }
                            ;


// TODO
Expression                  :   QualifiedName ASSIGN Expression { $$ = new Expression($1, ExprKind.ASSIGN, $3); }
   /* short-circuit OR  */  |   Expression OP_LOR Expression    { $$ = new Expression($1, ExprKind.OP_LOR, $3); } 
   /* short-circuit AND */  |   Expression OP_LAND Expression   { $$ = new Expression($1, ExprKind.OP_LOR, $3); }
                            |   Expression PIPE Expression      { $$ = new Expression($1, ExprKind.PIPE, $3); }
                            |   Expression HAT Expression       { $$ = new Expression($1, ExprKind.HAT, $3); }
                            |   Expression AND Expression       { $$ = new Expression($1, ExprKind.AND, $3); }
                            |   Expression OP_EQ Expression     { $$ = new Expression($1, ExprKind.OP_EQ, $3); }
                            |   Expression OP_NE Expression     { $$ = new Expression($1, ExprKind.OP_NE, $3); }
                            |   Expression OP_GT Expression     { $$ = new Expression($1, ExprKind.OP_GT, $3); }
                            |   Expression OP_LT Expression     { $$ = new Expression($1, ExprKind.OP_LT, $3); }
                            |   Expression OP_LE Expression     { $$ = new Expression($1, ExprKind.OP_LE, $3); }
                            |   Expression OP_GE Expression     { $$ = new Expression($1, ExprKind.OP_GT, $3); }
                            |   Expression PLUSOP Expression	{ $$ = new Expression($1, ExprKind.PLUSOP, $3); }
                            |   Expression MINUSOP Expression	{ $$ = new Expression($1, ExprKind.MINUSOP, $3); }
                            |   PLUSOP Expression %prec UNARY   { $$ = new Expression($1, ExprKind.PLUSOP); }
                            |   MINUSOP Expression %prec UNARY  { $$ = new Expression($1, ExprKind.MINUSOP); }
                            |   Expression ASTERISK Expression  { $$ = new Expression($1, ExprKind.ASTERISK, $3); }
                            |   Expression RSLASH Expression    { $$ = new Expression($1, ExprKind.RSLASH, $3); }
                            |   Expression PERCENT Expression   { $$ = new Expression($1, ExprKind.PERCENT, $3); }
                            |   PrimaryExpression               { $$ = $1; }
                            ;

PrimaryExpression           :   QualifiedName                   { $$ = $1;}   
                            |   NotJustName                     { $$ = $1;}
                            ;

NotJustName                 :   SpecialName                     { $$ = $1;}
                            |   ComplexPrimary                  { $$ = $1;}
                            ;

ComplexPrimary              :   LPAREN Expression RPAREN        { $$ = $2;}
                            |   ComplexPrimaryNoParenthesis     { $$ = $1;}
                            ;

ComplexPrimaryNoParenthesis :   STRING_LITERAL                  { $$ = $1;}
                            |   Number                          { $$ = $1;}
                            |   FieldAccess                     { $$ = $1;}   
                            |   MethodCall                      { $$ = $1;}    
                            ;

FieldAccess                 :   NotJustName PERIOD IDENTIFIER   { $1.makeSibling($3); $$ = $1;}
                            ;       

MethodCall                  :   MethodReference LPAREN ArgumentList RPAREN  { $$ = new MethodCall($1, $3); }
                            |   MethodReference LPAREN RPAREN               { $$ = new MethodCall($1); }
                            ;

MethodReference             :   ComplexPrimaryNoParenthesis  { $$ = $1; }
                            |   QualifiedName                { $$ = $1; }
                            |   SpecialName                  { $$ = $1; }
                           ;

QualifiedName               :   IDENTIFIER                      { $$ = new QualifiedName($1); }
                            |   QualifiedName PERIOD IDENTIFIER { $1.Child.makeSibling($3);  $$ = $1; }
                            ;

SpecialName                 :   THIS { $$ = $1; }
                            |   NULL { $$ = $1; }
                            ;

Number                      :   INT_NUMBER                      { $$ = $1; }
                            ;

%%

