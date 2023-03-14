using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace Compiler
{
    public class Program
    {
        static void Main(string[] args)
        {
            
            string filePath = "../../../../Tester/Tests/ParserTests/Files/017_defs.in";

            var lexer = new Lexer(filePath);
            var parser = new Parser(lexer);

            var node_main = parser.ParseProgram();

            parser.PrintParse(node_main);
            

            /*
            bool endfile = false;
            while (!endfile)
            {
                Lexeme currectlexeme = lexer.NextLexeme();

                if (currectlexeme.LexemeType == LexemeType.ENDFILE)
                {
                    endfile = true;
                    break;
                }
                if (currectlexeme.LexemeType == LexemeType.SEPARATOR || currectlexeme.LexemeType == LexemeType.OPERATOR)
                {
                    //string k = convert_sign.Where(x => x.Value == (object)Separator.CLOSE_BRACKET).FirstOrDefault().Key;
                    Console.WriteLine($"{currectlexeme.NumbLine}\t{currectlexeme.NumbSymbol}\t{currectlexeme.LexemeType}\t{currectlexeme.LexemeSource}\t{currectlexeme.LexemeSource}");
                }
                else
                {
                    Console.WriteLine($"{currectlexeme.NumbLine}\t{currectlexeme.NumbSymbol}\t{currectlexeme.LexemeType}\t{currectlexeme.LexemeValue}\t{currectlexeme.LexemeSource}");
                }
            }
            /**/

            //Parser parser = new Parser(lexer);
            //Node firstNode = parser.ParseProgram();
            //Console.WriteLine(firstNode.ToString());
            //int i = 1;
            /**/

            //var parser = new Parser.Parser(filePath);
            //var treeATC = parser.GetParser();
            //Parser.PrintAST.Print(treeATC);
            /*
            if (args.Length == 0)
                return;
            try
            {
                if (args.Contains("-lexer"))
                {
                    var lexer = new Lexer(args[0]);

                    bool endfile = false;
                    while (!endfile)
                    {
                        Lexeme currectlexeme = lexer.NextLexeme();

                        if (currectlexeme.LexemeType == LexemeType.ENDFILE)
                        {
                            endfile = true;
                            break;
                        }
                        if (currectlexeme.LexemeType == LexemeType.SEPARATOR || currectlexeme.LexemeType == LexemeType.OPERATOR)
                        {
                            //string k = convert_sign.Where(x => x.Value == (object)Separator.CLOSE_BRACKET).FirstOrDefault().Key;
                            Console.WriteLine($"{currectlexeme.NumbLine}\t{currectlexeme.NumbSymbol}\t{currectlexeme.LexemeType}\t{currectlexeme.LexemeSource}\t{currectlexeme.LexemeSource}");
                        }
                        else
                        {
                            Console.WriteLine($"{currectlexeme.NumbLine}\t{currectlexeme.NumbSymbol}\t{currectlexeme.LexemeType}\t{currectlexeme.LexemeValue}\t{currectlexeme.LexemeSource}");
                        }
                    }
                }
            if (args.Contains("-parser"))
                {
                    var lexer = new Lexer(args[0]);
                    var parser = new Parser(lexer);

                    var node_main = parser.ParseProgram();

                    parser.PrintParse(node_main);
                }
            }
            catch (ExceptionCompiler e)
            {
                Console.Write($"{e}\r\n");
            }
            */
        }
    }
}