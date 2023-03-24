using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Compiler
{
    public class Symbol : Node
    {
        string _name;
        public Symbol(string name)
        {
            _name = name;
        }
        public string GetName()
        {
            return _name;
        }
        public override string ToString(string prefix)
        {
            return _name;
        }
    }
    public class SymVar : Symbol
    {
        SymType _type;
        public SymType GetTypeVar()
        {
            return _type;
        }
        public SymType GetOriginalTypeVar()
        {
            SymType buildsType = _type;
            while (buildsType.GetType().Name == "SymTypeAlias")
            {
                SymTypeAlias symTypeAlias = (SymTypeAlias)buildsType;
                buildsType = symTypeAlias.GetOriginalType();
            }
            return buildsType;
        }
        public SymVar(string name, SymType type) : base(name)
        {
            _type = type;
        }
    }
    public class SymVarConst : SymVar
    {
        public NodeExpression _value;
        public SymVarConst(string name, SymType type, NodeExpression value) : base(name, type)
        {
            _value = value;
        }

    }
    public class SymParamVar : SymVar
    {
        public SymParamVar(SymVar var) : base("var " + var.GetName(), var.GetOriginalTypeVar()) { }
    }
    public class SymParamOut : SymVar
    {
        public SymParamOut(SymVar var) : base("out " + var.GetName(), var.GetOriginalTypeVar()) { }
    }
    public class SymProc : Symbol
    {
        bool _standart_proc = false;
        SymTable _params;
        SymTable _locals;
        BlockStmt _body;
        public int GetCountParams()
        {
            if (_standart_proc)
            {
                return -1;
            }
            else
            {
                return _params.GetSize();
            }
        }
        public BlockStmt GetBody()
        {
            return _body;
        }
        public SymTable GetLocals()
        {
            return _locals;
        }
        public SymProc(string name, SymTable params_, SymTable locals, BlockStmt body) : base(name)
        {
            _params = params_;
            _locals = locals;
            _body = body;
        }
        public SymProc(string name) : base(name)
        {
            _standart_proc = true;
            _params = new SymTable(new Dictionary<string, Symbol>());
            _locals = new SymTable(new Dictionary<string, Symbol>());
            _body = new BlockStmt(new List<NodeStatement>());
        }
    }
}
