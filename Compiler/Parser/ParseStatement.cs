using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Compiler
{
    public partial class Parser
    {
        // statement ::= simple_stmt | struct_stmt
        public NodeStatement ParseStatement()
        {
            NodeStatement res = new NullStmt();
            if (Expect(LexemeType.IDENTIFIER)||Expect(KeyWord.EXIT))
            {
                res = ParseSimpleStmt();
                return res;
            }
            res = ParseStructStmt();
            return res;
        }

        // simple_stmt ::= "exit" | call | assignment
        public NodeStatement ParseSimpleStmt()
        {
            if (Expect(KeyWord.EXIT))
            {
                GetNextLexeme();
                return new CallStmt((SymProc)symTableStack.Get("exit"), null);
            }

            Lexeme id = current_lexeme;
            GetNextLexeme();
            if (Expect(Separator.OPEN_PARENTHESIS))
            {
                return ParseCallStmt(id);
            }
            else
            {
                return ParseAssignmentStmt(id);
            }
        }

        // call ::= id "(" [simple_factor { "," simple_factor }] ")"
        public CallStmt ParseCallStmt(Lexeme id)
        {
            List<NodeExpression?> parameter = new List<NodeExpression?>();
            SymProc proc = (SymProc)symTableStack.Get(id.LexemeValue.ToString());
            GetNextLexeme();
            while (!Expect(Separator.CLOSE_PARENTHESIS))
            {
                parameter.Add(ParseFactor());
                if (Expect(Separator.COMMA))
                {
                    GetNextLexeme();
                }
                else
                {
                    break;
                }
            }
            Require(Separator.CLOSE_PARENTHESIS);
            if (proc.GetCountParams() != -1 && parameter.Count != proc.GetCountParams())
            {
                throw new ExceptionCompiler($"({id.NumbSymbol}, {id.NumbLine}) Wrong number of parameters specified for call to \"{id.LexemeValue.ToString()}\"");
            }
            return new CallStmt(proc, parameter);
        }

        // assignment ::= id {":=" | "+=" | "-=" | "*=" | "/="} (expression | string)
        public AssignmentStmt ParseAssignmentStmt(Lexeme id)
        {
            string operation;
            NodeExpression left;
            NodeExpression right;
            Symbol s = symTableStack.Get((string)id.LexemeValue);
            SymVar symVar = (SymVar)s;

            left = new NodeVar(symVar);
            switch (current_lexeme.LexemeValue)
            {
                case Separator.OPEN_BRACKET:
                    GetNextLexeme();
                    left = ParseArrayElement(left, ref symVar);
                    break;
                case Separator.POINT:
                    left = ParseRecordElement(left, ref symVar);
                    break;
            }
            operation = current_lexeme.LexemeSource;
            GetNextLexeme();
            right = ParseExpression();
            return new AssignmentStmt(operation, left, right);
        }

        //  array_element ::= id "[" simple_expression "]"
        public NodeExpression ParseArrayElement(NodeExpression node, ref SymVar var_)
        {
            SymArray array = new SymArray("", new List<OrdinalTypeNode>(), new SymInteger(""));
            List<NodeExpression> body = new List<NodeExpression>();
            bool bracketClose = false;
            bool end = false;
            body.Add(ParseSimpleExpression());
            while (Expect(Separator.COMMA, Separator.CLOSE_BRACKET))
            {
                if (end)
                {
                    break;
                }
                switch (current_lexeme.LexemeValue)
                {
                    case Separator.CLOSE_BRACKET:
                        array = (SymArray)((NodeVar)node).GetSymVar().GetOriginalTypeVar();
                        var_ = new SymVar(var_.GetName(), array.GetArrayType());
                        bracketClose = true;
                        GetNextLexeme();
                        if (Expect(Separator.OPEN_BRACKET))
                        {
                            bracketClose = false;
                            GetNextLexeme();
                            body.Add(ParseSimpleExpression());
                        }
                        break;
                    case Separator.COMMA:
                        if (!bracketClose)
                        {
                            GetNextLexeme();
                            body.Add(ParseSimpleExpression());
                        }
                        else
                        {
                            end = true;
                        }
                        break;
                }
            }
            if (!bracketClose)
            {
                throw new ExceptionCompiler($"({current_lexeme.NumbSymbol}, {current_lexeme.NumbLine}) expected ']'");
            }
            return new NodeArrayElement(var_.GetName(), array, body);
        }

        //  record_element ::= id "." id
        public NodeExpression ParseRecordElement(NodeExpression node, ref SymVar var_)
        {
            GetNextLexeme();
            if (!Expect(LexemeType.IDENTIFIER))
            {
                throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) expected Identifier");
            }
            SymRecord record = (SymRecord)var_.GetOriginalTypeVar();
            SymTable fields = record.GetFields();
            var_ = (SymVar)fields.Get((string)current_lexeme.LexemeValue);
            NodeExpression field = new NodeVar(var_);
            GetNextLexeme();
            return new NodeRecordAccess(Operation.POINT_RECORD, node, field);
        }

        //  structStmt ::=  if | for | while | repeat | block
        public NodeStatement ParseStructStmt()
        {
            NodeStatement res = new NullStmt();
            switch (current_lexeme.LexemeValue)
            {
                case KeyWord.BEGIN:
                    res = ParseBlock();
                    break;
                case KeyWord.IF:
                    res = ParseIf();
                    break;
                case KeyWord.FOR:
                    res = ParseFor();
                    break;
                case KeyWord.WHILE:
                    res = ParseWhile();
                    break;
                case KeyWord.REPEAT:
                    res = ParseRepeat();
                    break;
                case Separator.SEMICOLON:
                    break;
                default:
                    throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) expected statement");
            }
            return res;
        }

        //  if ::= "if" expression "then" statement["else" statement]
        public NodeStatement ParseIf()
        {
            NodeExpression condition;
            NodeStatement if_body = new NullStmt();
            NodeStatement else_body = new NullStmt();

            GetNextLexeme();
            condition = ParseExpression();
            Require(KeyWord.THEN);
            if (!Expect(KeyWord.ELSE))
            {
                if_body = ParseStatement();
            }
            if (Expect(KeyWord.ELSE))
            {
                GetNextLexeme();
                else_body = ParseStatement();
            }
            return new IfStmt(condition, if_body, else_body);
        }

        //  for ::= "for" id ":=" simple_expression "to" simple_expression "do" statement
        public NodeStatement ParseFor()
        {
            KeyWord to_downto;
            NodeVar control_var;
            NodeExpression start;
            NodeExpression final_value;
            NodeStatement? body;
            GetNextLexeme();
            Require(LexemeType.IDENTIFIER);
            control_var = new NodeVar((SymVar)symTableStack.Get((string)current_lexeme.LexemeValue));
            GetNextLexeme();
            Require(Operation.ASSIGNMENT);
            start = ParseSimpleExpression();
            if (!Expect(KeyWord.TO, KeyWord.DOWNTO))
            {
                throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) expected 'to' or 'downto'");
            }
            to_downto = (KeyWord)current_lexeme.LexemeValue;
            GetNextLexeme();
            final_value = ParseSimpleExpression();
            Require(KeyWord.DO);
            body = ParseStatement();
            return new ForStmt(control_var, start, to_downto, final_value, body);
        }

        //  while ::= "while" expression "do" statement
        public NodeStatement ParseWhile()
        {
            NodeExpression condition;
            NodeStatement body;

            GetNextLexeme();
            condition = ParseExpression();
            Require(KeyWord.DO);
            body = ParseStatement();
            return new WhileStmt(condition, body);
        }

        //  repeat ::= "repeat" { statement ";"} "until" expression
        public NodeStatement ParseRepeat()
        {
            NodeExpression condition;
            List<NodeStatement> body = new List<NodeStatement>();
            do
            {
                GetNextLexeme();
                if (Expect(KeyWord.UNTIL))
                {
                    break;
                }
                body.Add(ParseStatement());
            } while (Expect(Separator.SEMICOLON));
            Require(KeyWord.UNTIL);
            condition = ParseExpression();
            return new RepeatStmt(condition, body);
        }
    }
}
