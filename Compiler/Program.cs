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
        {
           if (args.Length == 0)
                return;
            
           var lexer = new Lexer.Lexer(args[0]);
          
          // string filePath = "../../../../Tester/Tests/LexerTests/Files/048_real.in";
          // var lexer = new Lexer.Lexer(filePath);
          
            var endfile = false;
            while (!endfile)
            {
                LexemeData currectlexeme = lexer.GetLexeme();

                if(currectlexeme.LexemeType == LexemeType.ENDFILE || currectlexeme.LexemeType == LexemeType.ERROR) endfile = true;
            }

            /*
            try
            {
                var lexer = new Lexer.Lexer(filePath);
                if (args.Contains("-lexer"))
                {
                    var token =lexer.GetLexeme();
                    while (token.TokenType != SymbolType.EndFile)
                    {
                        Console.WriteLine(token);
                        token = tokenizer.Get();
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"error: file {e.FileName} not found");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }*/
        }
    }
}