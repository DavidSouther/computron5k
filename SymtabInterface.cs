namespace ASTBuilder
{
	
	public interface SymtabInterface
	{
	   /// Open a new nested symbol table scope
	   void openScope();
	  
	   /// Close an existng nested scope
	   /// There must be an existng scope to close
	   void closeScope();

	   int CurrentNestLevel {get;}

	   void enter(string id, Attributes attr);

	   Attributes lookup(string id);

	}


}
