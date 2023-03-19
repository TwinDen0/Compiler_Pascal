using System.Xml.Linq;

namespace Compiler
{
    public class NodeStatement : Node { }
    public class NullStmt : NodeStatement
    {
        public NullStmt() { }
        public override string ToString(string prefix)
        {
            return "";
        }
    }
    public class AssignmentStmt : NodeStatement
    {
        string _name;
        NodeExpression _left;
        NodeExpression _right;
        public AssignmentStmt(string opname, NodeExpression left, NodeExpression right)
        {
            _name = opname;
            _left = left;
            _right = right;

        }
        public override string ToString(string prefix)
        {
            return _name + "\r\n" +
                prefix + $"├─── {_left.ToString(prefix + ChildrenPrefix(true))}\r\n" +
                prefix + $"└─── {_right.ToString(prefix + ChildrenPrefix(false))}";
        }
    }
    public class BlockStmt : NodeStatement
    {
        List<NodeStatement> _body;
        public BlockStmt(List<NodeStatement> body)
        {
            _body = body;
        }
        public override string ToString(string prefix)
        {
            string str = null;
            str += prefix + "├─── begin\r\n";
            foreach (var body in _body)
            {
                if (body != _body.Last())
                {
                    str += prefix + ChildrenPrefix(true) + $"├─── {body.ToString(prefix + ChildrenPrefix(true) + ChildrenPrefix(true))}\r\n";
                }
                else
                {
                    str += prefix + ChildrenPrefix(true) + $"└─── {body.ToString(prefix + ChildrenPrefix(true) + ChildrenPrefix(false))}\r\n";
                }
            }
            str += prefix + "└─── end\r\n";
            return str;
        }
    }
    public class CallStmt : NodeStatement
    {
        SymProc _proc;
        List<NodeExpression?>? _args;
        public CallStmt(SymProc proc, List<NodeExpression?>? arg)
        {
            _proc = proc;
            _args = arg;
        }
        public override string ToString(string prefix)
        {
            string str = null;
            str = $"{_proc.GetName()}";
            if (_args != null && _args.Count > 0)
            {
                str += "\r\n";
                int i = 1;
                foreach (NodeExpression? arg in _args)
                {
                    if (i == _args.Count)
                    {
                        if (arg != null)
                        {
                            str += prefix + $"└─── {arg.ToString(prefix + ChildrenPrefix(true))}";
                        }
                    }
                    else
                    {
                        if (arg != null)
                        {
                            str += prefix + $"├─── {arg.ToString(prefix + ChildrenPrefix(true))}\r\n";
                        }
                        i++;
                    }
                }
            }
            return str;
        }
    }
    public class IfStmt : NodeStatement
    {
        NodeExpression _condition;
        NodeStatement _if_body;
        NodeStatement _else_body;
        public IfStmt(NodeExpression condition, NodeStatement if_body, NodeStatement else_Body)
        {
            _condition = condition;
            _if_body = if_body;
            _else_body = else_Body;
        }
        public override string ToString(string prefix)
        {
            string str = null;
            str += "if \r\n";
            str += prefix + $"├─── {_condition.ToString(prefix + ChildrenPrefix(true))}\r\n";
            str += prefix + $"├─── {_if_body.ToString(prefix + ChildrenPrefix(true))}\r\n";
            str += prefix + "else\r\n";
            str += prefix + $"└─── {_else_body.ToString(prefix + ChildrenPrefix(false))}";
            return str;
        }
    }
    public class ForStmt : NodeStatement
    {
        NodeVar _control_var;
        NodeExpression _start;
        KeyWord _keyword;
        NodeExpression _finalval;
        NodeStatement _body;
        public ForStmt(NodeVar controlVar, NodeExpression initial_val, KeyWord toOrDownto, NodeExpression finalVal, NodeStatement body)
        {
            _control_var = controlVar;
            _start = initial_val;
            _keyword = toOrDownto;
            _finalval = finalVal;
            _body = body;
        }
        public override string ToString(string prefix)
        {
            string str = null;
            str += "for \r\n";
            str += prefix +  "├─── :=\r\n";
            str += prefix + ChildrenPrefix(true) + $"├─── {_control_var.ToString(prefix + ChildrenPrefix(true))}\r\n";
            str += prefix + ChildrenPrefix(true) + $"└─── {_start.ToString(prefix + ChildrenPrefix(true))}\r\n";
            str += prefix + $"├─── {_keyword.ToString().ToLower()}\r\n";
            str += prefix + ChildrenPrefix(true) + $"└─── {_finalval.ToString(prefix + ChildrenPrefix(true))}\r\n";
            str += prefix + $"└─── {_body.ToString(prefix + ChildrenPrefix(false))}";
            return str;
        }
    }
    public class WhileStmt : NodeStatement
    {
        NodeExpression _condition;
        NodeStatement _body;
        public WhileStmt(NodeExpression condition, NodeStatement body)
        {
            _condition = condition;
            _body = body;
        }
        public override string ToString(string prefix)
        {
            string str = null;
            str += "while \r\n";
            str += prefix + $"├─── {_condition.ToString(prefix + ChildrenPrefix(true))}\r\n";
            str += prefix + $"└─── {_body.ToString(prefix + ChildrenPrefix(false))}";
            return str;
        }
    }
    public class RepeatStmt : NodeStatement
    {
        NodeExpression _condition;
        List<NodeStatement> _body;
        public RepeatStmt(NodeExpression condition, List<NodeStatement> body)
        {
            _condition = condition;
            _body = body;
        }
        public override string ToString(string prefix)
        {
            string str = null;
            str = "repeat\r\n";
            foreach (NodeStatement? stmt in _body)
            {
                if (stmt == _body.Last())
                {
                    str += prefix + $"├─── {stmt.ToString(prefix + ChildrenPrefix(true))}\r\n";
                } else
                {
                    str += prefix + $"├─── {stmt.ToString(prefix + ChildrenPrefix(true))}\r\n";
                }
            }
            str += prefix + $"└─── {_condition.ToString(prefix + ChildrenPrefix(false))}";
            return str;
        }
    }
}
