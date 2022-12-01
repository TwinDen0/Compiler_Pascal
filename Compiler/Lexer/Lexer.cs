using System.IO;
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
        //public static List<LexemeData> lexems;
        private LexemeData lastLexeme;
        private readonly StreamReader reader;
        private string lastString;
        private int lineNum;
        private int symbolNum;
        public string foundLexeme;
        private string lexemeValue;
        private bool pairSymbol;
        private char firstPairSymbol;
        private string error;
        private bool flagEndFile;
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

        char[] operations = { '=', ':', '+', '-', '*', '/', '<', '>' };
        string[] pair_operations = { "<<", ">>", "**", "<>", "><", "<=", ">=", ":=", "+=", "-=", "*=", "/=" };
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
            //List<LexemeData> lexems = new List<LexemeData>();
            currentState = State.Start;
            lastLexeme = null;
            reader = new StreamReader(filePath);
            lastString = reader.ReadLine();
            transitions = new Dictionary<StateTransition, State>{
                {new StateTransition(State.Start, SymbolType.Letter), State.Identifier},
                {new StateTransition(State.Start, SymbolType.Number), State.Integer},
                {new StateTransition(State.Start, SymbolType.OperationSign), State.Operator},
                {new StateTransition(State.Start, SymbolType.SeparatorSign), State.Separator},

                {new StateTransition(State.Identifier, SymbolType.Letter), State.Identifier},
                {new StateTransition(State.Identifier, SymbolType.Number), State.Identifier},
                {new StateTransition(State.Identifier, SymbolType.OperationSign), State.EndToken},
                {new StateTransition(State.Identifier, SymbolType.SeparatorSign), State.EndToken},

                {new StateTransition(State.Char, SymbolType.Number), State.Char},
                {new StateTransition(State.Char, SymbolType.OperationSign), State.EndToken},
                {new StateTransition(State.Char, SymbolType.SeparatorSign), State.EndToken},

                {new StateTransition(State.String, SymbolType.Letter), State.String},
                {new StateTransition(State.String, SymbolType.Number), State.String},
                {new StateTransition(State.String, SymbolType.OperationSign), State.String},
                {new StateTransition(State.String, SymbolType.SeparatorSign), State.String},
                {new StateTransition(State.String, SymbolType.ExceptSign), State.String},

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
            pairSymbol = false;
            firstPairSymbol = ' ';
            error = "";
            flagEndFile = false;
            cursor = 0;
        }
        public LexemeData PeekLexeme()
        {
            return lastLexeme;
        }
        public LexemeData GetLexeme()
        {
            if(lastLexeme != null)
            {
                if (lastLexeme.LexemeType == LexemeType.ERROR) return null;
            }

            if (lastString == null)
            {
                lastLexeme = new LexemeData(0, 0, LexemeType.ENDFILE, "", 0);
                Console.Write("EndFile");
                return lastLexeme;
            }

            var lexemeReceived = false;

            while (!lexemeReceived)
            {
                if (cursor >= lastString.Length)
                {
                    lastString = reader.ReadLine();

                    if (lastString == null)
                    {
                        if (pairSymbol == false)
                        {
                            lastLexeme = new LexemeData(0, 0, LexemeType.ENDFILE, "", 0);
                            Console.Write("EndFile");
                            return lastLexeme;
                        } 
                        else
                        {
                            error = $"({lineNum},{cursor}) Fatal: Unexpected end of file";
                            lastLexeme = new LexemeData(lineNum, symbolNum, LexemeType.ERROR, "", 0);
                            Console.Write(error);
                            return lastLexeme;
                        }
                    }

                    symbolNum = 0;
                    cursor = 0;
                    lineNum += 1;
                }

                for (int i = cursor; i < lastString.Length; i++)
                {
                    char symbol = lastString[i];

                    if (currentState == State.Start && symbol == ' ')
                    {
                        symbolNum += 1;
                        if((i == lastString.Length - 1) && (foundLexeme == null))
                        {
                            cursor = lastString.Length;
                            break;
                        }
                        continue;
                    }

                    nextStates = NextState(symbol);

                    if (currentState == State.Comment || firstPairSymbol == '{')
                    {
                        if (symbol == '}')
                        {
                            firstPairSymbol = ' ';
                            pairSymbol = false;
                            cursor = i + 1;
                            break;
                        }
                        else
                        {
                            if (i == lastString.Length - 1)
                            {
                                cursor = lastString.Length;
                                symbolNum = 0;
                                lineNum += 1;
                                break;
                            }
                            continue;
                        }
                    }

                    if (nextStates != State.EndToken)
                    {
                        currentState = nextStates;
                        foundLexeme += symbol;
                        symbolNum += 1;
                        if(i == lastString.Length - 1)
                        {
                            i += 1;
                            nextStates = State.EndToken;
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

                        int newSymbolNum = cursor + 1 + symbolNum - foundLexeme.Length;
                        cursor = cursor + symbolNum;
                        symbolNum = 0;


                        if (currentState == State.Error)
                        {
                            Console.WriteLine(error);
                            lastLexeme = new LexemeData(lineNum, newSymbolNum, LexemeType.ERROR, error, foundLexeme);
                            return lastLexeme;
                        }

                        currentState = State.Start;
                        lastLexeme = new LexemeData(lineNum, newSymbolNum, LexemType, lexemeValue, foundLexeme);

                        Console.WriteLine($"{lineNum},\t{newSymbolNum},\t{LexemType},\t{lexemeValue},\t{foundLexeme}");
                        //lexems.Add(lastLexeme);
                        foundLexeme = null;
                        lexemeReceived = true;
                        break;
                    }
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
            if (symbol == ' ' && currentState != State.Comment && pairSymbol == false) return State.EndToken;

            nextState = SearchExcpection(symbol, byteType);
            if (nextState != State.Start) return nextState;

            StateTransition transition = new StateTransition(currentState, byteType);
            if (!transitions.TryGetValue(transition, out nextState))
            {
                error = $"({lineNum},{symbolNum}) Fatal: Syntax error, \";\" expected";
                nextState = State.Error;
                return nextState;
            }
            return nextState;
        }
        State SearchExcpection(char symbol, SymbolType byteType)
        {
            //комменты
            if (currentState == State.Comment) return State.Comment;
            if (foundLexeme == "/" && symbol == '/') return State.Comment;
            if (symbol == '{' && currentState != State.String)
            {
                firstPairSymbol = '{';
                pairSymbol = true;
                return State.Comment;
            }

            //исключения цифры
            if (foundLexeme != null)
            {
                if (foundLexeme.Length > 0)
                {
                    if (byteType == SymbolType.Number && foundLexeme[^1] == '.' && currentState == State.Integer) return State.Real;
                    if (symbol == '.' && foundLexeme[^1] == '.' && currentState == State.Integer)
                    {
                        symbolNum -= 1;
                        foundLexeme = foundLexeme.Remove(foundLexeme.Length - 1, 1);
                        return State.EndToken;
                    }
                    if (symbol == '.' && currentState == State.Integer) return State.Integer;
                }
            }
            if (symbol == 'e' && currentState == State.Integer)
            {
                return State.Real;
            }
            if (symbol == '+' && currentState == State.Real && pairSymbol == true)
            {
                pairSymbol = false;
                return State.Real;
            }
            if (symbol == '-' && currentState == State.Real && pairSymbol == true)
            {
                pairSymbol = false;
                return State.Real;
            }
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
                    error = $"({lineNum},{symbolNum}) Fatal: Syntax error, \";\" expected";
                    return State.Error;
                }
            }
            if (currentState == State.NumSystem_16 && byteType == SymbolType.Letter)
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

            //исключение строчки
            if (symbol == '\'' && (currentState == State.Start || currentState == State.String || currentState == State.Char))
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
            if (symbol == '#' && (currentState == State.Start || (currentState == State.String && pairSymbol == false))) return State.Char;
            if (symbol == '#' && currentState == State.Char) return State.Char;
            if (currentState == State.Char && symbol == '\'') return State.String;
            if (pairSymbol == false && symbol != '#' && symbol != '\'' && currentState == State.String)
            {
                error = $"({lineNum},{symbolNum}) Fatal: Syntax error, \";\" expected";
                return State.Error;
            }
            if (currentState == State.String) return State.String;
            

            //парные операнты
            if (currentState == State.Operator && byteType == SymbolType.OperationSign)
            {
                string pair = "";
                pair = foundLexeme[^1] + symbol.ToString();
                if (pair_operations.Any(sm => sm == pair))
                   return State.Operator;
            }

            //парные разделители
            if (foundLexeme != null)
            {
                if (symbol == '.' && foundLexeme[^1] == '.' && currentState == State.Separator)
                {
                    return State.Separator;
                }
            }

            //конец
            //файла/////////////////////////////////////////////////////////////////////////////////////////////////
            if (symbol == '.' && foundLexeme == "end" && currentState == State.Identifier)
            {
                foundLexeme = null;

                flagEndFile = true;
                return State.EndToken;
            }

            return State.Start;
        }
        private string SearchValueLexeme()
        {
            string lastLexemeValue = foundLexeme;
            if (currentState == State.Identifier)
            {
                if (foundLexeme.Length > 255)
                {
                    currentState = State.Error;
                    error = $"({lineNum},{symbolNum}) Fatal: Overflow string";
                    return "";
                }
                if (wordsKey.Contains(foundLexeme)) currentState = State.WordsKey;
                if (wordsKeyNonReserv.Contains(foundLexeme)) currentState = State.WordsKeyNonReserv;
                lastLexemeValue = foundLexeme;
            }
            if (currentState == State.String || currentState == State.Char)
            {
                lastLexemeValue = "";

                if (pairSymbol == true)
                {
                    error = $"({lineNum},{symbolNum}) Fatal: Syntax error, \";\" expected";
                    currentState = State.Error;
                    return "";
                }

                for (int i = 0; i < foundLexeme.Length; i++)
                {
                    if (foundLexeme[i] == '\'')
                    {
                        int j = i + 1;
                        var endstring = false;
                        while (!endstring)
                        {
                            if (j >= 0 && j < foundLexeme.Length)
                            {
                                if (foundLexeme[j] == '\'')
                                {
                                    j += 1;
                                    if (j < foundLexeme.Length)
                                    {
                                        if (foundLexeme[j] == '\'')
                                        {
                                            lastLexemeValue += "'";
                                            j += 1;
                                            continue;
                                        }
                                    }

                                    endstring = true;
                                    continue;
                                }


                                lastLexemeValue += foundLexeme[j];
                                j += 1;
                            }
                            else
                            {
                                return "";
                            }
                        }
                        i = j - 1;
                        continue;
                    }
                    
                    if (foundLexeme[i] == '#')
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

                if (lastLexemeValue.Length > 255)
                {
                    currentState = State.Error;
                    error = $"({lineNum},{symbolNum}) Fatal: Overflow string";
                    return "";
                }
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
                        lastLexemeValue = foundLexeme;
                    }
                    else
                    {
                        currentState = State.Error;
                        error = $"({lineNum},{symbolNum}) Fatal: Going outside";
                        // error = "Warning(s) Issued";
                    }
                }
            }
            if (currentState == State.Real)
            {
                double value = 0;
                foundLexeme = foundLexeme.Replace(".", ",");
                foundLexeme = foundLexeme.ToUpper();
                int mantis;

                if (foundLexeme[^1] == 'E' || foundLexeme[^1] == '.')
                {
                    foundLexeme += '0';
                    symbolNum += 1;
                }

                switch (foundLexeme[0])
                {
                    case '%':
                        foundLexeme = foundLexeme.Remove(0, 1);
                        if (foundLexeme.IndexOf(',') != -1)
                        {
                            mantis = Convert.ToInt32(foundLexeme.Substring(0, foundLexeme.IndexOf(',')), 2);
                            foundLexeme = foundLexeme.Remove(0, foundLexeme.IndexOf(','));
                        }
                        else
                        {
                            mantis = Convert.ToInt32(foundLexeme.Substring(0, foundLexeme.IndexOf('E')), 2);
                            foundLexeme = foundLexeme.Remove(0, foundLexeme.IndexOf('E'));
                        }
                        foundLexeme = mantis.ToString() + foundLexeme;
                        value = Convert.ToDouble(foundLexeme);
                        break;
                    case '&':
                        foundLexeme = foundLexeme.Remove(0, 1);
                        if (foundLexeme.IndexOf(',') != -1)
                        {
                            mantis = Convert.ToInt32(foundLexeme.Substring(0, foundLexeme.IndexOf(',')), 8);
                            foundLexeme = foundLexeme.Remove(0, foundLexeme.IndexOf(','));
                        }
                        else
                        {
                            mantis = Convert.ToInt32(foundLexeme.Substring(0, foundLexeme.IndexOf('E')), 8);
                            foundLexeme = foundLexeme.Remove(0, foundLexeme.IndexOf('E'));
                        }
                        foundLexeme = mantis.ToString() + foundLexeme;
                        value = Convert.ToDouble(foundLexeme);
                        break;
                    case '$':
                        foundLexeme = foundLexeme.Remove(0, 1);
                        if (foundLexeme.IndexOf(',') != -1)
                        {
                            mantis = Convert.ToInt32(foundLexeme.Substring(0, foundLexeme.IndexOf(',')), 16);
                            foundLexeme = foundLexeme.Remove(0, foundLexeme.IndexOf(','));
                        }
                        else
                        {
                            mantis = Convert.ToInt32(foundLexeme.Substring(0, foundLexeme.IndexOf('E')), 16);
                            foundLexeme = foundLexeme.Remove(0, foundLexeme.IndexOf('E'));
                        }
                        foundLexeme = mantis.ToString() + foundLexeme;
                        value = Convert.ToDouble(foundLexeme);
                        break;
                    default:
                        value = Convert.ToDouble(foundLexeme);
                        break;
                }

                lastLexemeValue = value.ToString("E10").Replace(",", ".");

                if (lastLexemeValue == "∞")
                {
                    lastLexemeValue = "+Inf";
                }
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
                    return "";
                }
                for (int j = 1; j < foundLexeme.Length; j++)
                {
                    for (int k = 1; k < lastLexemeValue.Length; k++)
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
                    return "";
                }
                currentState = State.Integer;
                lastLexemeValue = numValue.ToString();
            }
            if (currentState == State.Operator)
            {
                lastLexemeValue = foundLexeme;
            }
            if (currentState == State.Separator)
            {
                lastLexemeValue = foundLexeme;
            }
            return lastLexemeValue;
        }
    }

}
