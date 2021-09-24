// Compiler Assignment7
// Kehan Meng
// Date: 4-19-2021

using System;

namespace AdaCompiler
{
    class Program
    {
        static void Main(string[] args)
        {

            // Initializations
            var entry = new LexicalAnalyzer(args);
            var symtable = new SymbolTable();
            var x = new Parser(entry,symtable);
            x.Parse();
            x.MakeTACfile();
            x.GenerateASMfile();

            Console.WriteLine("Complete!");
            Console.ReadKey();
        }

       
    }

}