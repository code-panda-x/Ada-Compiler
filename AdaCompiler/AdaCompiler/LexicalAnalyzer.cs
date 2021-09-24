// LexicalAnalyzer
// Description: Scan file input and get,process tokens

using System;
using System.IO;
using System.Globalization;

namespace AdaCompiler
{
    public class LexicalAnalyzer
    {
        // Global variables
        public Tokens Token { get; set; } = Tokens.TEMPTY;
   
        public string Lexeme { get; set; } = null;

        public int Value { get; set; } = 0;

        public float ValueR { get; set; } = 0;

        public string Literal { get; set; } = "";
 
        private string file { get; set; }

        private int location { get; set; } = 0;

        public int LineNumber { get; set; } = 1;

        public static string filepath;

        public enum Tokens
        {
            // Reserved word tokens
            TBEGIN, TMODULE, TCONSTANT, TPROCEDURE, TIS, TIF, TTHEN, TELSE,
            TELSIF, TWHILE, TLOOP, TFLOAT, TINTEGER, TCHAR, TGET, TPUT, TEND,

            // Special tokens
            TRELOP, // =,/=,<,<=,>,>=
            TADDOP, // +,-,and,or
            TMULOP, // *,/,rem,mod,and
            TASSIGNOP,  //  :=
            TLP,    // (
            TRP,    // )
            TCOMMA, // ,
            TCOLON, // :
            TSEMI,  // ;
            TPERIOD,// .
            TQUO,   // "
            TAND,   // &
            TIN,    //in
            TOUT,   //out
            TINOUT,  //inout
            TNOT,



            // Other tokens
            TEMPTY, //  Initial token
            TEOF, //    End of File
            TNUM, //    Number
            TID,  //    Identifier
            TSTR, //    String literal
            TUNKNOWN, //Unknown
            TFINAL, //  Final Token
            
        }


        // Constructor and get fileinput
        public LexicalAnalyzer(string[] filetext)
        {

            // Get file
            filepath = Getfilename(filetext);
            file = File.ReadAllText(filepath);

        }

        public string returnfilename()
        {
            return filepath;
        }
        // Get and identifiy next token
        public void GetNextToken()
        {
            RefreshToken();
            if (Check_WhiteSpace())
                ProcessToken();
        }

        // Display tokens, lexeme and attributes
        public void DisplayToken()
        {
            if(Value != 0)
                Console.WriteLine("{0, -10} | {1,-20} | {2,-5}", Token, Lexeme,Value.ToString());
            else if (ValueR != 0)
                Console.WriteLine("{0, -10} | {1,-20} | {2,-5}", Token, Lexeme,ValueR.ToString());
            else if (Literal != "")
                Console.WriteLine("{0, -10} | {1,-20} | {2,-5}", Token, Lexeme.Substring(0,17), Lexeme);
            else
                Console.WriteLine("{0, -10} | {1,-20} | {2,-5}", Token, Lexeme,"");

        }

        // Reset token along with otehr attributes
        public void RefreshToken()
        {
            if (Token != Tokens.TEOF)
                Token = Tokens.TEMPTY;

            Lexeme = "";
            Value = 0;
            ValueR = 0;
            Literal = "";
        }

        // Update cursor location
        private void GetNextCh(int space = 1)
        {
            this.location += space;
        }

        // Check char following the current char
        private bool Check_Next_Char(char c)
        {
            if (this.location + 1 == file.Length)
                return false;
            else
            {
                if (file[this.location + 1] == c)
                    return true;
                else
                    return false;
            }
        }

        // Check if there's a char next, handle space only
        private bool Check_WhiteSpace()
        {
            // Get next space until it encounters a char or the end of file
            while (this.location < file.Length && Char.IsWhiteSpace(file[this.location]))
            {
                if (file[this.location] == '\n')
                {
                    this.LineNumber++;
                }
                this.location++;
            }

            // Reaches end of file
            if (this.location >= file.Length)
            {

                Token = Tokens.TEOF;
                return false;
            }

            // Return true if it escapes all white space and encounters a char
            return true;
        }

        private void ProcessToken()
        {
            // start with letter
            if (Char.IsLetter(file[this.location]))
                ProcessWordToken();
            // start with digit
            else if (Char.IsDigit(file[this.location]))
                ProcessNumToken();
            // other cases
            else if (!Char.IsLetterOrDigit(file[this.location]))
                ProcessSymbolToken();
        }

        private void ProcessWordToken()
        {
            // Keep adding to lexeme if next following char is a letter/digit/'_'       //  || file[this.location] == ' '
            while (Char.IsLetterOrDigit(file[this.location]) || file[this.location] == '_')
            {
                Lexeme += file[this.location];
                GetNextCh();
                if (this.location == file.Length)
                    break;
                
            }
            // Identify reserved word tokens
            switch (Lexeme.ToUpper())     
            {
                case "BEGIN":
                    Token = Tokens.TBEGIN;
                    break;
                case "MODULE":
                    Token = Tokens.TMODULE;
                    break;
                case "CONSTANT":
                    Token = Tokens.TCONSTANT;
                    break;
                case "PROCEDURE":
                    Token = Tokens.TPROCEDURE;
                    break;
                case "IS":
                    Token = Tokens.TIS;
                    break;
                case "IF":
                    Token = Tokens.TIF;
                    break;
                case "THEN":
                    Token = Tokens.TTHEN;
                    break;
                case "ELSE":
                    Token = Tokens.TELSE;
                    break;
                case "ELSIF":
                    Token = Tokens.TELSIF;
                    break;
                case "WHILE":
                    Token = Tokens.TWHILE;
                    break;
                case "LOOP":
                    Token = Tokens.TLOOP;
                    break;
                case "FLOAT":
                    Token = Tokens.TFLOAT;
                    break;
                case "INTEGER":
                    Token = Tokens.TINTEGER;
                    break;
                case "CHAR":
                    Token = Tokens.TCHAR;
                    break;
                case "GET":
                    Token = Tokens.TGET;
                    break;
                case "PUT":
                    Token = Tokens.TPUT;
                    break;
                case "END":
                    Token = Tokens.TEND;
                    break;
                case "OR":
                    Token = Tokens.TADDOP;
                    break;
                case "REM":
                    Token = Tokens.TMULOP;
                    break;
                case "MOD":
                    Token = Tokens.TMULOP;
                    break;
                case "AND":
                    Token = Tokens.TMULOP;
                    break;
                case "IN":
                    Token = Tokens.TIN;
                    break;
                case "OUT":
                    Token = Tokens.TOUT;
                    break;
                case "INOUT":
                    Token = Tokens.TINOUT;
                    break;
                default:    // invalid lexeme
                    Token = Lexeme.Length > 17 ? Tokens.TUNKNOWN : Tokens.TID;
                    break;
            }
        }

        private void ProcessNumToken()
        {
            // Keep adding digits to lexeme
            while (Char.IsDigit(file[this.location]))
            {
                Lexeme += file[this.location];
                GetNextCh();
                if (this.location == file.Length)
                    break;
            }

            if (this.location == file.Length)
            {
                Token = Tokens.TNUM;
            }
            else
            {
                // check float nums
                if (file[this.location] == '.')
                {
                    Lexeme += file[this.location];
                    GetNextCh();

                    // Check case 12.xx
                    if (!Char.IsDigit(file[this.location]))
                    {
                        Console.WriteLine("Unkonwn token at Line: {0}", LineNumber);
                        Token = Tokens.TUNKNOWN;
                        return;
                    }

                    // Finish scanning the floating num
                    while (Char.IsDigit(file[this.location]))
                    {
                        Lexeme += file[this.location];
                        GetNextCh();
                        if (this.location == file.Length)
                            break;
                    }

                    ValueR = float.Parse(Lexeme, CultureInfo.InvariantCulture.NumberFormat);
                }
                else
                    Value = Convert.ToInt32(Lexeme);

                Token = Tokens.TNUM;
            }



        }

        private void ProcessSymbolToken()
        {
            Lexeme += file[this.location];

            switch (Lexeme)
            {
                case "=":
                    Token = Tokens.TRELOP;
                    GetNextCh();
                    break;
                case "/":
                    if (Check_Next_Char('='))
                    {
                        Token = Tokens.TRELOP;
                        Lexeme = "/=";
                        GetNextCh(2);
                    }
                    else
                    {
                        Token = Tokens.TMULOP;
                        Lexeme = "/";
                        GetNextCh();
                    }
                    break;
                case "<":
                    if (Check_Next_Char('='))
                    {
                        Token = Tokens.TRELOP;
                        Lexeme = "<=";
                        GetNextCh(2);
                    }
                    else
                    {
                        Token = Tokens.TRELOP;
                        Lexeme = "<";
                        GetNextCh();
                    }
                    break;
                case ">":
                    if (Check_Next_Char('='))
                    {
                        Token = Tokens.TRELOP;
                        Lexeme = ">=";
                        GetNextCh(2);
                    }
                    else
                    {
                        Token = Tokens.TRELOP;
                        Lexeme = ">";
                        GetNextCh();
                    }
                    break;
                case ":":
                    if (Check_Next_Char('='))
                    {
                        Token = Tokens.TASSIGNOP;
                        Lexeme = ":=";
                        GetNextCh(2);
                    }
                    else
                    {
                        Token = Tokens.TCOLON;
                        Lexeme = ":";
                        GetNextCh();
                    }
                    break;
                case "-":
                    if (Check_Next_Char('-'))
                        ProcessComment();
                    else
                    {
                        Token = Tokens.TADDOP;
                        GetNextCh();
                    }
                    break;
                case ".":  
                    if(Char.IsDigit(file[this.location + 1]) && !Char.IsDigit(file[this.location - 1]) && file[this.location - 1] != '.')  
                    {
                        Token = Tokens.TUNKNOWN;
                        GetNextCh();
                        while(Char.IsDigit(file[this.location]))
                        {
                            Lexeme += file[this.location];
                            GetNextCh();
                        }
                        Console.WriteLine("Unknown token at line: " + LineNumber);
                    }
                  else
                    {
                    Token = Tokens.TPERIOD;
                    GetNextCh();
                    }
                    break;
                case "+":
                    Token = Tokens.TADDOP;
                    GetNextCh();
                    break;
                case "*":
                    Token = Tokens.TMULOP;
                    GetNextCh();
                    break;
                case "(":
                    Token = Tokens.TLP;
                    GetNextCh();
                    break;
                case ")":
                    Token = Tokens.TRP;
                    GetNextCh();
                    break;
                case ",":
                    Token = Tokens.TCOMMA;
                    GetNextCh();
                    break;
                case ";":
                    Token = Tokens.TSEMI;
                    GetNextCh();
                    break;
                case "&":
                    Token = Tokens.TAND;
                    GetNextCh();
                    break;
                case "\"":
                    ProcessLiteral();
                    GetNextCh();
                    break;
                default:
                    Token = Tokens.TUNKNOWN;
                    GetNextCh();
                    break;
            }
        }

        private void ProcessLiteral()
        {
            GetNextCh();

            // Find the string literal inside " " on the same line // change
            while (file[this.location] != '"' && file[this.location] != '\n' && this.location < file.Length)
            {
                Lexeme += file[this.location];
                GetNextCh();
            }

            if (this.location < file.Length)
            {
                // Find the pairing "
                if(file[this.location] == '"')
                {
                    Lexeme += file[this.location];
                    Token = Tokens.TSTR;
                    Literal = Lexeme;
                }
                else
                {
                    Console.WriteLine("Literal unkonwn at line: {0}", LineNumber);
                    Token = Tokens.TUNKNOWN;
                }
            }

            return;
        }

        private void ProcessComment()
        {
            while (this.location < file.Length && file[this.location] != '\n')
                GetNextCh();

            GetNextToken();
        }

        // Get filename from user
        public string Getfilename(string[] args)
        {
            string filename = null;

            if (args.Length == 0)
            {
                Console.WriteLine("Please enter filename: ");
                filename = Console.ReadLine();
            }
            else
            {
                filename = args[0];
            }

            while (true)
            {
                if (!File.Exists(filename))
                {
                    Console.WriteLine("'{0}' does not exists!", filename);
                    Console.WriteLine("Please enter a filename (or Type 'q' to exit):");
                    filename = Console.ReadLine();
                    if (filename == "q" || filename == "Q")
                        return null;
                    Console.WriteLine();
                }
                else if (!filename.EndsWith(".ada"))
                {
                    Console.WriteLine("'{0}' isn't a valid ada file!", filename);
                    Console.WriteLine("Please enter a valid ADA filename (or Type 'q' to exit):");
                    filename = Console.ReadLine();
                    if (filename == "q" || filename == "Q")
                        return null;
                    Console.WriteLine();
                }
                else if (filename == null)
                {
                    Console.WriteLine("'{0}' is empty", filename);
                    Console.WriteLine("Please enter a valid filename (or Type 'q' to exit):");
                    filename = Console.ReadLine();
                    if (filename == "q" || filename == "Q")
                        return null;
                    Console.WriteLine();
                }
                else
                    break;
            }


            return filename;
        }
    }


}
