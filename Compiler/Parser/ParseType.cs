namespace Compiler
{
    public partial class Parser
    {
        //  type ::= primitive_type | array_type | record_type | original_type
        public SymType ParseType()
        {
            if (!(Expect(LexemeType.IDENTIFIER)) && !(Expect(LexemeType.WORDKEY)))
            {
                throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) expected type variable");
            }
            var type = current_lexeme.LexemeValue;
            if (Expect(LexemeType.IDENTIFIER))
            {
                type = current_lexeme.LexemeValue.ToString().ToLower();
            } 
            GetNextLexeme();
            switch (type)
            {
                case "integer":
                    return ParsePrimitiveType("integer");
                case "real":
                    return ParsePrimitiveType("real");
                case KeyWord.STRING:
                    return ParsePrimitiveType("string");
                case KeyWord.ARRAY:
                    return ParseArrayType();
                case KeyWord.RECORD:
                    return ParseRecordType();
                default:
                    SymType original_type;
                    try
                    {
                        Symbol sym = symTableStack.Get((string)type);
                        original_type = (SymType)sym;
                    }
                    catch
                    {
                        throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) Identifier not found \"{type}\"");
                    }
                    return new SymTypeAlias((string)type, original_type);
            }
        }

        // primitive_type::= "integer" | "real" | "string" 
        public SymType ParsePrimitiveType(string a)
        {
            return (SymType)symTableStack.Get(a);
        }

        //  array_type ::= "array" "[" (ordinal_type) { "," (ordinal_type) } "]" "of" primitive_type
        public SymType ParseArrayType()
        {
            SymType type;
            List<OrdinalTypeNode> ordinal_types = new List<OrdinalTypeNode>();

            Require(Separator.OPEN_BRACKET);
            do
            {
                ordinal_types.Add(ParseOrdinalType());
            }
            while (Expect(Separator.COMMA));
            Require(Separator.CLOSE_BRACKET);
            Require(KeyWord.OF);
            if (Expect(LexemeType.IDENTIFIER) || Expect(LexemeType.WORDKEY))
            {
                if (Expect(LexemeType.IDENTIFIER))
                {
                    type = ParsePrimitiveType(current_lexeme.LexemeValue.ToString().ToLower());
                }
                else
                {
                    type = ParsePrimitiveType("string");
                }
                if (type == null)
                {
                    throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) expected type");
                }
            }
            else
            {
                throw new ExceptionCompiler($"({current_lexeme.NumbLine}, {current_lexeme.NumbSymbol}) expected type");
            }
            GetNextLexeme();
            return new SymArray("array", ordinal_types, type);
        }

        //  ordinal_type ::= simple_expression ".." simple_expression
        public OrdinalTypeNode ParseOrdinalType()
        {
            NodeExpression from = ParseSimpleExpression();
            Require(Separator.DOUBLE_POINT);
            NodeExpression to = ParseSimpleExpression();
            return new OrdinalTypeNode(from, to);
        }

        //  record_type ::=  "record" {one_var_declaration} "end"
        public SymType ParseRecordType()
        {
            SymTable fields = new SymTable(new Dictionary<string, Symbol>());
            symTableStack.AddTable(fields);
            while (Expect(LexemeType.IDENTIFIER))
            {
                ParseVarDef();
                if (Expect(KeyWord.END))
                {
                    break;
                }
                Require(Separator.SEMICOLON);
            }
            Require(KeyWord.END);
            symTableStack.PopBack();
            return new SymRecord("record", fields);
        }
    }
}
