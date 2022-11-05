using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace Compiler
{
    public enum State
    {
        Start,
        Comment,
        Identifier,
        Char,
        String,
        WordsKey,
        WordsNotKey,
        Integers,
        Real,
        NumSystem_2,
        NumSystem_8,
        NumSystem_16,
        Operation,
        Separator,
        EndToken,
        EndLine,
        Error
    }
    public enum TokenType
    {
        Letter,
        Number,
        OperationSign,
        SeparatorSign,
        ExceptSign
    }
    public class LexemeData
    {
        public int numb_line;
        public int numb_symbol;
        public State class_lexeme;
        public string value_lexeme = "";
        public string inital_value = "";
    }

    public class Lexer
    {
        public State CurrentState { get; private set; }
        char[] operations = { '=', ':', '+', '-', '*', '/', '<', '>' };
        char[] separators = { ',', ';', '(', ')', '[', ']', '.' };
        char[] exceptsign = { '\'', '#', '%', '&', '$', '{', '}', '\\' };
        string[] wordsKey = {
        "absolute",
        "abstract",
        "alias",
        "assembler",
        "bitpacked",
        "break",
        "cdecl",
        "continue",
        "cppdecl",
        "cvar",
        "default",
        "deprecated",
        "dynamic",
        "enumerator",
        "experimental",
        "export",
        "external",
        "far",
        "far16",
        "forward",
        "generic",
        "helper",
        "implements",
        "iocheck",
        "local",
        "near",
        "nodefault",
        "noreturn",
        "nostackframe",
        "oldfpccall",
        "otherwise",
        "overload",
        "override",
        "private",
        "protected",
        "public",
        "published",
        "read",
        "register",
        "reintroduce",
        "result",
        "safecall",
        "saveregisters",
        "softfloat",
        "specialize",
        "static",
        "stdcall",
        "stored",
        "strict",
        "unaligned",
        "unimplemented",
        "varargs",
        "virtual",
        "pascal platform",
        "winapi write",
        "index interrupt",
        "message name",
        };
        string[] wordsNotKey =
        {
            "absolute",
            "and",
            "array",
            "asm",
            "begin",
            "case",
            "const",
            "constructor",
            "destructor",
            "div",
            "do",
            "downto",
            "else",
            "end",
            "file",
            "for",
            "function",
            "goto",
            "if",
            "implementation",
            "in",
            "inherited",
            "inline",
            "interface",
            "label",
            "mod",
            "nil",
            "not",
            "object",
            "or",
            "packed",
            "procedure",
            "reintroduce",
            "repeat",
            "self",
            "set",
            "shl",
            "shr",
            "string",
            "then",
            "unit",
            "until",
            "uses",
            "var",
            "while",
            "with",
            "xor",

            "to type",
            "program record",
            "of operator",
        };

        int lineNum;
        int symbolNum;
        string lexeme = "";
        string lexemeValue = "";
        public bool pairSymbol = false;
        public char firstPairSymbol = ' ';
        string error = "";
        bool flagEndFile = false;

        struct StateTransition
        {
            State state;
            TokenType token;
            public StateTransition(State newState, TokenType newToken)
            {
                state = newState;
                token = newToken;
            }
        }
        Dictionary<StateTransition, State> transitions;

        public Lexer()
        {
            CurrentState = State.Start;
            transitions = new Dictionary<StateTransition, State>
            {
                { new StateTransition(State.Start, TokenType.Letter), State.Identifier },
                { new StateTransition(State.Start, TokenType.Number), State.Integers },
                { new StateTransition(State.Start, TokenType.OperationSign), State.Operation },
                { new StateTransition(State.Start, TokenType.SeparatorSign), State.Separator },

                { new StateTransition(State.Identifier, TokenType.Letter), State.Identifier },
                { new StateTransition(State.Identifier, TokenType.Number), State.Identifier },
                { new StateTransition(State.Identifier, TokenType.OperationSign), State.EndToken },
                { new StateTransition(State.Identifier, TokenType.SeparatorSign), State.EndToken },

                { new StateTransition(State.Char, TokenType.Number), State.Char},
                { new StateTransition(State.Char, TokenType.OperationSign), State.EndToken },
                { new StateTransition(State.Char, TokenType.SeparatorSign), State.EndToken },

                { new StateTransition(State.String, TokenType.Letter), State.String },
                { new StateTransition(State.String, TokenType.Number), State.String},
                { new StateTransition(State.String, TokenType.OperationSign), State.String },
                { new StateTransition(State.String, TokenType.SeparatorSign), State.String },
                { new StateTransition(State.String, TokenType.ExceptSign), State.String },

                { new StateTransition(State.Integers, TokenType.Number), State.Integers },
                { new StateTransition(State.Integers, TokenType.OperationSign), State.EndToken },
                { new StateTransition(State.Integers, TokenType.SeparatorSign), State.EndToken },

                { new StateTransition(State.Real, TokenType.Number), State.Real },
                { new StateTransition(State.Real, TokenType.OperationSign), State.EndToken },
                { new StateTransition(State.Real, TokenType.SeparatorSign), State.EndToken },

                { new StateTransition(State.NumSystem_16, TokenType.Number), State.NumSystem_16 },
            };
        }

        public void LexerReadFile(string path, string path_ans, string path_out)
        {
            LexemeData newLexeme;

            using (FileStream fstream = File.OpenRead(path))
            {
                lineNum = 1;
                symbolNum = 0;

                byte[] textFromFile = new byte[fstream.Length];
                fstream.Read(textFromFile);

                List<byte> text = new List<byte>();
                for (int i = 0; i < textFromFile.Length; i++)
                {
                    text.Add(textFromFile[i]);
                }

                byte endFile = 3;
                text.Add(endFile);

                while (text.Count != 1)
                {
                    while (text[0] == (byte)' ' || text[0] == 13)
                    {
                        symbolNum += 1;
                        text.RemoveAt(0);
                    }

                    newLexeme = GetLexeme(ref text);

                    if (newLexeme.value_lexeme == "error" || text[0] == 3)
                    {
                        break;
                    }
                    /*
                    for (int i = 0; i < newLexeme.inital_value.Length; i++)
                    {
                        text.RemoveAt(0);
                    }*/
                }
                if (flagEndFile == true)
                {
                    return;
                }


            }
            return;
        }
        public LexemeData GetLexeme(ref List<byte> text)
        {
            LexemeData foundLexeme = new LexemeData();

            CurrentState = State.Start;
            pairSymbol = false;
            firstPairSymbol = ' ';

            byte a;

            for (int i = 0; i < text.Count; i++)
            {
                a = text[i];

                State nextStates = NextState(a);

                if(a == 13) continue;

                if (CurrentState == State.Comment)
                {
                    if(firstPairSymbol == '{')
                    {
                        if (a == (byte)'}')
                        {
                            firstPairSymbol = ' ';
                            pairSymbol = false;
                            text.RemoveAt(0);
                            nextStates = State.EndToken;
                        }
                        else
                        {
                            if(a == 10)
                            {
                                symbolNum = 0;
                                lineNum += 1;
                            }
                            symbolNum += 1;
                            continue;
                        }
                        if (i == (text.Count - 1))
                        {
                            CurrentState = State.Error;
                            error = "Fatal: Unexpected end of file";
                        }
                    }
                    else
                    {
                        if (a == 10 || a == 3)
                        {
                            nextStates = State.EndToken;
                        }
                        else
                        {
                            symbolNum += 1;
                            continue;
                        }
                    }
                    
                }
                if (nextStates == State.EndLine)
                {
                    if(CurrentState != State.Start)
                    {
                        nextStates = State.EndToken;
                    } else
                    {
                        symbolNum = 0;
                        lineNum += 1;
                        text.RemoveAt(0);
                        return foundLexeme;
                    }
                }

                if (nextStates != State.EndToken && nextStates != State.Start)
                {
                    CurrentState = nextStates;
                    lexeme += (char)a;
                    symbolNum += 1;
                }
                else
                {
                    if (nextStates == State.EndToken && lexeme != "")
                    {
                        for (int j = 0; j < i; j++)
                        {
                            text.RemoveAt(0);
                        }
                        text = text;
                        if (CurrentState == State.Comment)
                        {
                            lexeme = "";
                            return foundLexeme;
                        }

                        if (CurrentState == State.Char) CurrentState = State.String;
                        if (CurrentState == State.Identifier)
                        {
                            if (wordsKey.Any(sm => sm == lexeme))
                                CurrentState = State.WordsKey;
                            if (wordsNotKey.Any(sm => sm == lexeme))
                                CurrentState = State.WordsNotKey;
                        }

                        SearchValue(lexeme);

                        if (CurrentState == State.Error)
                        {
                            Console.WriteLine($"({lineNum}, {(symbolNum - lexeme.Length + 1)}) {error} ");
                            foundLexeme.value_lexeme = "error";
                            lexeme = "";
                            return foundLexeme;
                        }

                        //проверить на что заканчивается лексема, если это реал, то обязательно на цифру
                        foundLexeme = new LexemeData() { numb_line = lineNum, numb_symbol = (symbolNum - lexeme.Length + 1), class_lexeme = CurrentState, value_lexeme =  lexemeValue, inital_value = lexeme };

                        Console.WriteLine($"{foundLexeme.numb_line} | {foundLexeme.numb_symbol} | {foundLexeme.class_lexeme} | {foundLexeme.value_lexeme} | {foundLexeme.inital_value}");

                        lexeme = "";
                        return foundLexeme;
                    }
                }
            }
            return foundLexeme;
        }
        State NextState(byte aByte)
        {
            TokenType tokenType = new TokenType();
            State nextState;
            char aChar = (char)aByte;
            if (aByte == 10)
                return State.EndLine;
            if (aByte == 3)
            {
                flagEndFile = true;
                return State.EndToken;
            }
                

            //определить какой токен это
            if (Char.IsDigit(aChar))
                tokenType = TokenType.Number;
            if (Char.IsLetter(aChar))
                tokenType = TokenType.Letter;
            if (operations.Any(sm => sm == aChar))
                tokenType = TokenType.OperationSign;
            if (separators.Any(sm => sm == aChar))
                tokenType = TokenType.SeparatorSign;
            if (exceptsign.Any(sm => sm == aChar))
                tokenType = TokenType.ExceptSign;
            if (aChar == ' ')
                return State.EndToken;

            //исключения цифры
            if (CurrentState == State.Operation && tokenType == TokenType.Number)
            {
                if (lexeme == "-" || lexeme == "+")
                    return State.Integers;
            }
            if (aChar == '.' && CurrentState == State.Integers)
            {
                return State.Real;
            }
            if (aChar == 'e' && CurrentState == State.Integers)
            {
                pairSymbol = true;
                return State.Real;
            }
            if (aChar == '+' && CurrentState == State.Integers && pairSymbol == true)
            {
                pairSymbol = false;
                return State.Real;
            }
            if (aChar == '-' && CurrentState == State.Integers && pairSymbol == true)
            {
                pairSymbol = false;
                return State.Real;
            }
            if (aChar == '%' && CurrentState == State.Start)
            {
                return State.NumSystem_2;
            }
            if (aChar == '&' && CurrentState == State.Start)
            {
                return State.NumSystem_8;
            }
            if (aChar == '$' && CurrentState == State.Start)
            {
                return State.NumSystem_16;
            }
            if (CurrentState == State.NumSystem_2)
            {
                if (aChar == '0' || aChar == '1')
                    return State.NumSystem_2;
                else
                {
                    error = "Fatal: Syntax error, \";\" expected";
                    return State.Error;
                }
            }
            if (CurrentState == State.NumSystem_8)
            {
                if (aChar >= '0' && aChar <= '7')
                    return State.NumSystem_8;
                else
                {
                    error = "Fatal: Syntax error, \";\" expected";
                    return State.Error;
                }
            }
            if (CurrentState == State.NumSystem_16 && tokenType == TokenType.Letter)
            {
                char aUp = char.ToUpper(aChar);
                if (aUp >= 'A' && aUp <= 'F')
                    return State.NumSystem_16;
                else
                {
                    error = "Fatal: Syntax error, \";\" expected";
                    return State.Error;
                }
            }

            //исключение строчки
            if (aChar == '\'' && (CurrentState == State.Start || CurrentState == State.String || CurrentState == State.Char))
            {
                if (pairSymbol == true && firstPairSymbol == '\'')
                {
                    firstPairSymbol = ' ';
                    pairSymbol = false;
                }
                else
                {
                    firstPairSymbol = '\'';
                    pairSymbol = true;
                }
                return State.String;
            }
            if (aChar == '#' && (CurrentState == State.Start || (CurrentState == State.String && pairSymbol == false)))
            {
                return State.Char;
            }
            if (CurrentState == State.Char && aChar == '\'')
            {
                return State.String;
            }
            if (pairSymbol == false && aChar != '#' && aChar != '\'' && CurrentState == State.String)
            {
                error = "Fatal: Syntax error, \";\" expected";
                return State.Error;
            }
            if (CurrentState == State.String)
            {
                return State.String;
            }

            //комменты
            if (lexeme == "/" && aChar == '/')
            {
                return State.Comment;
            }
            if (aChar == '{' && CurrentState != State.String)
            {
                firstPairSymbol = '{';
                pairSymbol = true;
                nextState = State.Comment;
                return nextState;
            }

            //конец файла/////////////////////////////////////////////////////////////////////////////////////////////////
            if (aChar == '.' && lexeme == "end" && CurrentState == State.Identifier)
            {
                lexeme = "";

                flagEndFile = true;
                return State.EndToken;
            }

            StateTransition transition = new StateTransition(CurrentState, tokenType);
            if (!transitions.TryGetValue(transition, out nextState))
            {
                error = "Fatal: Syntax error, \";\" expected";
                nextState = State.Error;
                return nextState;
            }
            return nextState;
        }
        string SearchValue(string lex)
        {
            lexemeValue = lex;

            if (CurrentState == State.String)
            {
                lexemeValue = "";

                if (pairSymbol == true)
                {
                    error = "Fatal: Syntax error, \";\" expected";
                    CurrentState = State.Error;
                    return "";
                }

                for (int i = 0; i < lex.Length; i++)
                {
                    if (lex[i] == '\'')
                    {
                        if (i > 1 && i < (lex.Length - 1))
                        {
                            if (lex[(i - 1)] == '\'')
                            {
                                lexemeValue += "'";
                                continue;
                            }
                        }
                        continue;
                    } 
                    if (lex[i] == '\'' && lex[i++] == '\'')
                        lexemeValue += "'";
                    if (lex[i] == '#' && (i == 0 || lex[i-1] == '\''))
                    {
                        i += 1;
                        string numberChar = "";
                        while (Char.IsDigit(lex[i]))
                        {
                            numberChar += lex[i];
                            if (i < (lex.Length - 1)) i++;
                            else break;
                        }
                        int symbol = Int32.Parse(numberChar);
                        if (symbol > 65535)
                        {
                            CurrentState = State.Error;
                            error = "ERROR: Overflow symbol in string";
                            break;
                        }
                        numberChar = "";
                        numberChar += (char)symbol;
                        lexemeValue += numberChar;
                        continue;
                    }

                    lexemeValue += lex[i];
                }
            }
            if (CurrentState == State.Integers)
            {
                if (lex[0] == '+') lex = lex.Remove(0, 1);
                long lexInt = long.Parse(lex);
                if (lexInt >= -2147483648 && lexInt <= 2147483647)
                {
                    lexemeValue = lex;
                }
                else
                {
                    CurrentState = State.Error;
                    error = "Fatal: Going outside";
                    //error = "Warning(s) Issued";
                }
            }
            if (CurrentState == State.Real)
            {
                double value = 0;
                lex = lex.Replace(".", ",");
                lex = lex.ToUpper();
                int mantis;

                switch (lex[0])
                {
                    case '%':
                        lex = lex.Remove(0, 1);
                        if (lex.IndexOf(',') != -1)
                        {
                            mantis = Convert.ToInt32(lex.Substring(0, lex.IndexOf(',')), 2);
                            lex = lex.Remove(0, lex.IndexOf(','));
                        }
                        else
                        {
                            mantis = Convert.ToInt32(lex.Substring(0, lex.IndexOf('E')), 2);
                            lex = lex.Remove(0, lex.IndexOf('E'));
                        }
                        lex = mantis.ToString() + lex;
                        value = Convert.ToDouble(lex);
                        break;
                    case '&':
                        lex = lex.Remove(0, 1);
                        if (lex.IndexOf(',') != -1)
                        {
                            mantis = Convert.ToInt32(lex.Substring(0, lex.IndexOf(',')), 8);
                            lex = lex.Remove(0, lex.IndexOf(','));
                        }
                        else
                        {
                            mantis = Convert.ToInt32(lex.Substring(0, lex.IndexOf('E')), 8);
                            lex = lex.Remove(0, lex.IndexOf('E'));
                        }
                        lex = mantis.ToString() + lex;
                        value = Convert.ToDouble(lex);
                        break;
                    case '$':
                        lex = lex.Remove(0, 1);
                        if (lex.IndexOf(',') != -1)
                        {
                            mantis = Convert.ToInt32(lex.Substring(0, lex.IndexOf(',')), 16);
                            lex = lex.Remove(0, lex.IndexOf(','));
                        }
                        else
                        {
                            mantis = Convert.ToInt32(lex.Substring(0, lex.IndexOf('E')), 16);
                            lex = lex.Remove(0, lex.IndexOf('E'));
                        }
                        lex = mantis.ToString() + lex;
                        value = Convert.ToDouble(lex);
                        break;
                    default:
                        value = Convert.ToDouble(lex);
                        break;
                }

                lexemeValue = value.ToString("E10").Replace(",", ".");

                if (lexemeValue == "∞")
                {
                    lexemeValue = "+Inf";
                }
            }

            if (CurrentState == State.NumSystem_2 || CurrentState == State.NumSystem_8 || CurrentState == State.NumSystem_16)
            {

            }


            if (CurrentState == State.Operation)
            {

            }
            if (CurrentState == State.Separator)
            {

            }

            return lexemeValue;
        }
    }


    public class Program
    {
        static void Main(string[] args)
        {
            string path;
            string path_out = @"..\..\..\out.txt";
            string path_ans = @"..\..\..\tests\01ans.txt";
            Lexer l = new Lexer();
            /*
            for(int i = 1; i < 44; i++)
            {
                Console.WriteLine($"НАЧАЛО ФАЙЛА {i}---------");
                path = $@"..\..\..\tests\{i}in.txt";
                l.LexerReadFile(path, path_ans, path_out);
            }*/
            
           path = $@"..\..\..\tests\100in.txt";
           l.LexerReadFile(path, path_ans, path_out);

        }
    }
}
