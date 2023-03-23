using System.Security.AccessControl;

namespace Compiler
{
    public partial class Parser
    {
        public List<NodeDefs> ParseDefs()
        {
            List<NodeDefs> types = new List<NodeDefs>();
            while (Expect(KeyWord.VAR, KeyWord.CONST, KeyWord.TYPE, KeyWord.PROCEDURE))
            {
                switch (current_lexeme.LexemeValue)
                {
                    case KeyWord.VAR:
                        types.Add(ParseVarDefs());
                        break;
                    case KeyWord.TYPE:
                        types.Add(ParseTypeDefs());
                        break;
                    case KeyWord.CONST:
                        types.Add(ParseConstDefs());
                        break;
                    case KeyWord.PROCEDURE:
                        types.Add(ParseProcedureDefs());
                        break;
                }
            }
            return types;
        }
        public NodeDefs ParseVarDefs()
        {
            List<VarDeclarationNode> body = new List<VarDeclarationNode>();
            Require(KeyWord.VAR);
            Require(LexemeType.IDENTIFIER);
            while (Expect(LexemeType.IDENTIFIER))
            {
                body.Add(ParseVarDef());
                Require(Separator.SEMICOLON);
            }
            return new VarTypesNode(body);
        }
        public VarDeclarationNode ParseVarDef(KeyWord? param = null)
        {
            List<string> names_def = new List<string>();
            List<SymVar> vars = new List<SymVar>();
            SymType type;
            NodeExpression? value = null;

            Require(LexemeType.IDENTIFIER);
            names_def.Add(current_lexeme.LexemeValue.ToString());
            symTableStack.Check(names_def[^1]);
            GetNextLexeme();
            while (Expect(Separator.COMMA))
            {
                GetNextLexeme();
                Require(LexemeType.IDENTIFIER);
                names_def.Add(current_lexeme.LexemeValue.ToString());
                symTableStack.Check(names_def[^1]);
                GetNextLexeme();
            }
            Require(Operation.COLON);
            if (!Expect(LexemeType.IDENTIFIER) && !Expect(LexemeType.WORDKEY))
            {
                throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) expected type variable");
            }
            type = ParseType();
            if (Expect(Operation.EQUAL))
            {
                if (names_def.Count > 1)
                {
                    throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) Fatal : only one variable can be initialized");
                }
                GetNextLexeme();
                value = ParseExpression();
            }

            foreach (string name in names_def)
            {
                SymVar var = new SymVar(name, type);
                switch (param)
                {
                    case KeyWord.VAR:
                        symTableStack.Add(name, new SymParamVar(var));
                        var = new SymParamVar(var);
                        break;
                    case KeyWord.OUT:
                        symTableStack.Add(name, new SymParamOut(var));
                        var = new SymParamOut(var);
                        break;
                    default:
                        symTableStack.Add(name, var);
                        break;
                }
                vars.Add(var);
            }
            return new VarDeclarationNode(vars, type, value);
        }
        public NodeDefs ParseConstDefs()
        {
            string name;

            List<ConstDeclarationNode> body = new List<ConstDeclarationNode>();
            GetNextLexeme();

            Require(LexemeType.IDENTIFIER);
            while (Expect(LexemeType.IDENTIFIER))
            {
                name = (string)current_lexeme.LexemeValue;
                symTableStack.Check(name);
                GetNextLexeme();
                Require(Operation.EQUAL);
                NodeExpression value;
                value = ParseExpression();
                Require(Separator.SEMICOLON);
                SymVarConst varConst = new SymVarConst(name, value.GetCachedType(), value);
                symTableStack.Add(name, varConst);
                body.Add(new ConstDeclarationNode(varConst, value));
            }
            return new ConstTypesNode(body);
        }
        public NodeDefs ParseTypeDefs()
        {
            List<DeclarationNode> body = new List<DeclarationNode>();
            GetNextLexeme();
            while (current_lexeme.LexemeType == LexemeType.IDENTIFIER)
            {
                body.Add(ParseTypeDef());
                Require(Separator.SEMICOLON);
            }
            return new TypeTypesNode(body);
        }
        public TypeDeclarationNode ParseTypeDef()
        {
            string nameType;
            SymType type;

            Require(LexemeType.IDENTIFIER);
            nameType = current_lexeme.LexemeValue.ToString();
            symTableStack.Check(nameType);
            GetNextLexeme();
            Require(Operation.EQUAL);
            type = ParseType();
            SymTypeAlias typeAlias = new SymTypeAlias(type.GetName(), type);
            symTableStack.Add(nameType, type);
            return new TypeDeclarationNode(nameType, typeAlias);
        }
        public NodeDefs ParseProcedureDefs()
        {
            string name;
            List<VarDeclarationNode> paramsNode = new List<VarDeclarationNode>();
            SymTable locals = new SymTable(new Dictionary<string, Symbol>());
            GetNextLexeme();
            Require(LexemeType.IDENTIFIER);
            name = current_lexeme.LexemeValue.ToString();
            symTableStack.Check(name);
            GetNextLexeme();
            symTableStack.AddTable(locals);
            if (Expect(Separator.OPEN_PARENTHESIS))
            {
                paramsNode = ParseProcedureParameters();
                Require(Separator.CLOSE_PARENTHESIS);
            }
            Require(Separator.SEMICOLON);

            SymTable params_ = new SymTable(symTableStack.GetBackTable());

            List<NodeDefs> localsTypes = new List<NodeDefs>();
            while (Expect(KeyWord.VAR, KeyWord.CONST, KeyWord.TYPE))
            {
                switch (current_lexeme.LexemeValue)
                {
                    case KeyWord.VAR:
                        localsTypes.Add(ParseVarDefs());
                        break;
                    case KeyWord.CONST:
                        localsTypes.Add(ParseConstDefs());
                        break;
                    case KeyWord.TYPE:
                        localsTypes.Add(ParseTypeDefs());
                        break;
                }
            }

            locals = symTableStack.GetBackTable();

            Require(KeyWord.BEGIN);
            BlockStmt body = ParseBlock();
            Require(Separator.SEMICOLON);

            symTableStack.PopBack();
            SymProc symProc = new SymProc(name, params_, locals, body);
            symTableStack.Add(name, symProc);
            return new ProcedureTypesNode(paramsNode, localsTypes, symProc);
        }
        public List<VarDeclarationNode> ParseProcedureParameters()
        {
            List<VarDeclarationNode> paramsNode = new List<VarDeclarationNode>();
            do
            {
                GetNextLexeme();
                VarDeclarationNode varDef;
                if (Expect(KeyWord.VAR, KeyWord.OUT))
                {
                    KeyWord param = (KeyWord)current_lexeme.LexemeValue;
                    GetNextLexeme();
                    varDef = ParseVarDef(param);
                    paramsNode.Add(varDef);
                }
                else
                {
                    varDef = ParseVarDef();
                    paramsNode.Add(varDef);
                }
            }
            while (Expect(Separator.SEMICOLON));
            return paramsNode;
        }
    }
}
