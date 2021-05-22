using System;
using System.Collections.Generic;

namespace ASTBuilder
{
    public class SemanticsVisitor
    {
        //Uncomment the next line once you include your symbol table implementation
        protected Symtab table = new Symtab().fillSystem();

        // used to produce a readable trace when desired
        protected String prefix = "";

        public SemanticsVisitor(String oldPrefix = "")
        // Parameter oldPrefix allows creation of this visitor during a traversal
        {
            prefix = oldPrefix + "   ";
        }

        public virtual string ClassName()
        {
            return this.GetType().Name;
        }

        private bool TraceFlag = true;  // Set to true or false to control trace

        private void Trace(AbstractNode node, string suffix = "")
        {
            if (TraceFlag)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(this.prefix + ">> In VisitNode for " + node.ClassName() + " " + suffix);
                Console.ResetColor();
            }
        }

        // The following are not needed now; they follow implementation from Chapter 8

        protected static MethodAttributes currentMethod = null;
        protected void SetCurrentMethod(MethodAttributes m)
        {
            currentMethod = m;
        }
        protected MethodAttributes GetCurrentMethod()
        {
            return currentMethod;
        }

        protected static ClassAttributes currentClass = null;

        protected void SetCurrentClass(ClassAttributes c)
        {
            currentClass = c;
        }
        protected ClassAttributes GetCurrentClass()
        {
            return currentClass;
        }
 

        // Call this method to begin the semantic checking process
        public void CheckSemantics(dynamic node)
        {
            if (node == null) return;

            if (TraceFlag)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(this.prefix + "Started Check Semantics for " + node.ClassName());
                Console.ResetColor();
            }

            VisitNode(node);
        }

        // This version of VisitNode is invoked if a more specialized one is not defined below.
        public virtual void VisitNode(AbstractNode node)
        {
            if (TraceFlag)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(this.prefix + ">> In general VisitNode for " + node.ClassName());
                Console.ResetColor();
            }
            VisitChildren(node);
        }

        public virtual void VisitChildren (AbstractNode node)
        {
            // Nothing to do if node has no children
            if (node.Child == null) return;

            if (TraceFlag)
            {
                Console.ForegroundColor = ConsoleColor.Green;
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

        //Starting Node of an AST
        public void VisitNode(CompilationUnit node)
        {
            Trace(node);

            VisitChildren(node);
        }

        public virtual void VisitNode(ClassDeclaration node)
        {
            Trace(node);

            var name = node.Child.Sib.ToString();
            var classAttributes = new ClassAttributes(name);
            SetCurrentClass(classAttributes);

            table.enter(name, classAttributes);
            table.openScope();

            VisitChildren(node);
        }

        public virtual void VisitNode(SelectionStatement node)
        {
            Trace(node);
            VisitChildren(node);

            var conditionType = node.Child.NodeType;

            if (conditionType is TypeAttributes typeAttrs && typeAttrs.thisType is BooleanTypeDescriptor)
            {
                node.NodeType = conditionType;
            } else
            {
                node.NodeType = new ErrorAttributes("Selection expected Boolean, got " + conditionType.ToString());
            }
        }

        public virtual void VisitNode(IterationStatement node)
        {
            Trace(node);
            VisitChildren(node);

            var conditionType = node.Child.NodeType;

            if (conditionType is TypeAttributes typeAttrs && typeAttrs.thisType is BooleanTypeDescriptor)
            {
                node.NodeType = conditionType;
            } else
            {
                node.NodeType = new ErrorAttributes("Iteration expected Boolean, got " + conditionType.ToString());
            }
        }

        // This method is needed to implement special handling of certain expressions
        public virtual void VisitNode(Expression node)
        {
            Trace(node);

            VisitChildren(node);

            var left = node.Child.NodeType;
            var right = node.Child.Sib.NodeType;

            if (right == null)
            {
                if (left is TypeAttributes typeAttrs && typeAttrs.thisType is IntegerTypeDescriptor)
                {
                    node.NodeType = left;
                } else
                {
                    node.NodeType = new ErrorAttributes("Expression expected Integer, got " + left.ToString() + " and " + right.ToString());
                }
            }
            else if (left.Equals(right))
            {
                switch (node.exprKind) {
                    case ExprKind.OP_LAND:
                    case ExprKind.OP_LOR:
                        // both must be boolean, type is boolean
                        node.NodeType = new TypeAttributes(new BooleanTypeDescriptor());
                        if (left.ToString() != "bool")
                        {
                            node.NodeType = new ErrorAttributes("Operation " + node.exprKind + " expected bool, got " + left);
                        }
                        return;
                    case ExprKind.OP_EQ:
                    case ExprKind.OP_GE:
                    case ExprKind.OP_GT:
                    case ExprKind.OP_LE:
                    case ExprKind.OP_LT:
                    case ExprKind.OP_NE:
                        // Both must be int, type is boolean
                        node.NodeType = new TypeAttributes(new BooleanTypeDescriptor());
                        if (left.ToString() != "int32")
                        {
                            node.NodeType = new ErrorAttributes("Operation " + node.exprKind + " expected int32, got " + left);
                        }
                        return;
                    default:
                        // Both must be int, type is int
                        node.NodeType = left;
                        if (left.ToString() != "int32")
                        {
                            node.NodeType = new ErrorAttributes("Operation " + node.exprKind + " expected int32, got " + left);
                        }
                        return;
                }
            }
            else
            {
                node.NodeType = new ErrorAttributes("Expression has mismatched types, got " + left.ToString() + " and " + right.ToString());
            }
        }
        
        // Node for Tokens have no children and include additional information that is useful in the trace
        public virtual void VisitNode(Identifier node)
        {
            Trace(node, node.Name);
        }

        public virtual void VisitNode(PrimitiveType node)
        {
            Trace(node, node.Type.ToString());
        }

        //define method attribute, increase scope, and check body 
        public void VisitNode(MethodDeclaration node)
        {
            table.openScope();

            Trace(node);
            // Increase indent for trace before visiting children
            String oldPrefix = this.prefix;
            this.prefix += "   ";

            dynamic modifiers = node.Child;
            VisitNode(modifiers);
            dynamic typeSpec = modifiers.Sib;
            VisitNode(typeSpec);

            dynamic signature = typeSpec.Sib;
            VisitNode(signature);

            // Update symbol table with inputs
            dynamic name = signature.Child;
            var returnType = TypeDescriptor.From(typeSpec);
            var parameters = (ParameterList)name.Sib;
            var types = parameters == null ? new List<TypeDescriptor>() : parameters.Types();
            var attrs = new MethodAttributes(returnType, types);
            table.enterInParent(name.ToString(), attrs);
            GetCurrentClass().enter(name.ToString(), attrs);
            node.NodeType = attrs;

            dynamic methodBody = signature.Sib;
            VisitNode(methodBody);

            // Restore prefix
            this.prefix = oldPrefix;
            table.closeScope();
        }

        public void VisitNode(STR_CONST node)
        {
            Trace(node);
            node.NodeType = new TypeAttributes(new StringTypeDescriptor());
        }

        public void VisitNode(INT_CONST node)
        {
            Trace(node);
            node.NodeType = new TypeAttributes(new IntegerTypeDescriptor());
        }

        public void VisitNode(MethodCall node)
        {
            Trace(node);
            VisitChildren(node);

            if (node.Child.ToString() == "WriteLine" || node.Child.ToString() == "Write")
            {
                // skip type checking for system calls
                return;
            }

            var pos = 0;
            var nodeType = node.Child.NodeType;
            if (!(nodeType is MethodAttributes))
            {
                node.NodeType = new ErrorAttributes("Error in Parameter list");
                return;
            }
            var attributes = ((MethodAttributes)nodeType); 
            var argument = node.Child.Sib;
            foreach (var argType in attributes.Arguments)
            {
                if (argument == null)
                {
                    node.NodeType = new ErrorAttributes("Missing parameter for argument");
                    return;
                }

                if (argument.NodeType is TypeAttributes)
                {
                    var actualTypeString = ((TypeAttributes)argument.NodeType).ToString();
                    var expectedTypeString = argType.type;
                    if (actualTypeString != expectedTypeString)
                    {
                        node.NodeType = new ErrorAttributes("Mismatched arguments in position " + pos + ", expected " + expectedTypeString + " got " + actualTypeString);
                        return;
                    }
                }

                pos += 1;
                argument = argument.Sib;
            }
            node.NodeType = new TypeAttributes(attributes.Return);
        }

        public void VisitNode(Parameter node)
        {
            Trace(node);

            node.NodeType = new TypeAttributes(node.Type());
            var declaration = node.Child.Sib;
            var name = ((Identifier)declaration).Name;
            if (table.Has(name))
            {
                // Mark as redeclaration error
            } else
            {
                table.enter(name, node.NodeType);
            }

            inDeclaration = true;
            VisitChildren(node);
            inDeclaration = false;
        }

        public void VisitNode(LocalVariableDeclaration node)
        {
            Trace(node);

            node.NodeType = new TypeAttributes(node.Type());
            var declarationNames = node.Child.Sib;
            foreach (var declaration in declarationNames.Children())
            {
                var name = ((Identifier)declaration).Name;
                if (table.Has(name))
                {
                    // Mark as redeclaration error
                } else
                {
                    table.enter(name, node.NodeType);
                }
            }

            inDeclaration = true;
            VisitChildren(node);
            inDeclaration = false;
        }

        private bool inDeclaration = false;

        public void VisitNode(QualifiedName node) {
            if (inDeclaration) return;

            var name = node.Child.ToString();
            if (!table.Has(name))
            {
                // Add an undeclared access error
                node.NodeType = new ErrorAttributes("Accessing undeclared variable " + name);
            } else
            {
                if (node.Child.Sib == null)
                {
                    node.NodeType = table.lookup(name);
                } else
                {
                    var scope = (ClassAttributes) table.lookup(name);
                    node.NodeType = scope.lookup(node.Child.Sib.ToString());
                }
            }
        }
    }
}
    
