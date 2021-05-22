using System;
using System.Collections.Generic;
using System.Text;

namespace ASTBuilder
{
    internal partial class TCCLScanner
    {

		public override void yyerror(string format, params object[] args)
		{
			base.yyerror(format, args);
			Console.WriteLine(format, args);
			Console.WriteLine();
		}

    }
}
