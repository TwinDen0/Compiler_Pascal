using System.Xml.Linq;

namespace Compiler
{
    public class SymType : Symbol
    {
        public SymType(string name) : base(name) { }
        public override string ToString(string prefix)
        {
            return $"{GetName()}";
        }
    }
    public class SymInteger : SymType
    {
        public SymInteger(string name) : base(name) { }
    }
    public class SymReal : SymType
    {
        public SymReal(string name) : base(name) { }
    }
    public class SymString : SymType
    {
        public SymString(string name) : base(name) { }
    }
    public class SymBoolean : SymType
    {
        public SymBoolean(string name) : base(name) { }
    }
    public class SymArray : SymType
    {
        List<OrdinalTypeNode> _ordinal_types;
        SymType _type;
        public SymType GetArrayType()
        {
            return _type;
        }
        public List<OrdinalTypeNode> GetOrdinalTypeNode()
        {
            return _ordinal_types;
        }
        public SymArray(string name, List<OrdinalTypeNode> ordinalTypes, SymType type) : base(name)
        {
            _ordinal_types = ordinalTypes;
            _type = type;
        }
        public override string ToString(string prefix)
        {
            string str;
            str = $"array\r\n";
            foreach (OrdinalTypeNode ordinalType in _ordinal_types)
            {
                str += prefix + $"├─── {ordinalType.ToString(prefix + ChildrenPrefix(true))}\r\n";
            }
            str += prefix + $"└─── {_type.ToString(prefix + ChildrenPrefix(true))}";
            return str;
        }
    }
    public class OrdinalTypeNode : Node
    {
        NodeExpression _from;
        NodeExpression _to;
        public OrdinalTypeNode(NodeExpression from, NodeExpression to)
        {
            _from = from;
            _to = to;
        }
        public override string ToString(string prefix)
        {
            string str;
            str = $"..\r\n";
            str += prefix + $"├─── {_from.ToString(prefix + ChildrenPrefix(true))}\r\n";
            str += prefix + $"└─── {_to.ToString(prefix + ChildrenPrefix(false))}";
            return str;
        }
    }
    public class SymRecord : SymType
    {
        SymTable _fields;
        public SymTable GetFields()
        {
            return _fields;
        }
        public SymRecord(string name, SymTable fields) : base(name)
        {
            _fields = fields;
        }
        public override string ToString(string prefix)
        {
            string str;
            str = $"record \r\n";
            List<Symbol> sym_fields = new List<Symbol>(_fields.GetData().Values);
            int i = 1;
            foreach (Symbol symField in sym_fields)
            {
                SymVar varField = (SymVar)symField;
                if (i == sym_fields.Count)
                {
                    str += prefix + $"└─── {varField.GetName()}\r\n";
                    str += prefix + $"     └─── {varField.GetOriginalTypeVar().ToString(prefix + ChildrenPrefix(false))}";
                }
                else
                {
                    str += prefix + $"├─── {varField.GetName()}\r\n";
                    str += prefix + $"│    └─── {varField.GetOriginalTypeVar().ToString(prefix + ChildrenPrefix(true))}\r\n";
                    i++;
                }
            }
            return str;
        }
    }
    public class SymTypeAlias : SymType
    {
        SymType _original;
        public SymType GetOriginalType()
        {
            return _original;
        }
        public SymTypeAlias(string name, SymType original) : base(name)
        {
            _original = original;
        }
        public override string ToString(string prefix)
        {
            string str;
            string name = GetName();
            if (name != _original.GetName())
            {
                str = $"{name} ({_original.ToString(prefix.Remove(prefix.Length - 5) + ChildrenPrefix(true))})";
            }
            else
            {
                str = $"{_original.ToString(prefix + ChildrenPrefix(false))}";
            }
            return str;
        }
    }
}
