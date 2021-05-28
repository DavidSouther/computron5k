using System;
using System.Collections.Generic;

namespace ASTBuilder
{

    public class CompilationUnit : AbstractNode
    {
        // just for the CompilationUnit because it's the top node
        //public override AbstractNode LeftMostSibling => this;
        public override AbstractNode Sib => null;

        public CompilationUnit(AbstractNode classDecl)
        {
            adoptChildren(classDecl);
        }

    }

    public class ClassDeclaration : AbstractNode
    {
        public ClassDeclaration(
            AbstractNode modifiers,
            AbstractNode className,
            AbstractNode classBody)
        {
            adoptChildren(modifiers);
            adoptChildren(className);
            adoptChildren(classBody);
        }

    }

    public class Noop : AbstractNode { }

    public class ClassBody : AbstractNode
    {
        public ClassBody() { }
        public ClassBody(AbstractNode members)
        {
            adoptChildren(members);
        }
    }
    public enum ModifierType { PUBLIC, STATIC, PRIVATE }

    public class Modifiers : AbstractNode
    {
        public List<ModifierType> ModifierTokens { get; set; } = new List<ModifierType>();

        public void AddModType(ModifierType type)
        {
            ModifierTokens.Add(type);
        }

        public Modifiers(ModifierType type) : base()
        {
            AddModType(type);
        }

    }

    public class MissingAST : AbstractNode
    {
        public virtual string Message { get; protected set; }

        public MissingAST(string s)
        {
            Message = s;
        }
    }

    public class Identifier : AbstractNode
    {
        public virtual string Name { get; protected set; }

        public Identifier(string s)
        {
            Name = s;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class INT_CONST : AbstractNode
    {
        public virtual int IntVal { get; protected set; }

        public INT_CONST(string s)
        {
            IntVal = Int32.Parse(s);
        }

        public override string ToString()
        {
            return IntVal.ToString();
        }
    }

    public class STR_CONST : AbstractNode
    {
        public virtual string StrVal { get; protected set; }

        public STR_CONST(string s)
        {
            StrVal = s;
        }
        public override string ToString()
        {
            return StrVal;
        }
    }


    public class MethodDeclaration : AbstractNode
    {
        public MethodDeclaration(
            AbstractNode modifiers,
            AbstractNode typeSpecifier,
            AbstractNode methodSignature,
            AbstractNode methodBody)
        {
            adoptChildren(modifiers);
            adoptChildren(typeSpecifier);
            adoptChildren(methodSignature);
            adoptChildren(methodBody);
        }

    }

    public enum EnumPrimitiveType { BOOLEAN, INT, VOID }
    public class PrimitiveType : AbstractNode
    {
        public EnumPrimitiveType Type { get; set; }
        public PrimitiveType(EnumPrimitiveType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
    
     public class MethodBody : AbstractNode
    {
        public MethodBody(AbstractNode localItems)
        {
            adoptChildren(localItems);
        }
    }

    public class Parameter : AbstractNode
    {
        public Parameter(AbstractNode typeSpec, AbstractNode declName) : base()
        {
            adoptChildren(typeSpec);
            adoptChildren(declName);
        }

        public TypeDescriptor Type()
        {
            return TypeDescriptor.From(Child);
        }

        public string Name {
            get {
                return Child.Sib.ToString();
            }
        }

        public TypeDeclaration Declaration()
        {
            return new TypeDeclaration(Name, Type());
        }
    }

    public class ParameterList : AbstractNode
    {
        public ParameterList(AbstractNode parameter) : base()
        {
            adoptChildren(parameter);
        }

        public List<TypeDeclaration> Types()
        {
            var parameters = new List<TypeDeclaration>();
            dynamic child = Child;
            while (child != null)
            {
                parameters.Add(((Parameter)child).Declaration());
                child = child.Sib;
            }

            return parameters;
        }
    }

    public class NameList : AbstractNode
    {
        public NameList(AbstractNode parameter) : base()
        {
            adoptChildren(parameter);
        }
    }

    public class MethodSignature : AbstractNode
    {
        public MethodSignature(AbstractNode name)
        {
            adoptChildren(name);
        }

        public MethodSignature(AbstractNode name, AbstractNode paramList)
        {
            adoptChildren(name);
            adoptChildren(paramList);
        }
    }

    public class StaticInitializer : AbstractNode
    {
        public StaticInitializer(AbstractNode block)
        {
            adoptChildren(block);
        }
    }

    public class MethodCall : AbstractNode
    {
        public MethodCall(AbstractNode reference)
        {
            adoptChildren(reference);
        }

        public MethodCall(AbstractNode reference, AbstractNode parameters)
        {
            adoptChildren(reference);
            adoptChildren(parameters);
        }
    }

    public class LocalVariableDeclaration : AbstractNode
    {
        public LocalVariableDeclaration(AbstractNode type, AbstractNode names)
        {
            adoptChildren(type);
            adoptChildren(names);
        }

        public TypeDescriptor Type()
        {
            return TypeDescriptor.From(this.Child);
        }
    }

    public class StructDeclaration : AbstractNode
    {
        public StructDeclaration(AbstractNode modifiers, AbstractNode identifier, AbstractNode body)
        {
            adoptChildren(modifiers);
            adoptChildren(identifier);
            adoptChildren(body);
        }
    }
    public class FieldDeclaration : AbstractNode
    {
        public FieldDeclaration(AbstractNode modifiers, AbstractNode type, AbstractNode fields)
        {
            adoptChildren(modifiers);
            adoptChildren(type);
            adoptChildren(fields);
        }
    }

    public class SelectionStatement : AbstractNode
    {
        public SelectionStatement(AbstractNode expression, AbstractNode trueCase)
        {
            adoptChildren(expression);
            adoptChildren(trueCase);
        }
        public SelectionStatement(AbstractNode condition, AbstractNode trueCase, AbstractNode falseCase)
        {
            adoptChildren(condition);
            adoptChildren(trueCase);
            adoptChildren(falseCase);
        }
    }

    public class Block : AbstractNode
    {
        public Block (AbstractNode items)
        {
            adoptChildren(items);
        }
    }

    public class IterationStatement : AbstractNode
    {
        public IterationStatement(AbstractNode condition, AbstractNode body)
        {
            adoptChildren(condition);
            adoptChildren(body);
        }
    }

    public class ReturnStatement: AbstractNode
    {
        public ReturnStatement(AbstractNode expression)
        {
            adoptChildren(expression);
        }

        public ReturnStatement() { }
    }

    public class QualifiedName : AbstractNode
    {
        public QualifiedName(AbstractNode pathPart)
        {
            adoptChildren(pathPart);
        }

        public override string ToString()
        {
            var next = this.Child;
            string id = "";
            do
            {
                id += ((Identifier)next).Name + ".";
                next = next.Sib;
            } while (next != null);
            return id.Substring(0, id.Length - 1);
        }
    }

    public enum ExprKind
    {
        ASSIGN, OP_LOR, OP_LAND, PIPE, HAT, AND, OP_EQ,
        OP_NE, OP_GT, OP_LT, OP_LE, OP_GE, PLUSOP, MINUSOP,
        ASTERISK, RSLASH, PERCENT, UNARY, PRIMARY,
    }
    public class Expression : AbstractNode
    {
        public ExprKind exprKind { get; set; }
        public Expression(AbstractNode expr, ExprKind kind)
        {
            adoptChildren(expr);
            exprKind = kind;
        }
        public Expression(AbstractNode lhs, ExprKind kind, AbstractNode rhs)
        {
            adoptChildren(lhs);
            adoptChildren(rhs);
            exprKind = kind;
        }

        public override string ToString()
        {
            return exprKind.ToString();
        }

    }
     
}

