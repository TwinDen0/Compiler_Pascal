using System.Xml.Linq;

namespace Compiler
{
    public partial class Parser
    {
        private Lexer _lexer;
        Lexeme current_lexeme;
        SymTableStack symTableStack;
        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            symTableStack = new SymTableStack();
            Dictionary<string, Symbol> builtins = new Dictionary<string, Symbol>();
            builtins.Add("integer", new SymInteger("integer"));
            builtins.Add("real", new SymReal("real"));
            builtins.Add("string", new SymString("string"));
            builtins.Add("write", new SymProc("write"));
            builtins.Add("read", new SymProc("read"));
            builtins.Add("exit", new SymProc("exit"));
            symTableStack.AddTable(new SymTable(builtins));
            symTableStack.AddTable(new SymTable(new Dictionary<string, Symbol>()));
            GetNextLexeme();
        }
        void GetNextLexeme()
        {
            current_lexeme = _lexer.NextLexeme();
        }
        private bool Expect(params object[] requires)
        {
            if (requires[0].GetType() == typeof(LexemeType))
            {
                foreach (LexemeType type in requires)
                {
                    if (current_lexeme.LexemeType == type)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                foreach (object require in requires)
                {
                    if (Equals(current_lexeme.LexemeValue, require))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        private void Require(params object[] requires)
        {
            if (!Expect(requires))
            {
                if (requires[0].GetType() == typeof(LexemeType))
                {
                    throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) Fatal: expected {requires[0]}");
                }
                else
                {
                    string? requiresStr = requires[0].ToString();
                    if (requires[0].GetType() == typeof(Separator) || requires[0].GetType() == typeof(Operation))
                    {
                        requiresStr = Lexer.convert_sign.FirstOrDefault(x => x.Value.ToString() == requires[0].ToString()).Key;
                    }
                    throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) Fatal: expected '{requiresStr}'");
                }
            }
            if (requires[0].GetType() != typeof(LexemeType))
            {
                GetNextLexeme();
            }
        }
        public NodeMainProgram ParseProgram()
        {
            string name_program = null;
            List<NodeDefs> types = new List<NodeDefs>();
            BlockStmt body;
            if (Expect(KeyWord.PROGRAM))
            {
                GetNextLexeme();
                Require(LexemeType.IDENTIFIER);
                name_program = current_lexeme.LexemeValue.ToString();
                GetNextLexeme();
                Require(Separator.SEMICOLON);
            }
            types = ParseDefs();
            Require(KeyWord.BEGIN);
            body = ParseBlock();
            Require(Separator.POINT);
            return new NodeMainProgram(name_program, types, body);
        }
        public BlockStmt ParseBlock()
        {
            List<NodeStatement> body = new List<NodeStatement>();
            while (!Expect(KeyWord.END))
            {
                body.Add(ParseStatement());
                if (Expect(Separator.SEMICOLON))
                {
                    GetNextLexeme();
                    continue;
                }
                Require(KeyWord.END);
            }
            Require(KeyWord.END);
            return new BlockStmt(body);
        }

        public string PrintSymTable()
        {
            string PrintSymProc(Dictionary<string, Symbol> dic, int index, int depth)
            {
                string res = "";
                SymProc symProc = (SymProc)dic.ElementAt(index).Value;
                Dictionary<string, Symbol> dicLocals = symProc.GetLocals().GetData();
                if (dicLocals.Count > 0)
                {
                    for (int i = 0; i < depth; i++)
                    {
                        res += "\t";
                    }
                    res += $"locals of procedure \"{dic.ElementAt(index).Key}\": \r\n";
                    for (int z = 0; z < dicLocals.Count; z++)
                    {
                        for (int i = 0; i < depth; i++)
                        {
                            res += "\t";
                        }
                        res += dicLocals.ElementAt(z).Key.ToString() + ": " + dicLocals.ElementAt(z).Value.GetType().Name + "\r\n";
                        if (dicLocals.ElementAt(z).Value.GetType() == typeof(SymProc))
                        {
                            res += PrintSymProc(dicLocals, z, depth + 1);
                        }
                    }
                }
                return res;
            }
            string PrintSymRecord(Dictionary<string, Symbol> dic, int index, int depth)
            {
                string res = "";
                SymRecord symRecord = (SymRecord)dic.ElementAt(index).Value;
                Dictionary<string, Symbol> dicLocals = symRecord.GetFields().GetData();
                if (dicLocals.Count > 0)
                {
                    for (int i = 0; i < depth; i++)
                    {
                        res += "\t";
                    }
                    res += $"locals of record \"{dic.ElementAt(index).Key}\": \r\n";
                    for (int z = 0; z < dicLocals.Count; z++)
                    {
                        for (int i = 0; i < depth; i++)
                        {
                            res += "\t";
                        }
                        res += dicLocals.ElementAt(z).Key.ToString() + ": " + dicLocals.ElementAt(z).Value.GetType().Name + "\r\n";
                        if (dicLocals.ElementAt(z).Value.GetType() == typeof(SymRecord))
                        {
                            res += PrintSymRecord(dicLocals, z, depth + 1);
                        }
                    }
                }
                return res;
            }
            string res = "\r\nSymbol Tables:\r\n";
            for (int i = 0; i < symTableStack.GetCountTables(); i++)
            {
                switch (i)
                {
                    case 0:
                        res += $"builtins:\r\n";
                        break;
                    case 1:
                        res += $"globals:\r\n";
                        break;
                    default:
                        res += $"table #{i}\r\n";
                        break;
                }
                Dictionary<string, Symbol> dic = symTableStack.GetTable(i).GetData();
                for (int j = 0; j < dic.Count; j++)
                {
                    res += "\t" + dic.ElementAt(j).Key.ToString() + ": " + dic.ElementAt(j).Value.GetType().Name + "\r\n";
                    if (dic.ElementAt(j).Value.GetType() == typeof(SymProc))
                    {
                        res += PrintSymProc(dic, j, 2);
                    }
                    if (dic.ElementAt(j).Value.GetType() == typeof(SymRecord))
                    {
                        res += PrintSymRecord(dic, j, 2);
                    }
                }
            }
            return res;
        }
    }
}
