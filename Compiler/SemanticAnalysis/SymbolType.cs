using System.Xml.Linq;

namespace Compiler
{
    public class SymType : Symbol
    {
        public SymType(string name) : base(name) { }
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
        public override string ToString()
        {
            string name = GetName();
            if (name == _original.ToString())
            {
                return name;
            } 
            else
            {
                return $"{name} ({_original.ToString()})";
            }
        }
    }
}
