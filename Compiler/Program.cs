using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

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
        public static List<LexemeData> lexems = new List<LexemeData>();

        public State CurrentState
        {
            get;
            private set;
        }
        char[] operations = { '=', ':', '+', '-', '*', '/', '<', '>' };
        string[] pair_operations = { "<<", ">>", "**", "<>", "><", "<=", ">=", ":=", "+=", "-=", "*=", "/="};
        char[] separators = { ',', ';', '(', ')', '[', ']', '.' };
        char[] exceptsign = { '\'', '#', '%', '&', '$', '{', '}', '\\' };
        string[] wordsKey = {
      "absolute",     "abstract",   "alias",         "assembler",
      "bitpacked",    "break",      "cdecl",         "continue",
      "cppdecl",      "cvar",       "default",       "deprecated",
      "dynamic",      "enumerator", "experimental",  "export",
      "external",     "far",        "far16",         "forward",
      "generic",      "helper",     "implements",    "iocheck",
      "local",        "near",       "nodefault",     "noreturn",
      "nostackframe", "oldfpccall", "otherwise",     "overload",
      "override",     "private",    "protected",     "public",
      "published",    "read",       "register",      "reintroduce",
      "result",       "safecall",   "saveregisters", "softfloat",
      "specialize",   "static",     "stdcall",       "stored",
      "strict",       "unaligned",  "unimplemented", "varargs",
      "virtual",      "pascal",     "platform",      "winapi",
      "write",        "index",      "interrupt",     "message",
      "name",
  };
        string[] wordsNotKey = {
      "absolute", "and",       "array",       "asm",        "begin",
      "case",     "const",     "constructor", "destructor", "div",
      "do",       "downto",    "else",        "end",        "file",
      "for",      "function",  "goto",        "if",         "implementation",
      "in",       "inherited", "inline",      "interface",  "label",
      "mod",      "nil",       "not",         "object",     "or",
      "packed",   "procedure", "reintroduce", "repeat",     "self",
      "set",      "shl",       "shr",         "string",     "then",
      "unit",     "until",     "uses",        "var",        "while",
      "with",     "xor",       "to",          "type",       "program",
      "record",   "of",        "operator",
  };

        public int lineNum = 1;
        public int symbolNum = 0;
        string lexeme = "";
        string lexemeValue = "";
        public bool pairSymbol = false;
        public char firstPairSymbol = ' ';
        string error = "";
        bool flagEndFile = false;
        int i;

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
            transitions = new Dictionary<StateTransition, State>{
        {new StateTransition(State.Start, TokenType.Letter), State.Identifier},
        {new StateTransition(State.Start, TokenType.Number), State.Integers},
        {new StateTransition(State.Start, TokenType.OperationSign),
         State.Operation},
        {new StateTransition(State.Start, TokenType.SeparatorSign),
         State.Separator},

        {new StateTransition(State.Identifier, TokenType.Letter),
         State.Identifier},
        {new StateTransition(State.Identifier, TokenType.Number),
         State.Identifier},
        {new StateTransition(State.Identifier, TokenType.OperationSign),
         State.EndToken},
        {new StateTransition(State.Identifier, TokenType.SeparatorSign),
         State.EndToken},

        {new StateTransition(State.Char, TokenType.Number), State.Char},
        {new StateTransition(State.Char, TokenType.OperationSign),
         State.EndToken},
        {new StateTransition(State.Char, TokenType.SeparatorSign),
         State.EndToken},

        {new StateTransition(State.String, TokenType.Letter), State.String},
        {new StateTransition(State.String, TokenType.Number), State.String},
        {new StateTransition(State.String, TokenType.OperationSign),
         State.String},
        {new StateTransition(State.String, TokenType.SeparatorSign),
         State.String},
        {new StateTransition(State.String, TokenType.ExceptSign), State.String},

        {new StateTransition(State.Integers, TokenType.Number), State.Integers},
        {new StateTransition(State.Integers, TokenType.OperationSign),
         State.EndToken},
        {new StateTransition(State.Integers, TokenType.SeparatorSign),
         State.EndToken},

        {new StateTransition(State.Real, TokenType.Number), State.Real},
        {new StateTransition(State.Real, TokenType.OperationSign),
         State.EndToken},
        {new StateTransition(State.Real, TokenType.SeparatorSign),
         State.EndToken},

        {new StateTransition(State.NumSystem_16, TokenType.Number),
         State.NumSystem_16},

        {new StateTransition(State.Operation, TokenType.Letter),
         State.EndToken},
        {new StateTransition(State.Operation, TokenType.Number),
         State.EndToken},
        {new StateTransition(State.Operation, TokenType.SeparatorSign),
         State.EndToken},
        {new StateTransition(State.Operation, TokenType.ExceptSign),
         State.EndToken},

        {new StateTransition(State.Separator, TokenType.Letter),
         State.EndToken},
        {new StateTransition(State.Separator, TokenType.Number),
         State.EndToken},
        {new StateTransition(State.Separator, TokenType.OperationSign),
         State.EndToken},
        {new StateTransition(State.Separator, TokenType.SeparatorSign),
         State.EndToken},
        {new StateTransition(State.Separator, TokenType.ExceptSign),
         State.EndToken},
    };
        }

        public void LexerReadFile(string path)
        {
            lineNum = 1;
            symbolNum = 0;
            lexems = new List<LexemeData>();
                LexemeData newLexeme;

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

            for (i = 0; i < text.Count; i++)
            {
                a = text[i];

                State nextStates = NextState(a);

                if (a == 13) continue;

                if (CurrentState == State.Comment)
                {
                    if (firstPairSymbol == '{')
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
                            if (a == 10)
                            {
                                symbolNum = 0;
                                lineNum += 1;
                            }
                            symbolNum += 1;
                            continue;
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
                    if (CurrentState != State.Start)
                    {
                        nextStates = State.EndToken;
                    }
                    else
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
                        if (CurrentState == State.Comment)
                        {
                            lexeme = "";
                            return foundLexeme;
                        }

                        if (CurrentState == State.Char) CurrentState = State.String;
                        if (CurrentState == State.Integers && !Char.IsDigit(lexeme[^1]))
                        {
                            CurrentState = State.Real;
                            lexeme += '0';
                        }
                            

                        SearchValue(lexeme);

                        if (CurrentState == State.Error)
                        {
                            foundLexeme = new LexemeData()
                            {
                                numb_line = lineNum,
                                numb_symbol = (symbolNum - lexeme.Length + 1),
                                class_lexeme = State.Error,
                                value_lexeme = error,
                                inital_value = lexeme
                            };
                            lexeme = "";
                            lexems.Add(foundLexeme);
                            return foundLexeme;
                        }

                        foundLexeme = new LexemeData()
                        {
                            numb_line = lineNum,
                            numb_symbol = (symbolNum - lexeme.Length + 1),
                            class_lexeme = CurrentState,
                            value_lexeme = lexemeValue,
                            inital_value = lexeme
                        };

                        lexems.Add(foundLexeme);

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
            if (aByte == 10) return State.EndLine;
            if (aByte == 3)
            {
                flagEndFile = true;
                return State.EndToken;
            }

            //определить какой токен это
            if (Char.IsDigit(aChar)) tokenType = TokenType.Number;
            if (Char.IsLetter(aChar)) tokenType = TokenType.Letter;
            if (operations.Any(sm => sm == aChar)) tokenType = TokenType.OperationSign;
            if (separators.Any(sm => sm == aChar)) tokenType = TokenType.SeparatorSign;
            if (exceptsign.Any(sm => sm == aChar)) tokenType = TokenType.ExceptSign;
            if (aChar == ' ' && CurrentState != State.Comment) return State.EndToken;

            //комменты
            if (CurrentState == State.Comment) return State.Comment;
            if (lexeme == "/" && aChar == '/') return State.Comment;
            if (aChar == '{' && CurrentState != State.String)
            {
                firstPairSymbol = '{';
                pairSymbol = true;
                nextState = State.Comment;
                return nextState;
            }

            //исключения цифры
            
            if(lexeme != "")
            {
                if (tokenType == TokenType.Number && lexeme[^1] == '.' && CurrentState == State.Integers) return State.Real;
                if (aChar == '.' && lexeme[^1] == '.' && CurrentState == State.Integers)
                {
                    i -= 1;
                    symbolNum -= 1;
                    lexeme = lexeme.Remove(lexeme.Length - 1, 1);
                    return State.EndToken;
                }
                if (aChar == '.' && CurrentState == State.Integers) return State.Integers;
            }

            if (aChar == 'e' && CurrentState == State.Integers)
            {
                pairSymbol = true;
                return State.Real;
            }
            if (aChar == '+' && CurrentState == State.Real && pairSymbol == true)
            {
                pairSymbol = false;
                return State.Real;
            }
            if (aChar == '-' && CurrentState == State.Real && pairSymbol == true)
            {
                pairSymbol = false;
                return State.Real;
            }
            if (aChar == '%' && CurrentState == State.Start) return State.NumSystem_2;
            if (aChar == '&' && CurrentState == State.Start) return State.NumSystem_8;
            if (aChar == '$' && CurrentState == State.Start) return State.NumSystem_16;
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
            if (aChar == '\'' &&
                (CurrentState == State.Start || CurrentState == State.String ||
                 CurrentState == State.Char))
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
            if (aChar == '#' &&
                (CurrentState == State.Start ||
                 (CurrentState == State.String && pairSymbol == false)))
            {
                return State.Char;
            }
            if (CurrentState == State.Char && aChar == '\'')
            {
                return State.String;
            }
            if (pairSymbol == false && aChar != '#' && aChar != '\'' &&
                CurrentState == State.String)
            {
                error = "Fatal: Syntax error, \";\" expected";
                return State.Error;
            }
            if (CurrentState == State.String)
            {
                return State.String;
            }

            //парные операнты
            if (CurrentState == State.Operation &&
                tokenType == TokenType.OperationSign)
            {
                string pair = "";
                pair = lexeme[^1] + aChar.ToString();
                pair = pair;
                if (pair_operations.Any(sm => sm == pair))
                {
                    return State.Operation;
                }
            }

            //парные разделители
            if (lexeme != "")
            {
                if (aChar == '.' && lexeme[^1] == '.' && CurrentState == State.Separator)
                {
                    return State.Separator;
                }
            }

            //конец
            //файла/////////////////////////////////////////////////////////////////////////////////////////////////
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
            if (CurrentState == State.Identifier)
            {
                if (lex.Length > 255)
                {
                    CurrentState = State.Error;
                    error = "Fatal: Overflow string";
                    return "";
                }
                if (wordsKey.Any(sm => sm == lexeme)) CurrentState = State.WordsKey;
                if (wordsNotKey.Any(sm => sm == lexeme)) CurrentState = State.WordsNotKey;
                lexemeValue = lex;
            }
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
                    if (lex[i] == '\'' && lex[i++] == '\'') lexemeValue += "'";
                    if (lex[i] == '#' && (i == 0 || lex[i - 1] == '\''))
                    {
                        i += 1;
                        string numberChar = "";
                        while (Char.IsDigit(lex[i]))
                        {
                            numberChar += lex[i];
                            if (i < (lex.Length - 1))
                                i++;
                            else
                                break;
                        }
                        int symbol = Int32.Parse(numberChar);
                        if (symbol > 65535)
                        {
                            CurrentState = State.Error;
                            error = "Fatal: Overflow symbol in string";
                            break;
                        }
                        numberChar = "";
                        numberChar += (char)symbol;
                        lexemeValue += numberChar;
                        continue;
                    }

                    lexemeValue += lex[i];
                }

                if (lexemeValue.Length > 255)
                {
                    CurrentState = State.Error;
                    error = "Fatal: Overflow string";
                    return "";
                }
            }
            if (CurrentState == State.Integers)
            {
                long lexInt = long.Parse(lex);
                if (lexInt <= 2147483647)
                {
                    lexemeValue = lex;
                }
                else
                {
                    CurrentState = State.Error;
                    error = "Fatal: Going outside";
                    // error = "Warning(s) Issued";
                }
            }
            if (CurrentState == State.Real)
            {
                double value = 0;
                lex = lex.Replace(".", ",");
                lex = lex.ToUpper();
                int mantis;

                if (lex[^1] == 'E' || lex[^1] == '.')
                    lex += '0';

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
            if (CurrentState == State.NumSystem_2 ||
                CurrentState == State.NumSystem_8 ||
                CurrentState == State.NumSystem_16)
            {
                int numSystem = 10;
                long numValue = 0;
                switch (CurrentState)
                {
                    case State.NumSystem_2:
                        {
                            numSystem = 2;
                            break;
                        }
                    case State.NumSystem_8:
                        {
                            numSystem = 8;
                            break;
                        }
                    case State.NumSystem_16:
                        {
                            numSystem = 16;
                            break;
                        }
                }
                if (lex.Length < 2)
                {
                    CurrentState = State.Error;
                    error = "Error: Invalid integer expression";
                    return "";
                }
                for (int j = 1; j < lex.Length; j++)
                {
                    for (int k = 1; k < lexemeValue.Length; k++)
                    {
                    }
                    if ((lexeme[j] >= 'A' && lexeme[j] <= 'F') ||
                        (lexeme[j] >= 'a' && lexeme[j] <= 'f'))
                    {
                        uint aUint = ((uint)Char.ToLower(lexeme[j]) - (uint)'a') + 10;
                        numValue = (numValue * numSystem) + aUint;
                    }
                    else
                        numValue = (numValue * numSystem) + (lexeme[j] - '0');
                }

                if (numValue > 2147483647)
                {
                    CurrentState = State.Error;
                    error = "Fatal: Going outside";
                    return "";
                }

                lexemeValue = numValue.ToString();
            }
            if (CurrentState == State.Operation)
            {
                lexemeValue = lex;
            }
            if (CurrentState == State.Separator)
            {
                lexemeValue = lex;
            }
            return lexemeValue;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            string path = $@"..\..\..\tests\tests_parser\in_test\500in.txt";
            string path_out = @"..\..\..\out.txt";
            string path_ans = @"..\..\..\tests\01ans.txt";
            Lexer l = new Lexer();
            Parser p = new Parser();

            //p.ParserReadFile(path);

            Tester.StartTest();

            //path = $@"..\..\..\tests\in_test\47in.txt";
            //l.LexerReadFile(path, path_ans, path_out);
        }
    }
}