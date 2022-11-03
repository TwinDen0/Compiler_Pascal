using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        EndFile,
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

        string lexeme = "";
        string lexemeValue = "";
        int lineNum = 0;
        int symbolNum = 1;
        public bool pairSymbol = false;
        public char firstPairSymbol = ' ';
        string error = "";

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
            //читаем txt
            LexemeData newLexeme;
            using (StreamReader sr = new StreamReader(path, Encoding.Default))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line + " ";
                    //line = line + " ";
                    lineNum += 1;
                    while (line != "")
                    {
                        while (line[0] == ' ')
                        {
                            line = line.Remove(0, 1);
                            symbolNum += 1;
                            if (line.Length == 0) break;
                        }
                        newLexeme = GetLexeme(line);
                        if (newLexeme.value_lexeme == "error")
                        {
                            line = "";
                        }
                        line = line.Remove(0, newLexeme.inital_value.Length);
                    }
                }
                return;
            }
        }
        public LexemeData GetLexeme(string line)
        {
            LexemeData foundLexeme = new LexemeData();

            CurrentState = State.Start;
            pairSymbol = false;
            firstPairSymbol = ' ';

            char a;

            for (int i = 0; i < line.Length; i++)
            {
                symbolNum += 1;
                a = line[i];

                CurrentState = CurrentState;
                State nextStates = NextState(a);

                if (nextStates == State.NumSystem_16) a = char.ToUpper(a);

                if (i == (line.Length - 1) && pairSymbol == true)
                    nextStates = State.EndToken;

                if (nextStates != State.EndToken && nextStates != State.EndFile && nextStates != State.Comment)
                {
                    CurrentState = nextStates;
                    lexeme += a;
                }
                else
                {
                    if (nextStates == State.EndToken)
                    {
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
                            Console.WriteLine($"({lineNum}, {(symbolNum - lexeme.Length)}) {error} ");
                            foundLexeme.value_lexeme = "error";
                            lexeme = "";
                            return foundLexeme;
                        }

                        //проверить на что заканчивается лексема, если это реал, то обязательно на цифру

                        //symbolNum -= 1;
                        
                        foundLexeme = new LexemeData() { numb_line = lineNum, numb_symbol = (symbolNum - lexeme.Length), class_lexeme = CurrentState, value_lexeme =  lexemeValue, inital_value = lexeme };

                        Console.WriteLine($"{foundLexeme.numb_line} | {foundLexeme.numb_symbol} | {foundLexeme.class_lexeme} | {foundLexeme.value_lexeme} | {foundLexeme.inital_value}");

                        lexeme = "";
                        
                        return foundLexeme;
                    }
                }
            }
            return foundLexeme;

        }
        State NextState(char a)
        {
            TokenType tokenType = new TokenType();
            State nextState;

            //определить какой токен это
            if (Char.IsDigit(a))
                tokenType = TokenType.Number;
            if (Char.IsLetter(a))
                tokenType = TokenType.Letter;
            if (operations.Any(sm => sm == a))
                tokenType = TokenType.OperationSign;
            if (separators.Any(sm => sm == a))
                tokenType = TokenType.SeparatorSign;
            if (exceptsign.Any(sm => sm == a))
                tokenType = TokenType.ExceptSign;
            if (a == ' ')
            {
                if (CurrentState == State.String && pairSymbol == true)
                {
                    nextState = State.String;
                    return nextState;
                }
                nextState = State.EndToken;
                symbolNum -= 1;
                return nextState;
            }

            //исключения цифры
            if (CurrentState == State.Operation && tokenType == TokenType.Number)
            {
                if (lexeme == "-" || lexeme == "+")
                {
                    nextState = State.Integers;
                    return nextState;
                }
            }
            if (a == '.' && CurrentState == State.Integers)
            {
                nextState = State.Real;
                return nextState;
            }
            if (a == 'e' && CurrentState == State.Integers)
            {
                pairSymbol = true;
                nextState = State.Real;
                return nextState;
            }
            if (a == '+' && CurrentState == State.Integers && pairSymbol == true)
            {
                pairSymbol = false;
                nextState = State.Real;
                return nextState;
            }
            if (a == '-' && CurrentState == State.Integers && pairSymbol == true)
            {
                pairSymbol = false;
                nextState = State.Real;
                return nextState;
            }
            if (a == '%' && CurrentState == State.Start)
            {
                nextState = State.NumSystem_2;
                return nextState;
            }
            if (a == '&' && CurrentState == State.Start)
            {
                nextState = State.NumSystem_8;
                return nextState;
            }
            if (a == '$' && CurrentState == State.Start)
            {
                nextState = State.NumSystem_16;
                return nextState;
            }
            if (CurrentState == State.NumSystem_2)
            {
                if (a == '0' || a == '1')
                {
                    nextState = State.NumSystem_2;
                    return nextState;
                }
                else
                {
                    nextState = State.Error;
                    error = "Fatal: Syntax error, \";\" expected";
                    return nextState;
                }
            }
            if (CurrentState == State.NumSystem_8)
            {
                if (a >= '0' && a <= '7')
                {
                    nextState = State.NumSystem_8;
                    return nextState;
                }
                else
                {
                    nextState = State.Error;
                    error = "Fatal: Syntax error, \";\" expected";
                    return nextState;
                }
            }
            if (CurrentState == State.NumSystem_16 && tokenType == TokenType.Letter)
            {
                char aUp = char.ToUpper(a);
                if (aUp >= 'A' && aUp <= 'F')
                {
                    nextState = State.NumSystem_16;
                    return nextState;
                }
                else
                {
                    nextState = State.Error;
                    error = "Fatal: Syntax error, \";\" expected";
                    return nextState;
                }
            }

            //исключение строчки
            if (a == '\'' && (CurrentState == State.Start || CurrentState == State.String || CurrentState == State.Char))
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
                nextState = State.String;
                return nextState;
            }
            if (a == '#' && (CurrentState == State.Start || (CurrentState == State.String && pairSymbol == false)))
            {
                nextState = State.Char;
                return nextState;
            }
            if (CurrentState == State.Char && a == '\'')
            {
                nextState = State.String;
                return nextState;
            }
            if (pairSymbol == false && a != '#' && a != '\'' && CurrentState == State.String)
            {
                nextState = State.Error;
                error = "Fatal: Syntax error, \";\" expected";
                return nextState;
            }
            if (CurrentState == State.String)
            {
                nextState = State.String;
                return nextState;
            }

            //комменты
            if (a == '/' && CurrentState != State.String)
            {
                if (pairSymbol == true && firstPairSymbol == '/')
                {
                    firstPairSymbol = ' ';
                    pairSymbol = false;
                    nextState = State.EndFile;
                    return nextState;
                }
                else
                {
                    firstPairSymbol = '/';
                    pairSymbol = true;
                    nextState = State.Operation;
                    return nextState;
                }
            }
            if (a == '{' && CurrentState != State.String)
            {
                firstPairSymbol = '{';
                pairSymbol = true;
                nextState = State.Comment;
                return nextState;
            }
            if (a == '}' && firstPairSymbol == '{' && CurrentState == State.Comment)
            {
                firstPairSymbol = ' ';
                pairSymbol = false;
                lexeme = "";
                nextState = State.EndToken;
                return nextState;
            }

            //конец файла/////////////////////////////////////////////////////////////////////////////////////////////////
            if (a == '.' && lexeme == "end" && CurrentState == State.Identifier)
            {
                lexeme = "";

                nextState = State.EndFile;
                return nextState;
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
            
            for(int i = 1; i < 44; i++)
            {
                Console.WriteLine($"НАЧАЛО ФАЙЛА {i}---------");
                path = $@"..\..\..\tests\{i}in.txt";
                l.LexerReadFile(path, path_ans, path_out);
            }
            
            //path = $@"..\..\..\tests\18in.txt";
            //l.LexerReadFile(path, path_ans, path_out);
        }
    }
}
