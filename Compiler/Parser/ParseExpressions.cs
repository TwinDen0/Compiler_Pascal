namespace Compiler
{
    public partial class Parser
    {
        public NodeExpression ParseExpression(bool inDef = false)
        {
            NodeExpression left = ParseSimpleExpression(inDef);
            while (Expect(Operation.LESS, Operation.LESS_OR_EQUAL, Operation.GREATER, Operation.GREATER_OR_EQUAL, Operation.EQUAL, Operation.NOT_EQUAL))
            {
                Operation operation = (Operation)current_lexeme.LexemeValue;
                GetNextLexeme();
                NodeExpression right = ParseSimpleExpression(inDef);
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public NodeExpression ParseSimpleExpression(bool inDef = false)
        {
            NodeExpression left = ParseTerm(inDef);
            while (Expect(Operation.PLUS, Operation.MINUS, KeyWord.OR, KeyWord.XOR))
            {
                object operation = current_lexeme.LexemeValue;
                GetNextLexeme();
                NodeExpression right = ParseTerm(inDef);
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public NodeExpression ParseTerm(bool inDef = false)
        {
            NodeExpression left = ParseFactor(inDef);
            while (Expect(Operation.MULTIPLY, Operation.DIVIDE, KeyWord.AND))
            {
                object operation = current_lexeme.LexemeValue;
                GetNextLexeme();
                NodeExpression right = ParseFactor(inDef);
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public NodeExpression ParseFactor(bool inDef = false)
        {
            if (Expect(LexemeType.STRING))
            {
                Lexeme factor = current_lexeme;
                GetNextLexeme();
                return new NodeString((string)factor.LexemeValue);
            }
            if (Expect(LexemeType.REAL))
            {
                Lexeme factor = current_lexeme;
                GetNextLexeme();
                return new NodeReal((double)factor.LexemeValue);
            }
            if (Expect(LexemeType.INTEGER))
            {
                Lexeme factor = current_lexeme;
                GetNextLexeme();
                return new NodeInt((int)(long)factor.LexemeValue);
            }
            if (Expect(Separator.OPEN_PARENTHESIS))
            {
                NodeExpression exp;
                GetNextLexeme();
                exp = ParseExpression(inDef);
                Require(Separator.CLOSE_PARENTHESIS);
                return exp;
            }
            if (Expect(LexemeType.IDENTIFIER))
            {
                NodeExpression ans;
                Lexeme factor = current_lexeme;
                GetNextLexeme();
                SymVar symVar;
                try
                {
                    symVar = (SymVar)symTableStack.Get((string)factor.LexemeValue);
                }
                catch
                {
                    throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) Fatal: Identifier not found {factor.LexemeValue}");
                }
                if (inDef && symVar.GetType() != typeof(SymVarConst))
                {
                    throw new ExceptionCompiler($"{current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) Illegal expression");
                }
                ans = new NodeVar(symVar);
                while (Expect(Separator.OPEN_BRACKET, Separator.POINT))
                {
                    Separator separator = (Separator)current_lexeme.LexemeValue;
                    GetNextLexeme();
                    switch (separator)
                    {
                        case Separator.OPEN_BRACKET:
                            ans = ParseArrayElement(ans, ref symVar);
                            break;
                        case Separator.POINT:
                            ans = ParseRecordElement(ans, ref symVar);
                            break;
                    }
                }
                return ans;
            }
            if (Expect(Operation.PLUS, Operation.MINUS, KeyWord.NOT))
            {
                object unOp = current_lexeme.LexemeValue;
                GetNextLexeme();
                NodeExpression factor = ParseFactor();
                return new NodeUnOp(unOp, factor);
            }
            throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) expected factor");
        }
    }
}