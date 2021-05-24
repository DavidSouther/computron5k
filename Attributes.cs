using System;
using System.Collections.Generic;

namespace ASTBuilder
{
    public class TypeDeclaration
    {
        public string Name { get; set; }
        public TypeDescriptor Type { get; set; }
        public TypeDeclaration(string name, TypeDescriptor type)
        {
            Name = name;
            Type = type;
        }
    }

    /****************************************************/
    /*Information about symbols held in AST nodes and the symbol table*/
    /****************************************************/
    public abstract class Attributes
    {
        public Attributes() 
        {
           
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return ToString() == obj.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class VariableAttributes : Attributes
    {
        public VariableAttributes() { }
    }

    public class TypeAttributes : Attributes
    {
        public TypeDescriptor thisType;
        public TypeAttributes(TypeDescriptor type)
        {
            thisType = type;
        }

        public override string ToString()
        {
            return thisType.type;
        }
    }
    
    public class MethodAttributes : Attributes
    {
        public TypeDescriptor Return;
        public List<TypeDescriptor> Arguments;
        private List<TypeDeclaration> locals = new List<TypeDeclaration>();

        public MethodAttributes(TypeDescriptor ret, List<TypeDescriptor> args)
        {
            Return = ret;
            Arguments = args;
        }

        public override string ToString()
        {
            var ret = "";
            if (Arguments.Count == 0) {
                ret += "() -> ";
            }
            else
            {
                foreach (var arg in Arguments)
                {
                    ret += arg.type + " -> ";
                }
            }
            ret += Return.type;
            return ret;
        }

        public void RegisterLocal(string name, TypeDescriptor type)
        {
            foreach(var local in locals)
            {
                if (local.Name == name) return;
            }
            locals.Add(new TypeDeclaration(name, type));
        }

        public int Location(QualifiedName name)
        {
            for(var i = 0; i < locals.Count; i++)
            {
                if (name.ToString() == locals[i].Name)
                {
                    return i;
                }
            }
            return -1;
        }
    }

    public class ClassAttributes: Attributes
    {
        public String Name;
        private Dictionary<string, Attributes> properties = new Dictionary<string, Attributes>();

        public ClassAttributes(string name)
        {
            Name = name;
        }

        public bool has(string name)
        {
            return properties.ContainsKey(name);
        }

        public void enter(string name, Attributes attributes)
        {
            properties[name] = attributes;
        }

        public Attributes lookup(string name) {
            return properties[name];
        }
    }

    public class ErrorAttributes : Attributes
    {
        public string Error { get; set; }
        public ErrorAttributes (string error)
        {
            Error = error;
        }

        public override string ToString()
        {
            return Error;
        }
    }
}
