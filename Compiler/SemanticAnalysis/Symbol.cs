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
        string name;
        public Symbol(string name)
        {
            this.name = name;
        }
        public string GetName()
        {
            return name;
        }
        public override string ToString()
        {
            return name;
        }
    }
    public class SymVar : Symbol
    {
        SymType type;
        public SymType GetTypeVar()
        {
            return type;
        }
        public SymType GetOriginalTypeVar()
        {
            SymType buildsType = type;
            while (buildsType.GetType().GetType() == typeof(SymTypeAlias))
            {
                SymTypeAlias symTypeAlias = (SymTypeAlias)buildsType;
                buildsType = symTypeAlias.GetOriginalType();
            }
            return buildsType;
        }
        public SymVar(string name, SymType type) : base(name)
        {
            this.type = type;
        }
    }
    public class SymVarConst : SymVar
    {
        public SymVarConst(string name, SymType type) : base(name, type) { }

    }
    public class SymVarGlobal : SymVar
    {
        public SymVarGlobal(string name, SymType type) : base(name, type) { }
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
        bool unlimitedParameters = false;
        SymTable _params;
        SymTable _locals;
        BlockStmt _body;
        public int GetCountParams()
        {
            if (unlimitedParameters)
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
        public SymProc(string name, SymTable params_, SymTable locals, BlockStmt body) : base(name)
        {
            _params = params_;
            _locals = locals;
            _body = body;
        }
        public SymProc(string name) : base(name)
        {
            unlimitedParameters = true;
            _params = new SymTable(new Dictionary<string, Symbol>());
            _locals = new SymTable(new Dictionary<string, Symbol>());
            _body = new BlockStmt(new List<NodeStatement>());
        }
    }
}
