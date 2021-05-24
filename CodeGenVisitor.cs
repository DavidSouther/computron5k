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

        private int nextLabel = 0;
        private string MakeLabel(string prefix)
        {
            return $"{prefix}_{nextLabel++}";
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
        }

        public void VisitNode(MethodDeclaration node)
        {
            currentMethod = (MethodAttributes) node.NodeType;
            dynamic modifiers = node.Child;
            var name = modifiers.Sib.Sib.Child.ToString();
            file.WriteLine($".method static {currentMethod.Return} {name}()");
            file.WriteLine("{");
            if (name == "main") file.WriteLine("  .entrypoint");
            file.WriteLine("  .maxstack 3"); // TODO calculate size
            VisitChildren(node);
            file.WriteLine("  ret");
            file.WriteLine("}");
            currentMethod = null;
        }

        private MethodAttributes currentMethod = null;

        public void VisitNode(MethodCall node) {
            Trace(node);
            VisitChildren(node);
            var ret = "void";
            var arg = "string";
            dynamic name = node.Child;
            if (name.ToString() == "Write" || name.ToString() == "WriteLine")
            {
                var type = ((TypeAttributes)name.Sib.NodeType).thisType;
                file.WriteLine($"  call void [mscorlib]System.Console::WriteLine({type})");
            } else
            {
                file.WriteLine($"  call {ret} {name}({arg})");
            }
        }

        public void VisitNode(STR_CONST node)
        {
            Trace(node);
            file.WriteLine($"  ldstr \"{node.ToString()}\"");
        }

        public void VisitNode(INT_CONST node)
        {
            Trace(node);
            if (node.IntVal < 128)
            {
                // Short form
                file.WriteLine($"  ldc.i4.s {node.IntVal}");
            } else
            {
                file.WriteLine($"  ldc.i4 {node.IntVal}");
            }
        }

        private void Store(QualifiedName node)
        {
            // Generate stloc for node
            int location = currentMethod.Location(node);
            if (location < 4)
            {
               file.WriteLine($"  stloc.{location}");
            } else if (location < 128)
            {
               file.WriteLine($"  stloc.s {location}");
            } else
            {
               file.WriteLine($"  stloc {location}");
            }
        }

        public void VisitNode(Expression node)
        {
            Trace(node);
            dynamic left = node.Child;
            dynamic right = left.Sib;

            if (node.exprKind == ExprKind.ASSIGN)
            {
                VisitNode(right);
                Store((QualifiedName)left);
            }
            else
            {
                if (right != null)
                {
                    if (node.exprKind == ExprKind.OP_LAND)
                    {
                        // Short circuit and
                        var endLabel = MakeLabel("and_short_circuit");
                        VisitNode(left);
                        file.WriteLine($"  bfalse.s {endLabel}");
                        // Left was true, clear it from the stack
                        file.WriteLine("  pop");
                        VisitNode(right);
                        // Right is final state of expression
                        file.WriteLine($"{endLabel}:");
                    }
                    else if (node.exprKind == ExprKind.OP_LOR)
                    {
                        // Short circuit OR
                        var endLabel = MakeLabel("or_short_circuit");
                        VisitNode(left);
                        file.WriteLine($"  btrue.s {endLabel}");
                        // Left was false, clear it from the stack
                        file.WriteLine("  pop");
                        VisitNode(right);
                        // Right is final state of expression
                        file.WriteLine($"{endLabel}:");
                    }
                    else
                    {
                        VisitNode(left);
                        VisitNode(right);
                        file.WriteLine(opcode(node.exprKind));
                    }
                }
                else
                {
                    // TODO unary conversion
                    if (node.exprKind == ExprKind.MINUSOP)
                    {
                        file.Write("  ldc.i4.0");
                        VisitNode(left);
                        file.Write("  sub");
                    }
                }
            }

        }

        private string opcode(ExprKind exprKind)
        {
            switch(exprKind)
            {
                // Bitwise operators
                case ExprKind.PIPE: return "  or";
                case ExprKind.HAT: return "  xor";
                case ExprKind.AND: return "  and";

                // Arithmetic operators
                case ExprKind.PLUSOP: return "  add";
                case ExprKind.MINUSOP: return "  sub";
                case ExprKind.ASTERISK: return "  mul";
                case ExprKind.RSLASH: return "  div";
                case ExprKind.PERCENT: return "  rem";

                // Comparison operators
                case ExprKind.OP_EQ: return "  ceq";
                case ExprKind.OP_NE: return "  ceq\n  not";
                case ExprKind.OP_GT: return "  cgt";
                case ExprKind.OP_LT: return "  clt";
                case ExprKind.OP_LE: return "  cgt\n  not";
                case ExprKind.OP_GE: return "  clt\n  not";

                default: return "noop // error"; // TODO throw
            }
        }
    }
}
