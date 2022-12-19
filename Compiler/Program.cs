using Compiler.Lexer;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Compiler
{

    public class Program
    {
        static void Main(string[] args)
        {   /*
            string filePath = "../../../../Tester/Tests/LexerTests/Files/063_numsystem2.in";

            var lexer = new Lexer.Lexer(filePath);

            bool endfile = false;
            while (!endfile)
            {
                LexemeData currectlexeme = lexer.GetLexeme();

                if (currectlexeme.LexemeType == LexemeType.ENDFILE)
                {
                    Console.Write("EndFile");
                    endfile = true;
                    break;
                }
                if (currectlexeme.LexemeType == LexemeType.ERROR)
                {
                    Console.Write(currectlexeme.LexemeValue);
                    endfile = true;
                    break;
                }

                Console.WriteLine($"{currectlexeme.NumbLine}\t{currectlexeme.NumbSymbol}\t{currectlexeme.LexemeType}\t{currectlexeme.LexemeValue}\t{currectlexeme.LexemeSource}");
            }/**/
            
            //var parser = new Parser.Parser(filePath);
            //var treeATC = parser.GetParser();
            //Parser.PrintAST.Print(treeATC);
            
            if (args.Length == 0)
                return;

            if (args.Contains("-lexer"))
            {
                var lexer = new Lexer.Lexer(args[0]);

                bool endfile = false;
                while (!endfile)
                {
                    LexemeData currectlexeme = lexer.GetLexeme();

                    if (currectlexeme.LexemeType == LexemeType.ENDFILE)
                    {
                        Console.Write("EndFile");
                        endfile = true;
                        break;
                    }
                    if (currectlexeme.LexemeType == LexemeType.ERROR)
                    {
                        Console.Write(currectlexeme.LexemeValue);
                        endfile = true;
                        break;
                    }

                    Console.WriteLine($"{currectlexeme.NumbLine}\t{currectlexeme.NumbSymbol}\t{currectlexeme.LexemeType}\t{currectlexeme.LexemeValue}\t{currectlexeme.LexemeSource}");
                }
            }
            if (args.Contains("-parser"))
            {
                var parser = new Parser.Parser(args[0]);

                var treeATC = parser.GetParser();

                Parser.PrintAST.Print(treeATC);
            }/**/
        }
    }
}