namespace Compiler
{
    public partial class Parser
    {
        //  expression ::= simple_expression ("<" | "<=" | ">" | ">=" | "=" | "<>") simple_expression
        public NodeExpression ParseExpression()
        {
            NodeExpression left = ParseSimpleExpression();
            while (Expect(Operation.LESS, Operation.LESS_OR_EQUAL, Operation.GREATER, Operation.GREATER_OR_EQUAL, Operation.EQUAL, Operation.NOT_EQUAL))
            {
                Operation operation = (Operation)current_lexeme.LexemeValue;
                GetNextLexeme();
                NodeExpression right = ParseSimpleExpression();
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }

        //  simple_expression ::= term { ("or" | "+" | "-") term }
        public NodeExpression ParseSimpleExpression()
        {
            NodeExpression left = ParseTerm();
            while (Expect(Operation.PLUS, Operation.MINUS, KeyWord.OR, KeyWord.XOR))
            {
                object operation = current_lexeme.LexemeValue;
                GetNextLexeme();
                NodeExpression right = ParseTerm();
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }

        //  term ::= factor { ( "and" | "/" | "*" ) factor } 
        public NodeExpression ParseTerm()
        {
            NodeExpression left = ParseFactor();
            while (Expect(Operation.MULTIPLY, Operation.DIVIDE, KeyWord.AND))
            {
                object operation = current_lexeme.LexemeValue;
                GetNextLexeme();
                NodeExpression right = ParseFactor();
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }

        //  factor ::= ( string | ["+" | "-"] ( int | real) | call |  variable | "(" expression ")"| id "." id)
        public NodeExpression ParseFactor()
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
                exp = ParseExpression();
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
                    throw new ExceptionCompiler($"{current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) Identifier not found \"{factor.LexemeValue}\"");
                }
                ans = new NodeVar(symVar);
                while (Expect(Separator.OPEN_BRACKET, Separator.POINT))
                {
                    Separator separator = (Separator)current_lexeme.LexemeValue;
                    GetNextLexeme();
                    switch (separator)
                    {
                        case Separator.OPEN_BRACKET:
                            ans = ParseArrayElement(ref symVar);
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
            throw new ExceptionCompiler($"{current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) expected factor");
        }
    }
}