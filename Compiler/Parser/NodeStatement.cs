using System.Xml.Linq;

namespace Compiler
{
    public class NodeStatement : Node { }
    public class NullStmt : NodeStatement
    {
        public NullStmt() { }
        public override string ToString(string indent, bool last)
        {
            return "\r\n";
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
        public override string ToString(string indent, bool last)
        {
            //string operation = Lexer.convert_sign.Where(x => x.Value == (object)_name).FirstOrDefault().Key;
            return _name + "\r\n" +
                indent + Prefix(true) + _left.ToString(indent + ChildrenPrefix(true), true) + "\r\n" +
                indent + Prefix(false) + _right.ToString(indent + ChildrenPrefix(false), false) + "\r\n";
        }
    }
    public class BlockStmt : NodeStatement
    {
        List<NodeStatement> _body;
        public BlockStmt(List<NodeStatement> body)
        {
            _body = body;
        }
        public override string ToString(string indent, bool last)
        {
            string str = null;
            str += indent + Prefix(true) + "begin" + "\r\n";
            foreach (NodeStatement body in _body)
            {
                bool prefix = true;
                if (body == _body.Last())
                { 
                    prefix = false;
                }
                str += indent + ChildrenPrefix(true) + Prefix(prefix) + body.ToString(indent + ChildrenPrefix(true) + ChildrenPrefix(prefix), true);
            }
            str += indent + Prefix(false) + "end" + "\r\n";
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
        public override string ToString(string indent, bool last)
        {
            string str = null;
            str = $"{_proc.GetName()}";
            if (_args != null && _args.Count > 0)
            {
                str += $"\r\n";
                int i = 1;
                foreach (NodeExpression? arg in _args)
                {
                    if (i == _args.Count)
                    {
                        if (arg != null)
                        {
                            str += indent + Prefix(false) + arg.ToString(indent + ChildrenPrefix(true), true);
                        }
                    }
                    else
                    {
                        if (arg != null)
                        {
                            str += indent + Prefix(true) + arg.ToString(indent + ChildrenPrefix(true), true);
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
        public override string ToString(string indent, bool last)
        {
            string str = null;
            str += "if \r\n";
            str += indent + Prefix(true) + _condition.ToString(indent + ChildrenPrefix(true), true) + "\r\n";
            str += indent + Prefix(true) + _if_body.ToString(indent + ChildrenPrefix(true), true) + "\r\n";
            str += indent + "else\r\n";
            str += indent + Prefix(false) + _else_body.ToString(indent + ChildrenPrefix(false), true) + "\r\n";
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
        public override string ToString(string indent, bool last)
        {
            string str = null;
            str += "for \r\n";
            str += indent + Prefix(true) + ":=\r\n";
            str += indent + ChildrenPrefix(true) + Prefix(true) + _control_var.ToString(indent + ChildrenPrefix(true), true) + "\r\n";
            str += indent + ChildrenPrefix(true) + Prefix(false) + _start.ToString(indent + ChildrenPrefix(true), true) + "\r\n";
            str += indent + Prefix(true) + _keyword.ToString().ToLower() + "\r\n";
            str += indent + ChildrenPrefix(true) + Prefix(false) + _finalval.ToString(indent + ChildrenPrefix(true), true) + "\r\n";
            str += indent + Prefix(false) + _body.ToString(indent + ChildrenPrefix(false), true) + "\r\n";
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
        public override string ToString(string indent, bool last)
        {
            string str = null;
            str += "while \r\n";
            str += indent + Prefix(true) + _condition.ToString(indent + ChildrenPrefix(true), true) + "\r\n";
            str += indent + Prefix(false) + _body.ToString(indent + ChildrenPrefix(false), true);
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
        public override string ToString(string indent, bool last)
        {
            string str = null;
            str = "repeat\r\n";
            foreach (NodeStatement? stmt in _body)
            {
                if (stmt == _body.Last())
                {
                    str += indent + Prefix(true) + stmt.ToString(indent + ChildrenPrefix(true), true);
                } else
                {
                    str += indent + Prefix(true) + stmt.ToString(indent + ChildrenPrefix(true), true) + "\r\n";
                }
            }
            str += indent + Prefix(false) + _condition.ToString(indent + ChildrenPrefix(false), true) + "\r\n";
            return str;
        }
    }
}
