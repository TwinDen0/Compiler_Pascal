using Compiler.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Parser
{
    public enum NodeType
    {
        Number,
        Identifier,
        BinOperation,
        Error
    }
    public class Node
    {
        public NodeType type;
        public object value;
        public List<Node?>? children;
    }
    public class Parser
    {
        public LexemeData CurrectLexem;
        public Lexer.Lexer lexer;
        int notClosedBracket;
        public Node resParser;

        public Parser(string filePath)
        {
            CurrectLexem = null;
            lexer = new Lexer.Lexer(filePath);
            notClosedBracket = 0;
            resParser = new Node();
        }
        
        public Node GetParser()
        {
            resParser = new Node();

            CurrectLexem = lexer.GetLexeme();

            resParser = ExpSimple();

            if (CurrectLexem.LexemeType == LexemeType.ERROR)
            {
                resParser = new Node()
                {
                    type = NodeType.Error,
                    value = $"({lexer.lineNum},{lexer.symbolNum - 1}) Fatal: Syntax error, \";\" expected but \"{CurrectLexem.LexemeValue}\" found"
                };
            }

            lexer.lineNum = 1;
            lexer.symbolNum = 0;
            return resParser;
        }

        public Node ExpSimple()
        {
        Node leftСhild = Term();

        while ((CurrectLexem.LexemeValue.ToString() == "+" || CurrectLexem.LexemeValue.ToString() == "-") && CurrectLexem.LexemeType == LexemeType.OPERATOR)
        {
            var operation = CurrectLexem.LexemeValue;

            if (CurrectLexem.LexemeType != LexemeType.ENDFILE)
            {
                CurrectLexem = lexer.GetLexeme();
            }
            Node rightСhild = Term();
            leftСhild = new Node()
            {
                type = NodeType.BinOperation,
                value = operation,
                children = new List<Node?> { leftСhild, rightСhild }
            };

            if (rightСhild.type == NodeType.Error)
            {
                return new Node()
                {
                    type = NodeType.Error,
                    value = rightСhild.value
                };
            }
        }

        return leftСhild;
    }

        public Node Term()
        {
            Node leftСhild = Factor();

            if (leftСhild.type != NodeType.Error)
            {
                CurrectLexem = lexer.GetLexeme();
                while ((CurrectLexem.LexemeValue.ToString() == "*" || CurrectLexem.LexemeValue.ToString() == "/") && CurrectLexem.LexemeType == LexemeType.OPERATOR)
                {
                    var operation = CurrectLexem.LexemeValue;
                    if (CurrectLexem.LexemeType != LexemeType.ENDFILE)
                    {
                        CurrectLexem = lexer.GetLexeme();
                    }
                    Node rightСhild = Factor();
                    if (CurrectLexem.LexemeType != LexemeType.ENDFILE)
                    {
                        CurrectLexem = lexer.GetLexeme();
                    }

                    leftСhild = new Node()
                    {
                        type = NodeType.BinOperation,
                        value = operation,
                        children = new List<Node?> { leftСhild, rightСhild }
                    };

                    if (rightСhild.type == NodeType.Error)
                    {
                        return new Node()
                        {
                            type = NodeType.Error,
                            value = rightСhild.value
                        };
                    }
                }
            }
            return leftСhild;
        }

        public Node Factor()
        {

            if (CurrectLexem.LexemeType == LexemeType.INTEGER || CurrectLexem.LexemeType == LexemeType.REAL)
            {
                LexemeData factor = CurrectLexem;

                return new Node()
                {
                    type = NodeType.Number,
                    value = factor.LexemeValue
                };
            }
            if (CurrectLexem.LexemeType == LexemeType.IDENTIFIER)
            {
                LexemeData factor = CurrectLexem;

                return new Node()
                {
                    type = NodeType.Identifier,
                    value = factor.LexemeValue
                };
            }
            if (CurrectLexem.LexemeValue.ToString() == "(" && CurrectLexem.LexemeType == LexemeType.SEPARATOR)
            {
                Node newExp = new Node();
                if (CurrectLexem.LexemeType != LexemeType.ENDFILE)
                {
                    CurrectLexem = lexer.GetLexeme();
                    newExp = ExpSimple();
                }
                else
                {
                    newExp = new Node()
                    {
                        type = NodeType.Error,
                        value = $"({lexer.lineNum},{lexer.symbolNum - 1}) Fatal: Syntax error, \")\" expected"
                    };
                }

                if (CurrectLexem.LexemeValue.ToString() != ")" || CurrectLexem.LexemeType != LexemeType.SEPARATOR)
                {
                    if (CurrectLexem.LexemeType != LexemeType.ENDFILE)
                    {
                        notClosedBracket += 1;
                    }
                    else
                    {
                        newExp = new Node()
                        {
                            type = NodeType.Error,
                            value = $"({lexer.lineNum},{lexer.symbolNum - 1}) Fatal: Syntax error, \")\" expected"
                        };
                    }
                }
                return newExp;
            }

            return new Node()
            {
                type = NodeType.Error,
                value = $"({lexer.lineNum},{lexer.symbolNum - 1}) Fatal: Syntax error, don't have factor"
            };
        }
    }
}
