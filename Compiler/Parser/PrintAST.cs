using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Parser
{
    internal class PrintAST
    {
        public static void Print(Node syntactic_tree)
        {
            List<List<string>> res = new List<List<string>>();
            List<string> final_res = new List<string>();

            List<string> temp = new List<string>();
            res.Add(temp);
            res[res.Count - 1].Add("");

            string line_res = null;

            if (syntactic_tree != null)
            {
                x = 0;
                y = 0;

                CreateMatrix(syntactic_tree, ref res);

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

            foreach (var line_out in final_res)
            {
                Console.WriteLine(line_out);
            }
        }

            static public int x = 0;
        static public int y = 0;
        static public List<int> OpenLeftСhild = new List<int>();
        public static void CreateMatrix(Node resParse, ref List<List<string>> res)
        {
            if (x == 0) OpenLeftСhild.Add(0);

            while (res.Count < x + 1)
            {
                List<string> temp = new List<string>();
                res.Add(temp);
                res[res.Count - 1].Add("");
            }
            for (int i = 0; i < x + 1; i++)
            {
                while (res[i].Count < y + 2)
                {
                    res[i].Add("");
                }
            }

            res[x][y] = resParse.value.ToString();

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

                    CreateMatrix(resParse.children[0], ref res);
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

                        CreateMatrix(resParse.children[1], ref res);

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
