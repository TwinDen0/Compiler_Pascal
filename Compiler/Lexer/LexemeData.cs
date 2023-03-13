using Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public enum LexemeType
    {
        IDENTIFIER,
        STRING,
        WORDKEY,
        INTEGER,
        REAL,
        SEPARATOR,
        OPERATOR,
        ENDFILE,
    }

    public enum Separator
    {
        POINT,              // .
        COMMA,              // ,
        SEMICOLON,          // ;
        OPEN_PARENTHESIS,   // (
        CLOSE_PARENTHESIS,  // )
        OPEN_BRACKET,       // [
        CLOSE_BRACKET,      // ]
        DOUBLE_POINT        // ..
    }

    public enum Operation
    {
        EQUAL,                      // =
        COLON,                      // :
        PLUS,                       // +
        MINUS,                      // -
        MULTIPLY,                   // *
        DIVIDE,                     // /
        GREATER,                    // >
        LESS,                       // <
        AT,                         // @
        BITWISE_SHIFT_TO_THE_LEFT,  // <<
        BITWISE_SHIFT_TO_THE_RIGHT, // >>
        NOT_EQUAL,                  // <>
        SYMMETRICAL_DIFFERENCE,     // ><
        LESS_OR_EQUAL,              // <=
        GREATER_OR_EQUAL,           // >=
        ASSIGNMENT,                 // :=
        ADDITION,                   // +=
        SUBTRACRION,                // -=
        MULTIPLICATION,             // *=
        DIVISION,                   // /=
        POINT_RECORD,               // .
    }

    public enum KeyWord
    {
        AND, ARRAY, AS, ASM,
        BEGIN, CASE, CONST,
        CONSTRUCTOR, DESTRUCTOR,
        DIV, DO, DOWNTO, ELSE,
        END, FILE, FOR, FOREACH,
        FUNCTION, GOTO, IMPLEMENTATION,
        IF, IN, INHERITED, INLINE,
        INTERFACE, LABEL, MOD,
        NIL, NOT, OBJECT, OF,
        OPERATOR, OR, PACKED,
        PROCEDURE, PROGRAM,
        RECORD, REPEAT, SELF,
        SET, SHL, SHR, STRING,
        THEN, TO, TYPE, UNIT,
        UNTIL, USES, VAR, WHILE,
        WITH, XOR, DISPOSE, EXIT,
        FALSE, NEW, TRUE, CLASS,
        DISPINTERFACE, EXCEPT,
        EXPORTS, FINALIZATION,
        FINALLY, INITIALIZATION,
        IS, LIBRARY, ON, OUT,
        PROPERTY, RAISE,
        RESOURCESTRING, THREADVAR,
        TRY
    }

    public class Lexeme
    {
        public Lexeme(int numbLine, int numbSymbol, LexemeType lexemeType, object lexemeValue, string lexemeSource)
        {
            NumbLine = numbLine;
            NumbSymbol = numbSymbol;
            LexemeType= lexemeType;
            LexemeValue= lexemeValue;
            LexemeSource = lexemeSource;
        }
        public int NumbLine { get; set; }
        public int NumbSymbol { get; set; }
        public LexemeType LexemeType;
        public object LexemeValue { get; }
        public string LexemeSource { get; }

        public override string ToString()
        {
            return $"{NumbLine}\t{NumbSymbol}\t{LexemeType}\t{LexemeValue}\t{LexemeSource}";
        }
    }
}
