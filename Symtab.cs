using System;
using System.Collections.Generic;

namespace ASTBuilder
{
	public class Symtab : SymtabInterface
	{
		private LinkedList<Dictionary<string, Attributes>> scopes = new LinkedList<Dictionary<string, Attributes>>();

		/// The name makes the use of this field obvious
		/// It should never have a negative integer as its value
		public int CurrentNestLevel { get; private set; }

		public Symtab()
		{
			this.CurrentNestLevel = 0;
		}

		public Symtab fillSystem()
        {
			openScope();
			enter("WriteLine", new MethodAttributes("WriteLine", new ClassAttributes("System.Console"), new VoidTypeDescriptor(), new List<TypeDeclaration> { new TypeDeclaration("arg", new StringTypeDescriptor() )}));
			enter("Write", new MethodAttributes("Write", new ClassAttributes("System.Console"), new VoidTypeDescriptor(), new List<TypeDeclaration> { new TypeDeclaration("arg", new StringTypeDescriptor()) }));
			return this;
        }

		/// <summary>
		/// Opens a new scope, retaining outer ones 
		/// </summary>
		public virtual void openScope()
		{
			scopes.AddFirst(new Dictionary<string, Attributes>());
			this.CurrentNestLevel += 1;
		}

		/// <summary>
		/// Closes the innermost scope </summary>
		/// </summary>
		public virtual void closeScope()
		{
			if (this.CurrentNestLevel == 0) return;
			scopes.RemoveFirst();
			this.CurrentNestLevel += 1;
		}

		/// <summary>
		/// Enter the given symbol information into the symbol table.  If the given
		///    symbol is already present at the current nest level, produce an error 
		///    message, but do NOT throw any exceptions from this method.
		/// </summary>
		public virtual void enter(string s, Attributes info)
		{
			scopes.First.Value.Add(s, info);
		}

		public virtual void enterInParent(string s, Attributes info)
		{
			scopes.First.Next.Value.Add(s, info);
		}

		public virtual bool Has(string s)
        {
			foreach (var scope in scopes)
            {
				if (scope.ContainsKey(s))
                {
					return true;
                }
            }
			return false;
        }

		/// <summary>
		/// Returns the information associated with the innermost currently valid
		///     declaration of the given symbol.  If there is no such valid declaration,
		///     return null.  Do NOT throw any excpetions from this method.
		/// </summary>
		public virtual Attributes lookup(string s)
		{
			foreach (var scope in scopes)
			{
				if (scope.ContainsKey(s))
				{
					return scope[s];
				}
			}
			return null;
		}

		public virtual void @out(string s)
		{
			string tab = "";
			for (int i = 1; i <= CurrentNestLevel; ++i)
			{
				tab += "  ";
			}
			Console.WriteLine(tab + s);
		}

		public virtual void err(string s)
		{
			@out("Error: " + s);
			//Console.Error.WriteLine("Error: " + s);
			Environment.Exit(-1);
		}
	}
}