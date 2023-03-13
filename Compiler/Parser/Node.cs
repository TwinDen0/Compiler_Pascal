using System.Collections.Generic;
using System;

namespace Compiler
{
    public class Node
    {
        public virtual string ToString()
        {
            return "node";
        }

        public virtual string ToString(string indent, bool last)
        {
            return "";
        }

        public static string NodePrefix(bool last)
        {
            return last ? "├─── " : "└─── ";
        }

        public static string ChildrenPrefix(bool last)
        {
            return last ? "│    " : "     ";
        }
    }

    public class NodeMainProgram : Node
    {
        string _name;
        List<NodeDefs> _types;
        BlockStmt _body;
        public NodeMainProgram(string name, List<NodeDefs> types, BlockStmt body)
        {
            _name = name;
            _types = types;
            _body = body;
        }
        public override string ToString()
        {
            string str = null;
            str = _name + "\r\n";
            foreach (NodeDefs type in _types)
            {
                str += type.ToString(ChildrenPrefix(true), true);
            }
            str += _body.ToString(null, true) + "\r\n";
            return str;
        }
    }
}
