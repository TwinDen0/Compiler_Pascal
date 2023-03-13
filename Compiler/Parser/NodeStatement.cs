using System.Xml.Linq;

namespace Compiler
{
    public class NodeStatement : Node { }
    public class NullStmt : NodeStatement
    {
        public NullStmt() { }
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
                indent + NodePrefix(true) + _left.ToString(indent + ChildrenPrefix(true), true) + "\r\n" +
                indent + NodePrefix(false) + _right.ToString(indent + ChildrenPrefix(false), false) + "\r\n";
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
            str += indent + NodePrefix(true) + "begin" + "\r\n";
            foreach (NodeStatement body in _body)
            {
                bool prefix = true;
                if (body == _body.Last())
                { 
                    prefix = false;
                }
                str += indent + ChildrenPrefix(true) + NodePrefix(prefix) + body.ToString(indent + ChildrenPrefix(true) + ChildrenPrefix(prefix), true);
            }
            str += indent + NodePrefix(false) + "end" + "\r\n";
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
                            str += indent + NodePrefix(false) + arg.ToString(indent + ChildrenPrefix(true), true);
                        }
                    }
                    else
                    {
                        if (arg != null)
                        {
                            str += indent + NodePrefix(true) + arg.ToString(indent + ChildrenPrefix(true), true);
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
            //string prefix = GetPrefixNode(isLeftParents);
            str += "if \r\n";
            str += indent + NodePrefix(true) + _condition.ToString(indent + ChildrenPrefix(true), true) + "\r\n";
            str += indent + NodePrefix(true) + _if_body.ToString(indent + ChildrenPrefix(true), true) + "\r\n";
            str += indent + "else\r\n";
            str += indent + NodePrefix(false) + _else_body.ToString(indent + ChildrenPrefix(false), true) + "\r\n";
            return str;
        }
    }
    public class ForStmt : NodeStatement
    {
        NodeVar _controlvar;
        NodeExpression _start;
        KeyWord _keyword;
        NodeExpression _finalval;
        NodeStatement _body;
        public ForStmt(NodeVar controlVar, NodeExpression initialVal, KeyWord toOrDownto, NodeExpression finalVal, NodeStatement body)
        {
            _controlvar = controlVar;
            _start = initialVal;
            _keyword = toOrDownto;
            _finalval = finalVal;
            _body = body;
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
    }
}
