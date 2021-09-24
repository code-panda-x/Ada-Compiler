using System;
using System.Collections.Generic;
using System.Linq;
using static AdaCompiler.LexicalAnalyzer;

namespace AdaCompiler
{
    public class SymbolTable
    {
        // Variable declarations
        public const int TableSize = 211;

        public enum VarType { TINTEGER, TSTR, TFLOAT, TCHAR, TCONSTANT ,TUNKNOWN};


        private List<TableAttributes>[] table;

        private struct variableType
        {
            public VarType TypeOfVariable;
            public int Offset;
            public int Size; //Integers have size 2, characters have size 1 and float has size 4
        }
        private struct constantType
        {
            public VarType TypeOfConstant;
            public int Offset;
            public int Size;
            public int Value;
            public double ValueR;
        }
        private struct procedureType
        {
            public int SizeofLocal;
            public int NumParameters;
            public string Mode;
            public string ParameterTypes;
        }
        public class TableAttributes
        {
            public Tokens Token;
            public dynamic TypeEntry;
            public string Lexeme;
            public int depth;
        }

        // Constructor that initializes the symbol talbe with size 211
        public SymbolTable()
        {
            table = new List<TableAttributes>[TableSize];

            for (int i = 0; i < table.Length; i++)
                table[i] = new List<TableAttributes>();
        }

        private VarType TokenToVar(Tokens token)
        {
            if (token == Tokens.TINTEGER)
                return VarType.TINTEGER;
            if (token == Tokens.TSTR)
                return VarType.TSTR;
            if (token == Tokens.TFLOAT)
                return VarType.TFLOAT;
            if (token == Tokens.TCHAR)
                return VarType.TCHAR;
            if (token == Tokens.TCONSTANT)
                return VarType.TCONSTANT;
            else
                return VarType.TUNKNOWN;
        }

        private List<VarType> TokenToVar(List<Tokens> tokens)
        {
            var newList = new List<VarType>();

            foreach (var token in tokens)
            {
                newList.Add(TokenToVar(token));
            }

            return newList;
        }


        // hash function passed a lexeme and return the location for that lexeme
        private int hashpjw(string s)
        {
            uint h = 0, g;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                h = (h << 24) + c;
                g = h & 0xF0000000;
                if (g != 0)
                {
                    h = h ^ (g >> 24);
                    h = h ^ g;
                }
            }
            return (int)h % TableSize;
        }

        public void SetProcedure(TableAttributes entry, int sizeOfLocal, int numberOfParameters, string mode, string paraTypes)
        {
            var abc = "";
            for (int i = 0; i < table.Length; i++)
                for (int j = 0; j < table[i].Count; j++)
                    if (table[i][j] == entry)
                    {
                        table[i][j].TypeEntry = new procedureType();
                        table[i][j].TypeEntry.SizeofLocal = sizeOfLocal;
                        table[i][j].TypeEntry.NumParameters = numberOfParameters;
                        table[i][j].TypeEntry.Mode = mode;
                        abc = table[i][j].TypeEntry.Mode;
                        table[i][j].TypeEntry.ParameterTypes = paraTypes;
                    }

        }

        public void SetVariable(TableAttributes entry, Tokens varType, int offset, int size)
        {
            var abc = 0;
            for (int i = 0; i < table.Length; i++)
                for (int j = 0; j < table[i].Count; j++)
                    if (table[i][j] == entry)
                    {
                        table[i][j].TypeEntry = new variableType();
                        table[i][j].TypeEntry.TypeOfVariable = TokenToVar(varType);
                        table[i][j].TypeEntry.Offset = offset;
                        table[i][j].TypeEntry.Size = size;
                    }
            
        }

        public void SetConstant(TableAttributes entry, string value)
        {
            for (int i = 0; i < table.Length; i++)
                for (int j = 0; j < table[i].Count; j++)
                    if (table[i][j] == entry)
                    {
                        table[i][j].TypeEntry = new constantType();

                        if (!value.Contains('.'))
                            table[i][j].TypeEntry.Value = Convert.ToInt32(value);
                        else
                            table[i][j].TypeEntry.ValueR = Convert.ToDouble(value);
                    }
        }


        // lookup uses the lexeme to find the entry and returns a pointer to that entry
        public TableAttributes Lookup(string Lex)
        {
            /*
            var index = hashpjw(Lex);
            var TableEntry = table[index];
            while (TableEntry.Count != 0)
            {
                for (int i = 0; i < table[index].Count; i++)
                {
                    if (table[index][i].lexeme == Lex)
                    {
                        return (TableAttributes)Convert.ChangeType(table[index][i], typeof(TableAttributes));
                    }
                }
            }
            return null;
            */
            foreach (var t in table)
                foreach (var c in t)
                    if (c.Lexeme == Lex)
                        return c;

            return null;
        }

        // insert the lexeme, token and depth into a record in the symbol table
        public void Insert(string lex, Tokens token, int depth)
        {
            int x = hashpjw(lex);
            var newEntry = new TableAttributes
            {
                Token = token,
                Lexeme = lex,
                depth = depth
            };

            table[x] = table[x].Prepend(newEntry).ToList();
        }

        // include a procedure that will write out all variables (lexeme only) that are in the table at a specified depth
        public void WriteTable(int depth)
        {
            /*
            foreach (var a in table)
                foreach (var b in a)
                {
                    //if (b.depth == depth)
                    //  Console.WriteLine("{0} {1}", b.Lexeme, b.TypeEntry.ToString().Split('+')[1]);
                }
            */

            if (table != null)
            {
                for (int i = 0; i < table.Length; i++)
                    for (int j = 0; j < table[i].Count; j++)
                        if (table[i][j].depth.Equals(depth))
                        {
                            string typeentry = Convert.ToString(table[i][j].TypeEntry);
                            string[] temps = typeentry.Split('+');
                            string entry = temps[1];
                            Console.WriteLine("Lexeme: " + table[i][j].Lexeme + " Depth: " + table[i][j].depth + " Type: " + entry);
                            
                        }
            }
            
        }


        // delete is passed the depth and deletes all records that are in the table at that depth
        public void DeleteDepth(int depth)
        {
            for (int i = 0; i < table.Length; i++)
                for (int j = 0; j < table[i].Count; j++)
                    if (table[i][j].depth == depth)
                        table[i].Remove(table[i][j]);
        }
    }
}