using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

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
        Integer,
        Real,
        Operator,
        Separator,
        EndToken,
        EndLine,
        EndFile,
    }
    public enum SymbolType
    {
        Letter,
        Number,
        Operation,
        Separator,
        ExceptSign
    }
    public partial class Lexer
    {
        private Lexeme lastLexeme;
        private readonly StreamReader reader;
        private string lastLineFile;
        public int lineNum;
        public int symbolNum;
        public int symbolStart;
        LexemeType lexemType;
        public string foundLexeme;
        private object lexemeValue;
        public int cursor;
        State nextStates;
        public State currentState { get; private set; }
        struct StateTransition
        {
            State _state;
            SymbolType _token;
            public StateTransition(State state, SymbolType token)
            {
                _state = state;
                _token = token;
            }
        }
        Dictionary<StateTransition, State> transitions;
        public static Dictionary<string, object> convert_sign;
        char[] operations = { '=', ':', '+', '-', '*', '/', '<', '>', '{', '@' };
        string[] pair_operations = { "<<", ">>", "**", "<>", "><", "<=", ">=", ":=", "+=", "-=", "*=", "/=" };
        char[] separators = { '.', ',', ';', '(', ')', '[', ']' };
        char[] exceptsign = { '\'', '#', '%', '&', '$', '}' };

        public Lexer(string filePath)
        {
            currentState = State.Start;
            lastLexeme = null;
            reader = new StreamReader(filePath);
            lastLineFile = reader.ReadLine();
            transitions = new Dictionary<StateTransition, State>{
                {new StateTransition(State.Start, SymbolType.Letter), State.Identifier},
                {new StateTransition(State.Start, SymbolType.Number), State.Integer},
                {new StateTransition(State.Start, SymbolType.Operation), State.Operator},
                {new StateTransition(State.Start, SymbolType.Separator), State.Separator},

                {new StateTransition(State.Identifier, SymbolType.Letter), State.Identifier},
                {new StateTransition(State.Identifier, SymbolType.Number), State.Identifier},
                {new StateTransition(State.Identifier, SymbolType.Operation), State.EndToken},
                {new StateTransition(State.Identifier, SymbolType.Separator), State.EndToken},

                {new StateTransition(State.Integer, SymbolType.Number), State.Integer},
                {new StateTransition(State.Integer, SymbolType.Operation), State.EndToken},
                {new StateTransition(State.Integer, SymbolType.Separator), State.EndToken},

                {new StateTransition(State.Real, SymbolType.Number), State.Real},
                {new StateTransition(State.Real, SymbolType.Operation), State.EndToken},
                {new StateTransition(State.Real, SymbolType.Separator), State.EndToken},

                {new StateTransition(State.Operator, SymbolType.Letter), State.EndToken},
                {new StateTransition(State.Operator, SymbolType.Number), State.EndToken},
                {new StateTransition(State.Operator, SymbolType.Separator), State.EndToken},
                {new StateTransition(State.Operator, SymbolType.ExceptSign), State.EndToken},

                {new StateTransition(State.Separator, SymbolType.Letter), State.EndToken},
                {new StateTransition(State.Separator, SymbolType.Number), State.EndToken},
                {new StateTransition(State.Separator, SymbolType.Operation), State.EndToken},
                {new StateTransition(State.Separator, SymbolType.Separator), State.EndToken},
                {new StateTransition(State.Separator, SymbolType.ExceptSign), State.EndToken},
            };
            convert_sign = new Dictionary<string, object>{
                {".", Separator.POINT},
                {",", Separator.COMMA},
                {";", Separator.SEMICOLON},
                {"(", Separator.OPEN_PARENTHESIS},
                {")", Separator.CLOSE_PARENTHESIS},
                {"[", Separator.OPEN_BRACKET},
                {"]", Separator.CLOSE_BRACKET},
                {"..", Separator.DOUBLE_POINT},

                {"=", Operation.EQUAL},
                {":", Operation.COLON},
                {"+", Operation.PLUS},
                {"-", Operation.MINUS},
                {"*", Operation.MULTIPLY},
                {"/", Operation.DIVIDE},
                {">", Operation.GREATER},
                {"<", Operation.LESS},
                {"@", Operation.AT},
                {"<<", Operation.BITWISE_SHIFT_TO_THE_LEFT},
                {">>", Operation.BITWISE_SHIFT_TO_THE_RIGHT},
                {"<>", Operation.NOT_EQUAL},
                {"><", Operation.SYMMETRICAL_DIFFERENCE},
                {"<=", Operation.LESS_OR_EQUAL},
                {">=", Operation.GREATER_OR_EQUAL},
                {":=", Operation.ASSIGNMENT},
                {"+=", Operation.ADDITION},
                {"-=", Operation.SUBTRACRION},
                {"*=", Operation.MULTIPLICATION},
                {"/=", Operation.DIVISION},
                {". ", Operation.POINT_RECORD},
            };
            lineNum = 1;
            symbolStart = 0;
            foundLexeme = null;
            lexemeValue = null;
            cursor = 0;
        }
        public Lexeme GetLexeme()
        {
            return lastLexeme;
        }
        public Lexeme NextLexeme()
        {
            bool lexemeReceived = false;
            while (!lexemeReceived)
            {
                for (int i = cursor; i < lastLineFile?.Length; i++)
                {
                    char symbol = lastLineFile[i];

                    nextStates = NextState(symbol);

                    if (nextStates == State.Start) break;

                    if (nextStates != State.EndToken)
                    {
                        currentState = nextStates;
                        foundLexeme += symbol;
                        symbolStart += 1;
                    }

                    if (nextStates == State.EndToken || i == lastLineFile.Length - 1)
                    {
                        lexemeValue = GetValueLexeme();

                        symbolNum = cursor + 1;
                        lastLexeme = new Lexeme(lineNum, symbolNum, lexemType, lexemeValue, foundLexeme);

                        cursor = cursor + symbolStart;
                        symbolStart = 0;
                        currentState = State.Start;
                        foundLexeme = null;
                        lexemeReceived = true;

                        return lastLexeme;
                    }
                }

                if (cursor >= lastLineFile?.Length)
                {
                    GetNextLine();
                }

                if (lastLineFile == null)
                {
                    lastLexeme = new Lexeme(0, 0, LexemeType.ENDFILE, null, null);
                    return lastLexeme;
                }
            }
            return lastLexeme;
        }
        State NextState(char symbol)
        {
            SymbolType symbolType = new SymbolType();
            State nextState;

            if (Char.IsDigit(symbol)) symbolType = SymbolType.Number;
            if (Char.IsLetter(symbol)) symbolType = SymbolType.Letter;
            if (operations.Any(sm => sm == symbol)) symbolType = SymbolType.Operation;
            if (separators.Any(sm => sm == symbol)) symbolType = SymbolType.Separator;
            if (exceptsign.Any(sm => sm == symbol)) symbolType = SymbolType.ExceptSign;

            if (symbol == ' ')
            {
                if (currentState == State.Start)
                {
                    cursor += 1;
                    return State.Start;
                }
                else
                {
                    return State.EndToken;
                }
            }

            if (symbol == '%' || symbol == '&' || symbol == '$')
            {
                return State.Integer;
            }

            if (foundLexeme == "/" && symbol == '/')
            {
                SkipСomment("small");
                return State.Start;
            }

            if (symbol == '{' && foundLexeme == null)
            {
                SkipСomment("big");
                if (currentState == State.Start)
                {
                    return State.Start;
                }
                else
                {
                    throw new ExceptionCompiler($"({lineNum},{cursor + symbolStart}) Fatal: Unexpected end of file");
                }
            }

            if ((symbol == '\'' || symbol == '#') && foundLexeme == null)
            {
                GetString();
                currentState = State.String;
                return State.EndToken;
            }

            if (currentState == State.Integer)
            {
                if (foundLexeme?[0] == '$' && symbolType == SymbolType.Letter)
                {
                    return State.Integer;
                }
                if (foundLexeme?[^1] == '.' && symbolType != SymbolType.Number && symbol != 'e')
                {
                    foundLexeme = foundLexeme.Remove(foundLexeme.Length - 1, 1);
                    symbolStart -= 1;
                    return State.EndToken;
                }
                if (currentState == State.Integer && foundLexeme?[^1] == '.' && symbol == '.')
                {
                    foundLexeme = foundLexeme.Remove('.');
                    symbolStart -= 1;
                    return State.EndToken;
                }

                if (symbol == 'e' || (foundLexeme?[^1] == '.' && symbolType == SymbolType.Number)) currentState = State.Real;

                if (symbol == '.') return State.Integer;
            }

            if (currentState == State.Real)
            {
                GetReal();
                return State.EndToken;
            }

            if (currentState == State.Operator && symbolType == SymbolType.Operation)
            {
                string pair = "";
                pair = foundLexeme + symbol.ToString();
                if (pair_operations.Any(sm => sm == pair))
                {
                    symbolStart += 1;
                    foundLexeme += symbol.ToString();
                    currentState = State.Operator;
                    return State.EndToken;
                }
                else
                {
                    return State.EndToken;
                }
            }

            if (currentState == State.Separator)
            {
                if (symbolType == SymbolType.Separator)
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

            StateTransition transition = new StateTransition(currentState, symbolType);
            if (!transitions.TryGetValue(transition, out nextState))
            {
                if(currentState == State.Start)
                {
                    throw new ExceptionCompiler($"({lineNum},{cursor + symbolStart + 1}) Fatal: illegal character \"'}}'\"");
                }
                else
                {
                    throw new ExceptionCompiler($"({lineNum},{cursor + symbolStart + 1}) Fatal: Lexical error, for {currentState} not expected \"{symbol}\"");
                }
            }

            return nextState;
        }
        public void GetReal() 
        {
            foundLexeme = null;
            symbolStart = 0;
            var point_found = false;
            var e_found = false;

            for (int j = cursor; j < lastLineFile.Length; j++)
            {
                char symbol = char.Parse(lastLineFile[j].ToString().ToLower());
                if (Char.IsDigit(symbol))
                {
                    foundLexeme += symbol;
                    symbolStart += 1;
                    continue;
                }
                if (symbol == '.')
                {
                    if (!point_found)
                    {
                        foundLexeme += symbol;
                        symbolStart += 1;
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
                        symbolStart += 1;
                        e_found = true;
                        point_found = true;
                        continue;
                    }
                    else
                    {
                        throw new ExceptionCompiler($"({lineNum},{cursor + symbolStart + 1}) Fatal: Lexical error, real cannot contain \"e\" more than once");
                    }
                }
                if (symbol == '+' || symbol == '-')
                {
                    if (foundLexeme[^1] == 'e') 
                    {
                        foundLexeme += symbol;
                        symbolStart += 1;
                        continue;
                    }
                    else
                    {
                        if(foundLexeme[^1] == '-' || foundLexeme[^1] == '+')
                        {
                            throw new ExceptionCompiler($"({lineNum},{cursor + symbolStart + 1}) Fatal: Lexical error, expected \"e\" but found \"{symbol}\"");
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

                throw new ExceptionCompiler($"({lineNum},{cursor + symbolStart + 1}) Fatal: Lexical error, expected number");
            }

            if (foundLexeme[^1] == 'e')
            {
                throw new ExceptionCompiler($"({lineNum},{cursor + symbolStart + 1}) Fatal: Lexical error, real cannot end with \"e\", after \"e\" you should specify a number");
            }
        }
        public void GetString()
        {
            symbolStart = 0;
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
                            throw new ExceptionCompiler($"({lineNum},{cursor + symbolStart + 2}) Fatal: Lexical error, \"'\" expected");
                        }

                        start_string = true;
                        symbolStart += 1;
                        continue;
                    }
                    else
                    {
                        start_string = false;
                        symbolStart += 1;
                        continue;
                    }

                }
                if (lastLineFile[j] == '#' && !start_string)
                {
                    foundLexeme += lastLineFile[j];
                    start_char = true;
                    symbolStart += 1;
                    continue;
                }
                if (start_char)
                {
                    if (Char.IsDigit(lastLineFile[j]))
                    {
                        foundLexeme += lastLineFile[j];
                        symbolStart += 1;
                        continue;
                    }
                    if (separators.Any(sm => sm == lastLineFile[j]) || operations.Any(sm => sm == lastLineFile[j]) || lastLineFile[j] == ' ')
                    {
                        return;
                    }
                    else
                    {
                        throw new ExceptionCompiler($"({lineNum},{cursor + symbolStart + 1}) Fatal: Lexical error, expected number");
                    }
                }
                if (start_string)
                {
                    foundLexeme += lastLineFile[j];
                    symbolStart += 1;
                }
                else
                {
                    if (separators.Any(sm => sm == lastLineFile[j]) || operations.Any(sm => sm == lastLineFile[j]) || lastLineFile[j] == ' ')
                    {
                        return;
                    }
                    else
                    {
                        throw new ExceptionCompiler($"({lineNum},{cursor + symbolStart + 1}) Fatal: Lexical error, missing separator or operations");
                    }
                }
            }

            if (start_string)
            {
                throw new ExceptionCompiler($"({lineNum},{cursor + symbolStart + 1}) Fatal: Lexical error, \"'\" expected");
            }

        }
        public void SkipСomment(string commentType)
        {
            if (commentType == "small")
            {
                foundLexeme = null;
                currentState = State.Start;
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
                    GetNextLine();
                }
                if (lastLineFile == null)
                {
                    throw new ExceptionCompiler($"({lineNum},{cursor + 1}) Fatal: Unexpected end of file");
                }
            }
        }
        private object GetValueLexeme()
        {
            if (currentState == State.Identifier)
            {
                lexemType = LexemeType.IDENTIFIER;

                if (Enum.TryParse((foundLexeme.ToUpper()), out KeyWord res))
                {
                    lexemType = LexemeType.WORDKEY;
                    return res;
                }
                return foundLexeme;
            }
            if (currentState == State.String)
            {
                lexemType = LexemeType.STRING;

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
                            throw new ExceptionCompiler($"({lineNum},{cursor + 1}) Fatal: Overflow symbol in string");
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
                lexemType = LexemeType.INTEGER;

                if (!char.IsDigit(foundLexeme[0]))
                {
                    int value = 0;
                    int numsystem = 0;
                    int valueLexeme = 0;
                    string foundLexemeUp = foundLexeme.ToUpper();

                    switch (foundLexemeUp[0])
                    {
                        case '%': numsystem = 2; break;
                        case '&': numsystem = 8; break;
                        case '$': numsystem = 16; break;
                    }
                    if(numsystem != 0 && (foundLexemeUp[^1] == '%' || foundLexemeUp[^1] == '&' || foundLexemeUp[^1] == '$'))
                    {
                        throw new ExceptionCompiler($"({lineNum},{cursor + 1}) Fatal: Incorrect lexeme");
                    }
                    symbolStart += 1;
                    for (int i = 1; i < foundLexemeUp.Length; i++)
                    {
                        int number = 0;
                        if(foundLexemeUp[i] >= '9' && foundLexemeUp[i] <= 'F')
                        {
                            switch (foundLexemeUp[i])
                            {
                                case 'A': number = 10; break;
                                case 'B': number = 11; break;
                                case 'C': number = 12; break;
                                case 'D': number = 13; break;
                                case 'E': number = 14; break;
                                case 'F': number = 15; break;
                            }
                        }
                        else
                        {
                            if (foundLexemeUp[i] > 'F')
                            {
                                throw new ExceptionCompiler($"({lineNum},{cursor + 1}) Fatal: Overflow of the number system");
                            }

                            number = Int32.Parse(foundLexemeUp[i].ToString());
                        }
                        if (number < numsystem)
                        {
                            valueLexeme = (valueLexeme * numsystem) + number;
                            symbolStart += 1;
                        }
                        else
                        {
                            throw new ExceptionCompiler($"({lineNum},{cursor + 1}) Fatal: Overflow of the number system");
                        }
                    }

                    if (valueLexeme <= 2147483647 && valueLexeme > -2147483647)
                    {
                        return valueLexeme;
                    }
                    else
                    {
                        throw new ExceptionCompiler($"({lineNum},{cursor + 1}) Fatal: Overflow symbol in integer");
                    }
                }

                if (!char.IsDigit(foundLexeme[^1]))
                {
                    currentState = State.Real;
                    this.foundLexeme += '0';
                    symbolStart += 1;
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
                        throw new ExceptionCompiler($"({lineNum},{cursor + 1}) Fatal: Overflow symbol in integer");
                    }
                }
            }
            if (currentState == State.Real)
            {
                lexemType = LexemeType.REAL;

                string convertLexeme;
                double value = 0;
                convertLexeme = foundLexeme.Replace('.', ',');
                convertLexeme = convertLexeme.ToLower();

                value = double.Parse(convertLexeme);

                return value;
            }
            if (currentState == State.Separator)
            {
                lexemType = LexemeType.SEPARATOR;
                object separator;
                convert_sign.TryGetValue(foundLexeme, out separator);
                return separator;
            }
            if (currentState == State.Operator)
            {
                lexemType = LexemeType.OPERATOR;
                object _operator;
                convert_sign.TryGetValue(foundLexeme, out _operator);
                return _operator;
            }
            return foundLexeme;
        }
        public void GetNextLine()
        {
            lastLineFile = reader.ReadLine();

            symbolStart = 0;
            cursor = 0;
            lineNum += 1;

            return;
        }
    }
}