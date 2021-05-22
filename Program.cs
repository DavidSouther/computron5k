using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Utilities;


namespace ASTBuilder
{
    class Program
    {
        public static void ILasm(String filename)
        {

            var ilasm = Microsoft.Build.Utilities.ToolLocationHelper.GetPathToDotNetFrameworkFile("ilasm.exe", TargetDotNetFrameworkVersion.VersionLatest);

            string filenameil = Path.Combine(Directory.GetCurrentDirectory(), filename + ".il");

            string ilasmArg = "\"" + filenameil + "\"";
            string execArgs = "/c \"" + filename + ".exe\" &pause";

            Console.WriteLine("Invoking ILASM: {0}", ilasm + " " + ilasmArg);
            Console.WriteLine("----------------------------------------");

            Process ilProcess = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = ilasm,
                    Arguments = ilasmArg
                }
            };
            ilProcess.Start();

            string output = ilProcess.StandardOutput.ReadToEnd();
            ilProcess.WaitForExit();
            Console.WriteLine(output);

            Console.WriteLine("Invoking compiled executable: {0}", filename);
            Console.WriteLine("----------------------------------------");


            Process userProcess = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = execArgs,
                },
                EnableRaisingEvents = true,
            };

            userProcess.Start();
            userProcess.WaitForExit();

        }


        // Use the PrintVisitor to print out an abstract syntax tree
        static void PrintTree(AbstractNode root)
        {
            PrintVisitor visitor = new PrintVisitor();
            Console.WriteLine("Starting to print AST ");
            visitor.PrintTree(root);
            Console.WriteLine("AST complete");

        }

        static void DoSemantics(AbstractNode root)
        {
            SemanticsVisitor visitor = new SemanticsVisitor("");
            Console.WriteLine("Starting semantic checking");
            visitor.CheckSemantics(root);
            Console.WriteLine("Semantic checking complete");
        }

        static void GenerateIL(AbstractNode root, string filename)
        {
            CodeGenVisitor visitor = new CodeGenVisitor();
            Console.WriteLine("Starting code generation");
            visitor.GenerateCode(root, filename);
        }

        static void Main(string[] args)
        {
            var parser = new TCCLParser();
            string name;
            AbstractNode ast;
            while (true)
            {
                Console.Write("Enter a file name: ");
                name = Console.ReadLine();
                Console.WriteLine("Compiling file " + name);
                ast = parser.Parse(name + ".txt");
                // Do semantc checking before printing the AST
                DoSemantics(ast);
                PrintTree(ast);
                // Generate IL and then call assembler
                GenerateIL(ast, name);
                ILasm(name);
            }

        }

    }
}
