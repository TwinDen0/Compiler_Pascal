using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public partial class Parser
    {
        public void PrintParse(NodeMainProgram main_prog)
        {
            string print = main_prog.ToString();
            Console.WriteLine(print);
        }
    }
}
