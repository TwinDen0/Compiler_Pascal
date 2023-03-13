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
                    throw new ExceptionCompiler($"({current_lexeme.NumbLine},{current_lexeme.NumbSymbol}) expected {requires[0]}");
                }
                else
                {
                    if (requires[0].GetType() == typeof(Separator) || requires[0].GetType() == typeof(Operation))
                    {
                        requires[0] = Lexer.convert_sign.Where(x => x.Value == (object)requires[0]).FirstOrDefault().Key;
                    }
                    throw new ExceptionCompiler($"expected '{requires[0]}'");
                }
            }
            if (requires[0].GetType() != typeof(LexemeType))
            {
                GetNextLexeme();
            }
        }

        // mainProgram ::= ['progam' identifier ';'] {declarations} block '.'
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

        //  block ::= "begin" [(statement) {";" (statement)}] "end"
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
    }
}
