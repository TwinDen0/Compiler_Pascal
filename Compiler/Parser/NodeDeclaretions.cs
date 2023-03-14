using System;
using System.Data;
using System.Xml.Linq;

namespace Compiler
{
    public class NodeDefs : Node { }
    public class VarTypesNode : NodeDefs
    {
        List<VarDeclarationNode> _body;
        public VarTypesNode(List<VarDeclarationNode> body)
        {
            _body = body;
        }
        public override string ToString(string indent, bool last)
        {
            string str = null;
            str += Prefix(last) + "var" + "\r\n";
            foreach (VarDeclarationNode body in _body) 
            {
                bool pref = true;
                if (body == _body.Last())
                {
                    pref = false;
                }
                str += indent + Prefix(pref) + body.ToString(indent + ChildrenPrefix(pref), pref);
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
        public override string ToString(string indent, bool last)
        {
            string str = null;
            str += Prefix(last) + "const" + "\r\n";
            foreach (ConstDeclarationNode body in _body)
            {
                bool pref = true;
                if (body == _body.Last())
                {
                    pref = false;
                }
                str += indent + Prefix(pref) + body.ToString(indent + ChildrenPrefix(pref), pref);
            }
            return str;
        }
    }
    public class TypeTypesNode : NodeDefs
    {
        List<DeclarationNode> _body;
        public TypeTypesNode(List<DeclarationNode> body)
        {
            _body = body;
        }
        public override string ToString(string indent, bool last)
        {
            string str = null;
            str += Prefix(last) + "type" + "\r\n";
            foreach (DeclarationNode body in _body)
            {
                bool pref = true;
                if (body == _body.Last())
                {
                    pref = false;
                }
                str += indent + Prefix(pref) + body.ToString(indent + ChildrenPrefix(pref), pref);
            }
            return str;
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
        public override string ToString(string indent, bool last)
        {
            string str = null;
            str += Prefix(last) + "procedure " + _symProc.ToString() + "\r\n" +
                indent + Prefix(true) + "parameters" + "\r\n";
            foreach (VarDeclarationNode param in _params)
            {
                bool pref = true;
                if (param == _params.Last())
                {
                    pref = false;
                }
                str += indent + ChildrenPrefix(last) + Prefix(pref) + param.ToString(indent + ChildrenPrefix(last) + ChildrenPrefix(pref), pref);
            }
            str += indent + Prefix(true) + "local_types" + "\r\n";
            foreach (NodeDefs local_type in _localsTypes)
            {
                bool pref = true;
                if (local_type == _localsTypes.Last())
                {
                    pref = false;
                }
                str += indent + ChildrenPrefix(last) + local_type.ToString(indent + ChildrenPrefix(last) + ChildrenPrefix(pref), pref);
            }
            str += _symProc.GetBody().ToString(indent, last);
            return str;
        }
    }

    public class DeclarationNode : Node { }
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
        public override string ToString(string indent, bool last)
        {
            string str = _type.ToString(indent + ChildrenPrefix(false), true) + "\r\n";
            if (_value == null)
            {
                foreach (SymVar name in _vars_name)
                {
                    bool pref = true;
                    if (name == _vars_name.Last())
                    {
                        pref = false;
                    }
                    str += indent + Prefix(pref) + name.ToString() + "\r\n";
                }
            } 
            else
            {
                str += indent + Prefix(true) + _vars_name[0].ToString() + "\r\n" +
                    indent + Prefix(false) + "=\r\n" +
                    indent + ChildrenPrefix(false) + Prefix(false) + _value.ToString(indent + ChildrenPrefix(false), true) + "\r\n";
            }
            return str;
        }
    }
    public class ConstDeclarationNode : DeclarationNode
    {
        string _name;
        NodeExpression _value;
        public ConstDeclarationNode(string name, NodeExpression value)
        {
            _name = name;
            _value = value;
        }
        public override string ToString(string indent, bool last)
        {
            return "=" + "\r\n" + 
                indent + Prefix(true) + _name.ToString() + "\r\n" +
                indent + Prefix(false) + _value.ToString(indent + ChildrenPrefix(false), true) + "\r\n";
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
        public override string ToString(string indent, bool last)
        {
            return "=" + "\r\n" +
                indent + Prefix(true) + _name.ToString() + "\r\n" +
                indent + Prefix(false) + _type.ToString(indent, false) + "\r\n";
        }
    }
}
