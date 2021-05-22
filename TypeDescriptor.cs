using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASTBuilder
{
    /******************************************/
    /*Describes type of node for decorated AST*/
    /******************************************/
    public abstract class TypeDescriptor
    {
        // All types include a string representation for code generation purposes.
        public string type;  
        
        public void PrintType()
        {
            Console.WriteLine("    " + this.GetType().ToString());
        }

        public static TypeDescriptor From(AbstractNode node)
        {
            if (node is PrimitiveType)
            {
                switch (((PrimitiveType)node).Type)
                {
                    case EnumPrimitiveType.BOOLEAN: return new BooleanTypeDescriptor();
                    case EnumPrimitiveType.INT: return new IntegerTypeDescriptor();
                }
            }
            if (node is QualifiedName)
            {
                return new QualifiedTypeDescriptor((QualifiedName)node);
            }
            return new VoidTypeDescriptor();
        }
    }

    /******************************************/
    /*********Basic Primitive Types************/
    /******************************************/
    public class IntegerTypeDescriptor : TypeDescriptor
    {
        public IntegerTypeDescriptor()
        {
            type = "int32";
        }
    }
    public class StringTypeDescriptor : TypeDescriptor
    {
        public StringTypeDescriptor()
        {
            type = "string";
        }
    }
    public class BooleanTypeDescriptor : TypeDescriptor
    {
        public BooleanTypeDescriptor()
        {
            type = "bool";
        }
    }
    public class VoidTypeDescriptor : TypeDescriptor
    {
        public VoidTypeDescriptor()
        {
            type = "void";
        }
    }

    public class QualifiedTypeDescriptor: TypeDescriptor
    {
        public QualifiedName Name { get; set; }
        public QualifiedTypeDescriptor(QualifiedName name) {
            Name = name;
            type = name.ToString();
        }

    }
   
    /*****MSCoreLib Type Descriptor for mscorlib code generation*****/
    public class MSCorLibTypeDescriptor : TypeDescriptor
    {
        public MSCorLibTypeDescriptor()
        {
            type = "void [mscorlib]System.Console::";
        }
    }

    /*****Error Type Descriptor*****/
    public class ErrorTypeDescriptor : TypeDescriptor
    {
    }
}
