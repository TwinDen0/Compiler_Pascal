﻿using System.Collections.Generic;
using System;

namespace Compiler
{
    public class Node
    {
        public virtual string ToString(string prefix)
        {
            return "";
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
            if(_name != null)
            {
                str = _name + "\r\n";
            }
            else
            {
                str = "program\r\n";
            }
            foreach (NodeDefs type in _types)
            {
                str += $"├─── {type.ToString(ChildrenPrefix(true))}";
            }
            str += _body.ToString(null) + "\r\n";
            return str;
        }
    }
}
