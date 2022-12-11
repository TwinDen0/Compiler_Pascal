using System;
using System.IO;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;

namespace Compiler.Lexer
{
    public enum State
    {
        Start,
        Comment,
        Identifier,
        Char,
        String,
        WordsKey,
        WordsKeyNonReserv,
        Integer,
        Real,
        NumSystem_2,
        NumSystem_8,
        NumSystem_16,
        Operator,
        Separator,
        EndToken,
        EndLine,
        EndFile,
        Error
    }
    public enum SymbolType
    {
        Letter,
        Number,
        OperationSign,
        SeparatorSign,
        ExceptSign
    }
    public class Lexer
    {
        private LexemeData lastLexeme;
        private readonly StreamReader reader;
        private string? lastLineFile;
        public int lineNum;
        public int symbolNum;
        public string foundLexeme;
        private object lexemeValue;
        private bool pairSymbol;
        private char firstPairSymbol;
        private string error;
        private int cursor;
        State nextStates;
        public State currentState { get; private set; }
        struct StateTransition
        {
            State state;
            SymbolType token;
            public StateTransition(State newState, SymbolType newToken)
            {
                state = newState;
                token = newToken;
            }
        }
        Dictionary<StateTransition, State> transitions;
        char[] operations = { '=', ':', '+', '-', '*', '/', '<', '>', '{' };
        string[] pair_operations = { "<<", ">>", "**", "<>", "><", "<=", ">=", ":=", "+=", "-=", "*=", "/=" };
        char[] separators = { '.', ',', ';', '(', ')', '[', ']' };
        char[] exceptsign = { '\'', '#', '%', '&', '$' };
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
        string[] wordsKeyNonReserv = {
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
        public Lexer(string filePath)
        {
            currentState = State.Start;
            lastLexeme = null;
            reader = new StreamReader(filePath);
            lastLineFile = reader.ReadLine();
            transitions = new Dictionary<StateTransition, State>{
                {new StateTransition(State.Start, SymbolType.Letter), State.Identifier},
                {new StateTransition(State.Start, SymbolType.Number), State.Integer},
                {new StateTransition(State.Start, SymbolType.OperationSign), State.Operator},
                {new StateTransition(State.Start, SymbolType.SeparatorSign), State.Separator},

                {new StateTransition(State.Identifier, SymbolType.Letter), State.Identifier},
                {new StateTransition(State.Identifier, SymbolType.Number), State.Identifier},
                {new StateTransition(State.Identifier, SymbolType.OperationSign), State.EndToken},
                {new StateTransition(State.Identifier, SymbolType.SeparatorSign), State.EndToken},

                {new StateTransition(State.Integer, SymbolType.Number), State.Integer},
                {new StateTransition(State.Integer, SymbolType.OperationSign), State.EndToken},
                {new StateTransition(State.Integer, SymbolType.SeparatorSign), State.EndToken},

                {new StateTransition(State.Real, SymbolType.Number), State.Real},
                {new StateTransition(State.Real, SymbolType.OperationSign), State.EndToken},
                {new StateTransition(State.Real, SymbolType.SeparatorSign), State.EndToken},

                {new StateTransition(State.NumSystem_16, SymbolType.Number), State.NumSystem_16},

                {new StateTransition(State.Operator, SymbolType.Letter), State.EndToken},
                {new StateTransition(State.Operator, SymbolType.Number), State.EndToken},
                {new StateTransition(State.Operator, SymbolType.SeparatorSign), State.EndToken},
                {new StateTransition(State.Operator, SymbolType.ExceptSign), State.EndToken},

                {new StateTransition(State.Separator, SymbolType.Letter), State.EndToken},
                {new StateTransition(State.Separator, SymbolType.Number), State.EndToken},
                {new StateTransition(State.Separator, SymbolType.OperationSign), State.EndToken},
                {new StateTransition(State.Separator, SymbolType.SeparatorSign), State.EndToken},
                {new StateTransition(State.Separator, SymbolType.ExceptSign), State.EndToken},
            };
            lineNum = 1;
            symbolNum = 0;
            foundLexeme = null;
            lexemeValue = null;
            error = null;
            cursor = 0;
        }
        public LexemeData PeekLexeme()
        {
            return lastLexeme;
        }
        public LexemeData GetLexeme()
        {
            if (lastLexeme != null)
            {
                if (lastLexeme.LexemeType == LexemeType.ERROR) return null;
            }

            var lexemeReceived = false;
            while (!lexemeReceived)
            {
                for (int i = cursor; i < lastLineFile?.Length; i++)
                {
                    char symbol = lastLineFile[i];

                    nextStates = NextState(symbol);

                    if (nextStates == State.Comment)
                    {
                        currentState = State.Start;
                        break;
                    }

                    if (nextStates == State.Error)
                    {
                        lastLexeme = new LexemeData(lineNum, symbolNum, LexemeType.ERROR, error, null);
                        return lastLexeme;
                    }

                    if(nextStates == State.Start)
                    {
                        cursor += 1;
                        continue;
                    }

                    if (nextStates != State.EndToken)
                    {
                        currentState = nextStates;
                        foundLexeme += symbol;
                        symbolNum += 1;
                        if (i == lastLineFile.Length - 1)
                        {
                            i += 1;
                            if (currentState == State.Comment)
                            {
                                nextStates = State.Comment;
                            }
                            else
                            {
                                nextStates = State.EndToken;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (nextStates == State.EndToken)
                    {
                        lexemeValue = SearchValueLexeme();

                        var LexemType = new LexemeType();
                        switch (currentState)
                        {
                            case State.Comment:
                                LexemType = LexemeType.NONE; break;
                            case State.Identifier:
                                LexemType = LexemeType.IDENTIFIER; break;
                            case State.Char:
                                LexemType = LexemeType.STRING; break;
                            case State.String:
                                LexemType = LexemeType.STRING; break;
                            case State.WordsKey:
                                LexemType = LexemeType.WORDKEY; break;
                            case State.WordsKeyNonReserv:
                                LexemType = LexemeType.WORDKEYNONRESERV; break;
                            case State.Integer:
                                LexemType = LexemeType.INTEGER; break;
                            case State.Real:
                                LexemType = LexemeType.REAL; break;
                            case State.NumSystem_2:
                                LexemType = LexemeType.INTEGER; break;
                            case State.NumSystem_8:
                                LexemType = LexemeType.INTEGER; break;
                            case State.NumSystem_16:
                                LexemType = LexemeType.INTEGER; break;
                            case State.Operator:
                                LexemType = LexemeType.OPERATOR; break;
                            case State.Separator:
                                LexemType = LexemeType.SEPARATOR; break;
                            case State.EndFile:
                                LexemType = LexemeType.ENDFILE; break;
                        }

                        int newSymbolNum = (cursor + 1) + symbolNum - foundLexeme.Length;
                        cursor = cursor + symbolNum;
                        symbolNum = 0;


                        if (currentState == State.Error)
                        {
                            lastLexeme = new LexemeData(lineNum, newSymbolNum, LexemeType.ERROR, error, foundLexeme);
                            return lastLexeme;
                        }

                        currentState = State.Start;
                        lastLexeme = new LexemeData(lineNum, newSymbolNum, LexemType, lexemeValue, foundLexeme);

                        foundLexeme = null;
                        lexemeReceived = true;

                        return lastLexeme;
                    }

                }

                if (cursor >= lastLineFile?.Length)
                {
                    NewLine();
                }

                if (lastLineFile == null)
                {
                    lastLexeme = new LexemeData(0, 0, LexemeType.ENDFILE, "", null);

                    return lastLexeme;
                }
            }
            return lastLexeme;
        }
        State NextState(char symbol)
        {
            SymbolType byteType = new SymbolType();
            State nextState;

            if (Char.IsDigit(symbol)) byteType = SymbolType.Number;
            if (Char.IsLetter(symbol)) byteType = SymbolType.Letter;
            if (operations.Any(sm => sm == symbol)) byteType = SymbolType.OperationSign;
            if (separators.Any(sm => sm == symbol)) byteType = SymbolType.SeparatorSign;
            if (exceptsign.Any(sm => sm == symbol)) byteType = SymbolType.ExceptSign;

            if(symbol == '}')
            {
                error = $"({lineNum},{symbolNum + 1})Fatal: illegal character \"'}}'\"";
                return State.Error;
            }

            if (symbol == ' ')
            {
                if (currentState == State.Start)
                {
                    return State.Start;
                }
                if (pairSymbol == false)
                {
                    return State.EndToken;
                }
            }

            if (foundLexeme == "/" && symbol == '/')
            {
                SkipСomment("small");
                return State.Comment;
            }
            if (symbol == '{' && foundLexeme == null)
            {
                SkipСomment("big");
                if (currentState == State.Start)
                {
                    return State.Comment;
                }
                else
                {
                    error = $"({lineNum},{symbolNum}) Fatal: Unexpected end of file";
                    return State.Error;
                }
            }

            nextState = SearchExcpection(symbol, byteType);
            if (nextState != State.Start)
            {
                return nextState;
            }

            StateTransition transition = new StateTransition(currentState, byteType);
            if (!transitions.TryGetValue(transition, out nextState))
            {
                error = $"({lineNum},{symbolNum + 1}) Fatal: Syntax error, \";\" expected";
                return State.Error;
            }
            return nextState;
        }
        State SearchExcpection(char symbol, SymbolType symbolType)
        {
            //исключения цифры
            if (foundLexeme != null && currentState == State.Integer)
            {
                if (foundLexeme.Length > 0)
                {
                    if (foundLexeme[^1] == '.' && symbolType != SymbolType.Number && symbol != 'e')
                    {
                        foundLexeme = foundLexeme.Remove(foundLexeme.Length - 1, 1);
                        cursor -= 1;
                        return State.EndToken;
                    }
                    if (currentState == State.Integer && foundLexeme[^1] == '.' && symbol == '.')
                    {
                        foundLexeme = foundLexeme.Remove('.');
                        cursor -= 1;
                        return State.EndToken;
                    }

                    if (symbol == 'e' || (foundLexeme[^1] == '.' && symbolType == SymbolType.Number)) currentState = State.Real;

                    if (symbol == '.') return State.Integer;
                }
            }

            //исключение реал
            if (currentState == State.Real)
            {
                FoundReal();
                
                if (error == null)
                {
                    return State.EndToken;
                }
                else
                {
                    return State.Error;
                }
            }

            //лишнее поиск реала
            /*
            if (symbol == '%' && currentState == State.Start) return State.NumSystem_2;
            if (symbol == '&' && currentState == State.Start) return State.NumSystem_8;
            if (symbol == '$' && currentState == State.Start) return State.NumSystem_16;
            if (currentState == State.NumSystem_2)
            {
                if (symbol == '0' || symbol == '1')
                    return State.NumSystem_2;
                else
                {
                    error = $"({lineNum},{symbolNum}) Fatal: Syntax error, \";\" expected";
                    return State.Error;
                }
            }
            if (currentState == State.NumSystem_8)
            {
                if (symbol >= '0' && symbol <= '7')
                    return State.NumSystem_8;
                else
                {
                    error = $"({lineNum},{symbolNum}) Fatal: Lexical error, \";\" expected";
                    return State.Error;
                }
            }
            if (currentState == State.NumSystem_16 && symbolType == SymbolType.Letter)
            {
                char aUp = char.ToUpper(symbol);
                if (aUp >= 'A' && aUp <= 'F')
                    return State.NumSystem_16;
                else
                {
                    error = $"({lineNum},{symbolNum}) Fatal: Syntax error, \";\" expected";
                    return State.Error;
                }
            }
            */


            ///строчка
            if ((symbol == '\'' || symbol == '#') && foundLexeme == null)
            {
                FoundString();
                currentState= State.String;
                if (error == null)
                {
                    return State.EndToken;
                }
                else
                {
                    return State.Error;
                }
            }

            //парные операнты
            if (currentState == State.Operator && symbolType == SymbolType.OperationSign)
            {
                string pair = "";
                pair = foundLexeme + symbol.ToString();
                if (pair_operations.Any(sm => sm == pair))
                {
                    symbolNum += 1;
                    foundLexeme += symbol.ToString();
                    currentState = State.Operator;
                    return State.EndToken;
                }
                else
                {
                    return State.EndToken;
                }
            }

            //парные разделители
            if (currentState == State.Separator)
            {
                if(symbolType == SymbolType.SeparatorSign)
                {
                    string pair = "";
                    pair = foundLexeme + symbol.ToString();
                    if (pair == "..")
                    {
                        return State.Separator;
                    }
                }
                else
                {
                    return State.EndToken;
                }

            }


            return State.Start;
        }
        public void FoundReal() 
        {
            foundLexeme = null;
            symbolNum = 0;
            var point_found = false;
            var e_found = false;

            for (int j = cursor; j < lastLineFile.Length; j++)
            {
                char symbol = char.Parse(lastLineFile[j].ToString().ToLower());
                if (Char.IsDigit(symbol))
                {
                    foundLexeme += symbol;
                    symbolNum += 1;
                    continue;
                }
                if (symbol == '.')
                {
                    if (!point_found)
                    {
                        foundLexeme += symbol;
                        symbolNum += 1;
                        point_found = true;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                if (symbol == 'e')
                {
                    if (!e_found)
                    {
                        foundLexeme += symbol;
                        symbolNum += 1;
                        e_found = true;
                        point_found = true;
                        continue;
                    }
                    else
                    {
                        error = $"({lineNum},{symbolNum + 1}) Fatal: Lexical error, \";\" expected";
                        return;
                    }
                }
                if (symbol == '+' || symbol == '-')
                {
                    if (foundLexeme[^1] == 'e') 
                    {
                        foundLexeme += symbol;
                        symbolNum += 1;
                        continue;
                    }
                    else
                    {
                        if(foundLexeme[^1] == '-' || foundLexeme[^1] == '+')
                        {
                            error = $"({lineNum},{symbolNum + 1}) Fatal: Lexical error, expected number";
                            return;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                if (separators.Any(sm => sm == lastLineFile[j]) || operations.Any(sm => sm == lastLineFile[j]) || lastLineFile[j] == ' ')
                {
                    break;
                }

                error = $"({lineNum},{symbolNum + 1}) Fatal: Lexical error, expected number";
                return;
            }

            if (foundLexeme[^1] == '+' || foundLexeme[^1] == '-' || foundLexeme[^1] == 'e')
            {
                error = $"({lineNum},{symbolNum + 1}) Fatal: Lexical error, expected number";
                return;
            }
        }
        public void FoundString()
        {
            symbolNum = 0;
            var start_string = false;
            var start_char = false;
            for (int j = cursor; j < lastLineFile.Length; j++)
            {
                if (lastLineFile[j] == '\'')
                {
                    foundLexeme += lastLineFile[j];
                    start_char = false;

                    if (!start_string)
                    {
                        if (j >= lastLineFile.Length - 1)
                        {
                            error = $"({lineNum},{symbolNum + 2}) Fatal: Lexical error, \"'\" expected";
                            return;
                        }

                        start_string = true;
                        symbolNum += 1;
                        continue;
                    }
                    else
                    {
                        start_string = false;
                        symbolNum += 1;
                        continue;
                    }

                }
                if (lastLineFile[j] == '#' && !start_string)
                {
                    foundLexeme += lastLineFile[j];
                    start_char = true;
                    symbolNum += 1;
                    continue;
                }
                if (start_char)
                {
                    if (Char.IsDigit(lastLineFile[j]))
                    {
                        foundLexeme += lastLineFile[j];
                        symbolNum += 1;
                        continue;
                    }
                    if (separators.Any(sm => sm == lastLineFile[j]) || operations.Any(sm => sm == lastLineFile[j]) || lastLineFile[j] == ' ')
                    {
                        return;
                    }
                    else
                    {
                        error = $"({lineNum},{symbolNum + 1}) Fatal: Lexical error, expected number";
                        return;
                    }
                }
                if (start_string)
                {
                    foundLexeme += lastLineFile[j];
                    symbolNum += 1;
                }
                else
                {
                    if (separators.Any(sm => sm == lastLineFile[j]) || operations.Any(sm => sm == lastLineFile[j]) || lastLineFile[j] == ' ')
                    {
                        return;
                    }
                    else
                    {
                        error = $"({lineNum},{symbolNum + 1}) Fatal: Lexical error, \";\" expected";
                        return;
                    }
                }
            }

            if (start_string)
            {
                error = $"({lineNum},{symbolNum + 1}) Fatal: Lexical error, \";\" expected";
                return;
            }

        }
        public void SkipСomment(string commentType)
        {
            if (commentType == "small")
            {
                foundLexeme = null;
                currentState = State.Comment;
                cursor = lastLineFile.Length;
                return;
            }

            if (commentType == "big")
            {
                foundLexeme = null;
                currentState = State.Start;

                while (lastLineFile != null)
                {
                    for (int j = cursor; j < lastLineFile.Length; j++)
                    {
                        cursor = j + 1;
                        if (lastLineFile[j] == '}') return;
                    }
                    NewLine();
                }
                if (lastLineFile == null)
                {
                    error = $"({lineNum},{cursor}) Fatal: Unexpected end of file";
                    lastLexeme = new LexemeData(lineNum, symbolNum, LexemeType.ERROR, error, null);
                    currentState = State.Error;
                    return;
                }
            }
        }
        public void NewLine()
        {
            lastLineFile = reader.ReadLine();

            symbolNum = 0;
            cursor = 0;
            lineNum += 1;

            return;
        }


        private object SearchValueLexeme()
        {
            if (currentState == State.Identifier)
            {
                if (foundLexeme.Length > 127)
                {
                    currentState = State.Error;
                    error = $"({lineNum},{symbolNum}) Fatal: Overflow string";
                    lastLexeme = new LexemeData(lineNum, symbolNum, LexemeType.ERROR, error, null);
                    return null;
                }
                if (wordsKey.Contains(foundLexeme)) currentState = State.WordsKey;
                if (wordsKeyNonReserv.Contains(foundLexeme)) currentState = State.WordsKeyNonReserv;
                return foundLexeme;
            }
            if (currentState == State.String)
            {
                string lastLexemeValue = "";
                int stringLength = 0;
                var double_quote = false;
                var start_string = false;

                if (foundLexeme.Length >= 255)
                {
                    stringLength = 256;
                }
                else
                {
                    stringLength = foundLexeme.Length;
                }
                for (int i = 0; i < stringLength; i++)
                {
                    if (foundLexeme[i] == '\'')
                    {
                        start_string = !start_string;
                        if (double_quote)
                        {
                            lastLexemeValue += '\'';
                            double_quote = false;
                            continue;
                        }
                        if (i > 0)
                        {
                            double_quote = true;
                        }
                        continue;
                    }
                    double_quote = false;

                    if (foundLexeme[i] == '#' && !start_string)
                    {
                        i += 1;
                        string numberChar = "";
                        while (Char.IsDigit(foundLexeme[i]))
                        {
                            numberChar += foundLexeme[i];
                            if (i < (foundLexeme.Length - 1))
                            {
                                i++;
                            }
                            else
                            {
                                i++;
                                break;
                            }

                        }
                        int symbol = Int32.Parse(numberChar);
                        if (symbol > 65535)
                        {
                            currentState = State.Error;
                            error = $"({lineNum},{symbolNum}) Fatal: Overflow symbol in string";
                            lastLexeme = new LexemeData(lineNum, symbolNum, LexemeType.ERROR, error, null);
                            currentState = State.Error;
                            break;
                        }
                        numberChar = "";
                        numberChar += (char)symbol;
                        lastLexemeValue += numberChar;
                        i -= 1;
                        continue;
                    }

                    lastLexemeValue += foundLexeme[i];
                }
                return lastLexemeValue;
            }
            if (currentState == State.Integer)
            {
                if (!char.IsDigit(foundLexeme[^1]))
                {
                    currentState = State.Real;
                    this.foundLexeme += '0';
                    symbolNum += 1;
                }
                else
                {
                    long lexInt = long.Parse(foundLexeme);
                    if (lexInt <= 2147483647)
                    {
                        return lexInt;
                    }
                    else
                    {
                        error = $"({lineNum},{symbolNum}) Fatal: Going outside";
                        lastLexeme = new LexemeData(lineNum, symbolNum, LexemeType.ERROR, error, null);
                        currentState = State.Error;
                    }
                }
            }
            if (currentState == State.Real)
            {
                string convertLexeme;
                double value = 0;
                convertLexeme = foundLexeme.Replace('.', ',');
                convertLexeme = convertLexeme.ToLower();

                value = double.Parse(convertLexeme);

                return value;
            }
            if (currentState == State.NumSystem_2 || currentState == State.NumSystem_8 || currentState == State.NumSystem_16)
            {
                int numSystem = 10;
                long numValue = 0;
                switch (currentState)
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
                if (foundLexeme.Length < 2)
                {
                    currentState = State.Error;
                    error = $"({lineNum},{symbolNum}) Error: Invalid integer expression";
                    lastLexeme = new LexemeData(lineNum, symbolNum, LexemeType.ERROR, error, null);
                    currentState = State.Error;
                    return "";
                }
                for (int j = 1; j < foundLexeme.Length; j++)
                {
                    for (int k = 1; k < foundLexeme.Length; k++)
                    {
                    }
                    if ((foundLexeme[j] >= 'A' && foundLexeme[j] <= 'F') ||
                        (foundLexeme[j] >= 'a' && foundLexeme[j] <= 'f'))
                    {
                        uint aUint = ((uint)char.ToLower(foundLexeme[j]) - (uint)'a') + 10;
                        numValue = (numValue * numSystem) + aUint;
                    }
                    else
                        numValue = (numValue * numSystem) + (foundLexeme[j] - '0');
                }

                if (numValue > 2147483647)
                {
                    currentState = State.Error;
                    error = $"({lineNum},{symbolNum}) Fatal: Going outside";
                    lastLexeme = new LexemeData(lineNum, symbolNum, LexemeType.ERROR, error, null);
                    currentState = State.Error;
                    return "";
                }
                currentState = State.Integer;
                return numValue;
            }

            return foundLexeme;
        }
    }

}
