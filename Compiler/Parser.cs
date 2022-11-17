﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
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
        public string value = "";
        public List<Node?>? children;
    }
    public class Parser
    {
        LexemeData CurrectLexem = null;
        Lexer lexer = new Lexer();

        int notClosedBracket = 0;
        public Node resParser = new Node();

        public void ParserReadFile(string path)
        {
            resParser = new Node();
            using (FileStream fstream = File.OpenRead(path))
            {
                byte[] textFromFile = new byte[fstream.Length];
                fstream.Read(textFromFile);

                List<byte> text = new List<byte>();
                for (int i = 0; i < textFromFile.Length; i++)
                {
                    text.Add(textFromFile[i]);
                }

                byte endFile = 3;
                text.Add(endFile);

                CurrectLexem = lexer.GetLexeme(ref text);
                resParser = ExpSimple(ref text);
            }
            return;
        }
            
        public Node ExpSimple(ref List<byte> text)
        {
            Node leftСhild = Term(ref text);

            while ((CurrectLexem.value_lexeme == "+" || CurrectLexem.value_lexeme == "-") && CurrectLexem.class_lexeme == State.Operation)
            {
                var operation = CurrectLexem.value_lexeme;
                if (text.Count > 0)
                {
                    CurrectLexem = lexer.GetLexeme(ref text);
                }
                Node rightСhild = Term(ref text);
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

        public Node Term(ref List<byte> text)
        {
            Node leftСhild = Factor(ref text);

            if (leftСhild.type != NodeType.Error)
            {
                CurrectLexem = lexer.GetLexeme(ref text);
                while ((CurrectLexem.value_lexeme == "*" || CurrectLexem.value_lexeme == "/") && CurrectLexem.class_lexeme == State.Operation)
                {
                    var operation = CurrectLexem.value_lexeme;
                    if (text.Count > 0)
                    {
                        CurrectLexem = lexer.GetLexeme(ref text);
                    }
                    Node rightСhild = Factor(ref text);
                    if (text.Count > 0)
                    {
                        CurrectLexem = lexer.GetLexeme(ref text);
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

        public Node Factor(ref List<byte> text)
        {

            if (CurrectLexem.class_lexeme == State.Integers || CurrectLexem.class_lexeme == State.Real)
            {
                LexemeData factor = CurrectLexem;

                return new Node()
                {
                    type = NodeType.Number,
                    value = factor.value_lexeme
                };
            }
            if (CurrectLexem.class_lexeme == State.Identifier)
            {
                LexemeData factor = CurrectLexem;

                return new Node()
                {
                    type = NodeType.Identifier,
                    value = factor.value_lexeme
                };
            }
            if (CurrectLexem.value_lexeme == "(" && CurrectLexem.class_lexeme == State.Separator)
            {
                Node newExp = new Node();
                if (text.Count > 0)
                {
                    CurrectLexem = lexer.GetLexeme(ref text);
                    newExp = ExpSimple(ref text);
                } 
                else
                {
                    newExp = new Node()
                    {
                        type = NodeType.Error,
                        value = $"({lexer.lineNum},{lexer.symbolNum - 1}) Fatal: Syntax error, \")\" expected"
                    };
                }

                if (CurrectLexem.value_lexeme != ")" || CurrectLexem.class_lexeme != State.Separator)
                {
                    if (text.Count != 0)
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
