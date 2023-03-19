using System.Xml.Linq;

namespace Compiler
{
    public class NodeExpression : Node
    {
        protected SymType _cached_type = new SymType("");
        public SymType GetCachedType()
        {
            return _cached_type;
        }
        public virtual SymType CalcType()
        {
            return new SymType("");
        }
        public virtual string Print()
        {
            return "";
        }
    }
    public partial class NodeCast : NodeExpression
    {
        SymType _cast;
        NodeExpression _exp;
        public NodeCast(SymType cast, NodeExpression exp)
        {
            _cast = cast;
            _exp = exp;
        }
        public override string ToString(string prefix)
        {
            string res;
            res = $"{_cast.GetName()}\r\n";
            res += prefix + $"└─── {_exp.ToString(prefix)}";
            return res;
        }
    }
    public class NodeBinOp : NodeExpression
    {
        object _opname;
        NodeExpression _left;
        NodeExpression _right;
        public NodeBinOp(object opname, NodeExpression left, NodeExpression right)
        {
            _opname = opname;
            _left = left;
            _right = right;
            _cached_type = CalcType();
        }
        public override SymType CalcType()
        {
            SymType leftType = _left.GetCachedType();
            SymType rightType = _right.GetCachedType();
            string? opnameStr = _opname.ToString();
            if (_opname.GetType() == typeof(Operation))
            {
                opnameStr = Lexer.convert_sign.FirstOrDefault(x => x.Value.ToString() == (object)_opname.ToString()).Key;
            }
            if (leftType.GetType() != rightType.GetType() && opnameStr != ".")
            {
                if ((leftType.GetType().Name == "SymInteger" || leftType.GetType().Name == "SymReal") &&
                   (rightType.GetType().Name == "SymInteger" || rightType.GetType().Name == "SymReal"))
                {
                    if (leftType.GetType().Name == "SymInteger")
                    {
                        _left = new NodeCast(rightType, _left);
                    }
                    else
                    {
                        _right = new NodeCast(leftType, _right);
                    }
                    if ((opnameStr == "<" || opnameStr == "<=" || opnameStr == ">" || opnameStr == "=>" || opnameStr == "=" || opnameStr == "<>"))
                    {
                        return new SymBoolean("boolean");
                    }
                    return new SymReal("real");
                }
                else
                {
                    throw new Exception($"Incompatible types {_opname.GetType()}");
                }
            }
            if ((leftType.GetType().Name == "SymString" && rightType.GetType().Name == "SymString" && (opnameStr == "/" || opnameStr == "*" || opnameStr == "-")) ||
               ((opnameStr == "or" || opnameStr == "and" || opnameStr == "not") && (leftType.GetType().Name != "SymBoolean" || rightType.GetType().Name != "SymBoolean")))
            {
                throw new Exception("Operator is not overloaded");
            }
            if ((opnameStr == "<" || opnameStr == "<=" || opnameStr == ">" || opnameStr == "=>" || opnameStr == "=" || opnameStr == "<>"))
            {
                return new SymBoolean("boolean");
            }
            return leftType;
        }
        public override string ToString(string prefix)
        {
            string operation = _opname.ToString();
            if (operation != null)
            {
                operation = operation.ToLower();
            }
            if (_opname.GetType() == typeof(Operation))
            {
                operation = Lexer.convert_sign.FirstOrDefault(x => x.Value.ToString() == _opname.ToString()).Key;
            }
            return operation + "\r\n" +
                prefix + $"├─── {_left.ToString(prefix + ChildrenPrefix(true))}\r\n" +
                prefix + $"└─── {_right.ToString(prefix + ChildrenPrefix(false))}";
        }
    }
    public class NodeRecordAccess : NodeBinOp
    {
        public NodeRecordAccess(Operation opname, NodeExpression left, NodeExpression right) : base(opname, left, right) { }
    }
    public class NodeUnOp : NodeExpression
    {
        object _opname;
        NodeExpression _arg;
        public NodeUnOp(object opname, NodeExpression arg)
        {
            _opname = opname;
            _arg = arg;
            _cached_type = CalcType();
        }
        public override SymType CalcType()
        {
            return _arg.CalcType();
        }
        public override string ToString(string prefix)
        {
            string str;
            string? opnameStr = _opname.ToString();
            if (opnameStr != null)
            {
                opnameStr = opnameStr.ToLower();
            }
            if (_opname.GetType() == typeof(Operation))
            {
                opnameStr = Lexer.convert_sign.FirstOrDefault(x => x.Value.ToString() == _opname.ToString()).Key;
            }
            str = $"{opnameStr}\r\n";
            str += prefix + $"└─── {_arg.ToString(prefix + ChildrenPrefix(false))}";
            return str;
        }
    }
    public class NodeArrayElement : NodeExpression
    {
        string _name;
        SymArray _symArray;
        List<NodeExpression?>? _args;
        public NodeArrayElement(string name, SymArray symArray, List<NodeExpression?>? arg)
        {
            _name = name;
            _symArray = symArray;
            _args = arg;
        }
        public override string ToString(string prefix)
        {
            string str = null;
            str += $"{_name}[";
            foreach (NodeExpression arg in _args)
            {
                if (arg != _args.First()) { str += ", "; }
                str += arg.ToString(prefix);
            }
            return str + "]";
        }
    }
    public class NodeVar : NodeExpression
    {
        SymVar _name;
        public string GetName()
        {
            return _name.GetName();
        }
        public NodeVar(SymVar var_)
        {
            this._name = var_;
            _cached_type = CalcType();
        }
        public override SymType CalcType()
        {
            return _name.GetOriginalTypeVar();
        }
        public SymVar GetSymVar()
        {
            return _name;
        }
        public override string ToString(string prefix)
        {
            return _name.ToString(prefix);
        }
    }
    public class NodeInt : NodeExpression
    {
        int _value;
        public NodeInt(int value)
        {
            _value = value;
            _cached_type = CalcType();
        }
        public override SymType CalcType()
        {
            return new SymInteger("integer");
        }
        public override string ToString(string prefix)
        {
            return _value.ToString();
        }
    }
    public class NodeReal : NodeExpression
    {
        double _value;
        public NodeReal(double value)
        {
            _value = value;
            _cached_type = CalcType();
        }
        public override SymType CalcType()
        {
            return new SymReal("real");
        }
        public override string ToString(string prefix)
        {
            return _value.ToString();
        }
    }
    public class NodeString : NodeExpression
    {
        string _value;
        public NodeString(string value)
        {
            _value = value;
            _cached_type = CalcType();
        }
        public override SymType CalcType()
        {
            return new SymString("string");
        }
        public override string ToString(string prefix)
        {
            return $"\"{_value}\"";
        }
    }
}
