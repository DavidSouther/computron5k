using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ASTBuilder
{
    class CodeGenVisitor 
    {

        private StreamWriter file;  // IL code written to this file

        // used to produce a readable trace when desired
        protected String prefix = "";
        
        public virtual string ClassName()
        {
            return this.GetType().Name;
        }

        private bool TraceFlag = true;  // Set to true or false to control trace

        private void Trace(AbstractNode node, string suffix = "")
        {
            if (TraceFlag)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(this.prefix + ">> In VistNode for " + node.ClassName() + " " + suffix);
                Console.ResetColor();
            }
        }
        public void GenerateCode(dynamic node, string filename)
            // node is the root of the AST for the entire TCCL program
        {
            if (node == null) return;  // No code generation for empty AST
            
            file = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), filename + ".il"));

            if (TraceFlag)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(this.prefix + "Started code Generation for " + filename + ".txt");
                Console.ResetColor();
            }
            // Since node is the root node of the AST, this call begins the code generation traversal
            VisitNode(node);

            file.Close();

        }

        public virtual void VisitNode(AbstractNode node)
        {
            if (TraceFlag)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(this.prefix + ">> In general VistNode for " + node.ClassName());
                Console.ResetColor();
            }

            VisitChildren(node);
        }
        public virtual void VisitChildren(AbstractNode node)
        {
            // Nothing to do if node has no children
            if (node.Child == null) return;

            if (TraceFlag)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(this.prefix + " └─In VistChildren for " + node.ClassName());
                Console.ResetColor();
            }
            // Increase prefix while visiting children
            String oldPrefix = this.prefix;
            this.prefix += "   "; 
            
            foreach (dynamic child in node.Children())
            {
                VisitNode(child);
            };

            this.prefix = oldPrefix;
        }
        public virtual void VisitNode(CompilationUnit node)
        {
            Trace(node);

            // The two lines that follow generate the prelude required in all .il files
            file.WriteLine(".assembly extern mscorlib {}");
            file.WriteLine(".assembly test1 { }");

            VisitChildren(node);

            // The following lines are present so that an executable .il file is generated even
            // before you have implemented any VisitNode routines.  It generated the body for the
            // hello.txt program regardless of what file is parsed to create the AST.
            // DELETE THESE LINES ONCE YOU HAVE IMPLEMENTED VisitNode FOR MethodDeclaration
            file.WriteLine(".method static void main()");
            file.WriteLine("{");
            file.WriteLine("   .entrypoint");
            file.WriteLine("   .maxstack 1");
            file.WriteLine("   ldstr \"Hello world!\"");
            file.WriteLine("   call void [mscorlib]System.Console::WriteLine(string)");
            file.WriteLine("   ret");
            file.WriteLine("}");
            // DELETE TRHOUGH HERE

        }

    }
}
