using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer
{
    public enum LexemeType
    {
        IDENTIFIER,
        STRING,
        WORDKEY,
        WORDKEYNONRESERV,
        INTEGER,
        REAL,
        SEPARATOR,
        OPERATOR,
        ENDFILE,
        ERROR,
    }

    public class LexemeData
    {
        public LexemeData(int numbLine, int numbSymbol, LexemeType lexemeType, object lexemeValue, string lexemeSource)
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
