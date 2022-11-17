using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection.PortableExecutable;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics.Metrics;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Compiler
{
    class Tester
    {
        public static void StartTest()
        {
            Console.WriteLine("Начать тестирование:\n" +
                              "1 - Лексический анализатор\n" +
                              "2 - Анализ простейшего выражения\n");
            string? key = Console.ReadLine();
            Console.WriteLine();

            if (key == "1") //lexer
            {
                TesterLexer();
            }
            if (key == "2") //Синтактический
            {
                TesterParser(); /////
            }


        }
        static void TesterLexer()
        {
            Console.WriteLine("Для запуска общего тестирования нажмите 1\n" +
                              "Для запуска тестов с подробным результатом нажмите 2\n");
            string? key = Console.ReadLine();
            Console.WriteLine();

            Lexer l = new Lexer();
            string path_in;
            string path_ans;
            string path_out;

            int countErrors = 0;
            int countTests = 117;

            for (int i = 1; i <= countTests; i++)
            {
                path_in = $@"..\..\..\tests\tests_lexer\in_test\{i}in.txt";
                path_ans = $@"..\..\..\tests\tests_lexer\ans_test\{i}ans.txt";
                path_out = $@"..\..\..\tests\tests_lexer\out_test\{i}out.txt";

                l.LexerReadFile(path_in);

                List<string> res = new List<string>();
                string line_res = "";

                if (Lexer.lexems.Count > 0)
                {
                    foreach (LexemeData r in Lexer.lexems)
                    {
                        if (r.class_lexeme != State.Error)
                        {
                            line_res = $"{r.numb_line} {r.numb_symbol} {r.class_lexeme} {r.value_lexeme} {r.inital_value}";
                        }
                        else
                        {
                            line_res = $"({r.numb_line}, {r.numb_symbol}) {r.value_lexeme} ";

                        }
                        res.Add(line_res);
                    }
                }

                using (StreamWriter writer = new StreamWriter(path_out))
                {
                    foreach (var line_out in res)
                    {
                        writer.WriteLine(line_out);
                    }
                }

                int j = 0;
                bool correctAns = false;

                using (StreamReader sr = new StreamReader(path_ans, Encoding.Default))
                {

                    string line_ans;

                    while ((line_ans = sr.ReadLine()) != null && j < res.Count)
                    {
                        if (line_ans == res[j])
                        {
                            correctAns = true;
                        }
                        else
                        {
                            correctAns = false;

                        }
                        j += 1;
                    }
                    if (line_ans == null && line_res == "" && j == 0)
                    {
                        correctAns = true;
                    }
                }

                if (key == "1")
                {
                    if (correctAns == true)
                    {
                        Console.WriteLine($"{i} OK");
                    }
                    else
                    {
                        countErrors += 1;
                        Console.WriteLine($"{i} WA");
                    }
                }

                if (key == "2")
                {
                    string? textFromFile;

                    using (FileStream fstream = File.OpenRead(path_in))
                    {
                        byte[] buffer = new byte[fstream.Length];
                        fstream.Read(buffer, 0, buffer.Length);
                        textFromFile = Encoding.Default.GetString(buffer);
                    }
                    Console.WriteLine("_________________________\n" +
                                      $"Тест {i}\n" +
                                      textFromFile +
                                      "\n");

                    foreach (string ans in res)
                    {
                        if (res.Count >= 1)
                        {
                            Console.WriteLine($"Ответ:\n" +
                                              ans);
                        }
                        else
                        {
                            Console.WriteLine($"Ответ: \n");
                        }
                    }

                    var key2 = Console.ReadKey();
                    if (key2.Key == ConsoleKey.Escape)
                    {
                        return;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            if (key == "1")
            {
                Console.WriteLine($"Общее количество тестов: {countTests}\n" +
                                  $"Общее количество ошибок: {countErrors}\n");
            }
        }
        static void TesterParser()
        {
            Console.WriteLine("Для запуска общего тестирования нажмите 1\n" +
                              "Для запуска тестов с подробным результатом нажмите 2\n");
            string? key = Console.ReadLine();
            Console.WriteLine();

            Parser p = new Parser();
            string path_in;
            string path_ans;
            string path_out;

            int countErrors = 0;
            int countTests = 1;

            for (int i = 1; i <= countTests; i++)
            {
                path_in = $@"..\..\..\tests\tests_parser\in_test\{i}in.txt";
                path_ans = $@"..\..\..\tests\tests_parser\ans_test\{i}ans.txt";
                path_out = $@"..\..\..\tests\tests_parser\out_test\{i}out.txt";

                p.ParserReadFile(path_in);

                List<List<string>> res = new List<List<string>>();
                List<string> final_res = new List<string>();

                List<string> temp = new List<string>();
                res.Add(temp);
                res[res.Count - 1].Add("");

                string line_res = "";

                if (p.resParser != null)
                {
                    CheckChildren(p.resParser, ref res);

                    for (int m = 0; m <= res[0].Count - 1; m++)
                    {
                        for (int n = 0; n <= res.Count - 1; n++)
                        {
                            line_res += res[n][m];
                        }
                        final_res.Add(line_res);
                        line_res = "";
                    }
                }

                using (StreamWriter writer = new StreamWriter(path_out))
                {
                    foreach (var line_out in final_res)
                    {
                        writer.WriteLine(line_out);
                    }
                }

                int j = 0;
                bool correctAns = false;

                using (StreamReader sr = new StreamReader(path_ans, Encoding.Default))
                {
                    string line_ans;
                    while ((line_ans = sr.ReadLine()) != null && j < final_res.Count)
                    {
                        if (line_ans == final_res[j])
                        {
                            correctAns = true;
                        }
                        else
                        {
                            correctAns = false;
                        }
                        j += 1;
                    }
                    if (line_ans == null && line_res == "")
                    {
                        correctAns = true;
                    }
                }

                if (key == "1")
                {
                    if (correctAns == true)
                    {
                        Console.WriteLine($"{i} OK");
                    }
                    else
                    {
                        countErrors += 1;
                        Console.WriteLine($"{i} WA");
                    }
                }

                if (key == "2")
                {
                    string? textFromFile;

                    using (FileStream fstream = File.OpenRead(path_in))
                    {
                        byte[] buffer = new byte[fstream.Length];
                        fstream.Read(buffer, 0, buffer.Length);
                        textFromFile = Encoding.Default.GetString(buffer);
                    }
                    Console.WriteLine("_________________________\n" +
                                      $"Тест {i}\n" +
                                      textFromFile +
                                      "\n");

                    Console.WriteLine($"Ответ:\n");
                    foreach (string ans in final_res)
                    {
                        if (final_res.Count >= 1)
                        {
                            Console.WriteLine(ans);
                        }
                    }

                    var key2 = Console.ReadKey();
                    if (key2.Key == ConsoleKey.Escape)
                    {
                        return;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            if (key == "1")
            {
                Console.WriteLine($"Общее количество тестов: {countTests}\n" +
                                  $"Общее количество ошибок: {countErrors}\n");
            }
        }

        static public int x = 0;
        static public int y = 0;
        static public List<int> OpenLeftСhild = new List<int>(); // хранит все стоблики, если в столбике 1, то не зкартыа, если 0, то закрыта
        static void CheckChildren(Node resParse, ref List<List<string>> res)
        {
            if (x == 0) OpenLeftСhild.Add(0);

            while (res.Count < x + 1)
            {
                List<string> temp = new List<string>();
                res.Add(temp);
                res[res.Count - 1].Add("");
            }
            for (int i = 0; i < x+1; i++)
            {
                while (res[i].Count < y + 2)
                {
                    res[i].Add("");
                }
            }

            res[x][y] = resParse.value;

            y += 1;

            for (int i = 0; i < res.Count; i++)
            {
                if (res[i].Count < y + 1)
                {
                    res[i].Add("");
                }
            }

            if (resParse.children != null)
            {
                for (int j = 0; j < res.Count; j++)
                {
                    if (OpenLeftСhild[j] > 0)
                    {
                        res[j][y] = "│    ";
                    }
                    else
                    {
                        res[j][y] = "     ";
                    }
                }

                if (resParse.children.Count == 2)
                {
                    res[x][y] = "├─── ";
                    OpenLeftСhild[x] = 1;
                }

                if (resParse.children.Count == 1)
                {
                    res[x][y] = "└─── ";
                    OpenLeftСhild.RemoveAt(OpenLeftСhild.Count - 1);
                }
                
                if (resParse.children[0] != null)
                {
                    x += 1;
                    OpenLeftСhild.Add(0);

                    if (res.Count < x + 1)
                    {
                        List<string> temp = new List<string>();
                        res.Add(temp);
                        res[res.Count - 1].Add("");
                    }
                    for (int i = 0; i < res.Count; i++)
                    {
                        if (res[i].Count < y + 1)
                        {
                            res[i].Add("");
                        }
                    }

                    CheckChildren(resParse.children[0], ref res);
                } 
                
                if (resParse.children.Count > 1)
                {
                    if (resParse.children[1] != null)
                    {
                        for (int j = 0; j < res.Count; j++)
                        {
                            if (OpenLeftСhild[j] > 0)
                            {
                                res[j][y] = "│    ";
                            }
                            else
                            {
                                res[j][y] = "     ";
                            }
                        }

                        res[x][y] = "└─── ";
                        OpenLeftСhild[x] = 0;

                        x += 1;
                        OpenLeftСhild.Add(0);

                        CheckChildren(resParse.children[1], ref res);

                        x -= 1;
                    }
                }
            }
            else
            {
                x -= 1;
                return;
            }
        }
    }
}