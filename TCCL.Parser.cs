using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ASTBuilder
{
    internal partial class TCCLParser
    {
        public TCCLParser() : base(null) { }

        public AbstractNode Parse(string filename)
        {
            this.Scanner = new TCCLScanner(File.OpenRead(filename));
            this.Parse();
            return CurrentSemanticValue; // The final $$ value in the parser

        }
        public AbstractNode Parse(Stream strm)
        {
            this.Scanner = new TCCLScanner(strm);
            this.Parse();
            return CurrentSemanticValue; // The final $$ value in the parser

        }

    }
}
