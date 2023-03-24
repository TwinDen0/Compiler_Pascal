using System;
using System.Data;
using System.Xml.Linq;

namespace Compiler
{
    public class NodeDefs : Node { }
    public class DeclarationNode : Node { }

    public class VarTypesNode : NodeDefs
    {
        List<VarDeclarationNode> _body;
        public VarTypesNode(List<VarDeclarationNode> body)
        {
            _body = body;
        }
        public override string ToString(string prefix)
        {
            string str = null;
            str += "var" + "\r\n";
            foreach (VarDeclarationNode body in _body) 
            {
                if (body != _body.Last())
                {
                    str += prefix + $"├─── {body.ToString(prefix + ChildrenPrefix(true))}";
                } 
                else
                {
                    str += prefix + $"└─── {body.ToString(prefix + ChildrenPrefix(false))}";
                }
            }
            return str;
        }
    }
    public class VarDeclarationNode : DeclarationNode
    {
        List<SymVar> _vars_name;
        SymType _type;
        NodeExpression? _value = null;
        public VarDeclarationNode(List<SymVar> name, SymType type, NodeExpression? value)
        {
            _vars_name = name;
            _type = type;
            _value = value;
        }
        public List<SymVar> GetVars()
        {
            return _vars_name;
        }
        public NodeExpression? GetValue()
        {
            return _value;
        }
        public override string ToString(string prefix)
        {
            string str = _type.ToString(prefix + ChildrenPrefix(true)) + "\r\n";
            if (_value == null)
            {
                foreach (SymVar name in _vars_name)
                {
                    if (name != _vars_name.Last())
                    {
                        str += prefix + $"├─── {name.GetName()}\r\n";
                    }
                    else
                    {
                        str += prefix + $"└─── {name.GetName()}\r\n";
                    }
                }
            }
            else
            {
                str += prefix + $"├─── {_vars_name[0].GetName()}\r\n" +
                       prefix + "└─── =\r\n" +
                       prefix + $"     └─── {_value.ToString(prefix + ChildrenPrefix(false))}\r\n";
            }
            return str;
        }
    }

    public class ConstTypesNode : NodeDefs
    {
        List<ConstDeclarationNode> _body;
        public ConstTypesNode(List<ConstDeclarationNode> body)
        {
            _body = body;
        }
        public override string ToString(string prefix)
        {
            string str = null;
            str += "const" + "\r\n";
            foreach (ConstDeclarationNode body in _body)
            {
                if (body != _body.Last())
                {
                    str += prefix + $"├─── {body.ToString(prefix + ChildrenPrefix(true))}";
                }
                else
                {
                    str += prefix + $"└─── {body.ToString(prefix + ChildrenPrefix(false))}";
                }
            }
            return str;
        }
    }
    public class ConstDeclarationNode : DeclarationNode
    {
        SymVarConst _var;
        NodeExpression _value;
        public ConstDeclarationNode(SymVarConst var, NodeExpression value)
        {
            this._var = var;
            this._value = value;
            if ((value.GetCachedType().GetType() != typeof(SymInteger)) &&
                (value.GetCachedType().GetType() != typeof(SymReal)) &&
                (value.GetCachedType().GetType() != typeof(SymString)))
            {
                throw new Exception($"Incompatible types");
            }
        }
        public override string ToString(string prefix)
        {
            return "=" + "\r\n" +
                prefix + $"├─── {_var.GetName()} \r\n" +
                prefix + $"└─── {_value.ToString(prefix + ChildrenPrefix(false))} \r\n";
        }
    }

    public class TypeTypesNode : NodeDefs
    {
        List<DeclarationNode> _body;
        public TypeTypesNode(List<DeclarationNode> body)
        {
            _body = body;
        }
        public override string ToString(string prefix)
        {
            string str = null;
            str += "type" + "\r\n";
            foreach (var body in _body)
            {
                if (body != _body.Last())
                {
                    str += prefix + $"├─── {body.ToString(prefix + ChildrenPrefix(true))}";
                }
                else
                {
                    str += prefix + $"└─── {body.ToString(prefix + ChildrenPrefix(false))}";
                }
            }
            return str;
        }
    }
    public class TypeDeclarationNode : DeclarationNode
    {
        string _name;
        SymTypeAlias _type;
        public TypeDeclarationNode(string name, SymTypeAlias type)
        {
            _name = name;
            _type = type;
        }
        public override string ToString(string prefix)
        {
            return "=" + "\r\n" +
                prefix + $"├─── {_name.ToString()} \r\n" +
                prefix + $"└─── {_type.ToString(prefix)} \r\n";
        }
    }

    public class ProcedureTypesNode : NodeDefs
    {
        List<VarDeclarationNode> _params;
        List<NodeDefs> _localsTypes;
        SymProc _symProc;
        public ProcedureTypesNode(List<VarDeclarationNode> params_, List<NodeDefs> localsTypes, SymProc symProc)
        {
            _params = params_;
            _localsTypes = localsTypes;
            _symProc = symProc;
        }
        public override string ToString(string prefix)
        {
            string str = null;
            str += "procedure " + _symProc.GetName() + "\r\n" +
                   prefix + "├─── parameters" + "\r\n";
            foreach (VarDeclarationNode param in _params)
            {
                if (param != _params.Last())
                {
                    str += prefix + ChildrenPrefix(true) + $"├─── {param.ToString(prefix + ChildrenPrefix(true) + ChildrenPrefix(true))}";
                }
                else
                {
                    str += prefix + ChildrenPrefix(true) + $"└─── {param.ToString(prefix + ChildrenPrefix(true) + ChildrenPrefix(false))}";
                }
            }
            str += prefix + "├─── local_types" + "\r\n";
            foreach (NodeDefs local_type in _localsTypes)
            {
                if (local_type != _localsTypes.Last())
                {
                    str += prefix + ChildrenPrefix(true) + $"├─── {local_type.ToString(prefix + ChildrenPrefix(true) + ChildrenPrefix(true))}";
                }
                else
                {
                    str += prefix + ChildrenPrefix(true) + $"└─── {local_type.ToString(prefix + ChildrenPrefix(true) + ChildrenPrefix(false))}";
                }
            }
            str += _symProc.GetBody().ToString(prefix);
            return str;
        }
    }
}
