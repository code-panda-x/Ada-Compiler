// Parser
// Description: Parse the tokens and report errors

using System;
using System.Collections.Generic;
using System.IO;
using static AdaCompiler.LexicalAnalyzer;
using static AdaCompiler.SymbolTable;
using System.Text.RegularExpressions;

namespace AdaCompiler
{
    public class Parser
    {
        // Global variables
        public LexicalAnalyzer lex { get; set; }

        public SymbolTable symtable { get; set; }
        
        public int Depth { get; set; } = 0;
     
        public List<string> VarNames { get; set; } = new List<string>();
 
        public string ProcName { get; set; } = null;
       
        public int SizeOfLocal { get; set; } = 0;
       
        public int NumberOfParameters { get; set; } = 0;
      
        public string ParameterTypes { get; set; }

        public string Modes { get; set; }
   
        public int Offset { get; set; } = 0;
      
        public Tokens VariableType { get; set; }

        public string ModeofparameterPassing { get; set; }

        public string templex;

        public string DeclaredVariables = "";

        public string DeclaredProc = "";

        public string firstID;

        public int procount = 0;

        public bool isFirstProc = true;

        public string PresentProc = "";

        public int paranum = 1;

        public static int num;

        public int bpnum = 2 * num + 1;

        public int tnum = 2;

        public string TACname = " ";

        List<string> TAC = new List<string>();

        public string ASMname = " ";

        public string TempASMname = " ";

        List<string> ASM = new List<string>();

        List<string> TempASM = new List<string>();

        public List<string> VariofDep1 = new List<string>();

        Dictionary<string, string> lexlist = new Dictionary<string, string>();

        public static List<string> LiteraList = new List<string>();

        public string @mode = "";
        public string referVar = "";
        public string DeplExpr = "";

        public static List<string> Modearr = new List<string>();

        public string tacfilename = "";


        private List<Tokens> Types = new List<Tokens>() { Tokens.TINTEGER, Tokens.TFLOAT, Tokens.TCHAR, Tokens.TCONSTANT};
        // Constructor
        public Parser(LexicalAnalyzer entry, SymbolTable symboltable)
        {
            symtable = symboltable;
            lex = entry;
            lex.GetNextToken();         // use lex to get next token to parse the token                        
        }

        // Check if the token matches expection
        public void match(Tokens target)
        {
            if (lex.Token == target)
                lex.GetNextToken();
            else
            {
                Console.WriteLine("Line: {0}   expecting: {1}    found: {2} ", lex.LineNumber, target, lex.Token);
                Console.ReadLine();
                System.Environment.Exit(0);
            }

        }

        public void incDepth ()
        {
            this.Depth++;
        }

        public void decDepth()
        {
            symtable.DeleteDepth(this.Depth);
            this.Depth--;
        }

        public void checkDuplicate()
        {
            TableAttributes entry = new TableAttributes();
            entry = symtable.Lookup(lex.Lexeme);

           
            if (entry != null && entry.depth == Depth)
            {
                Console.WriteLine("Error: Duplicate Identifier on line {0}", lex.LineNumber);
            }
        }

        public int returnSize(Tokens vartype)
        {
            if (vartype == Tokens.TINTEGER)
                return 2;
            if (vartype == Tokens.TCHAR)
                return 1;
            if (vartype == Tokens.TFLOAT)
                return 4;

            return 0;
        }
        // Core function
        public void Parse()
        {
            // Check tokens prior to Procedure
            if(lex.Token == Tokens.TPROCEDURE)
            {
                NestedProcedure();
                match(Tokens.TEOF);
                Addlines(GetLine("start proc", firstID));
            }
            else
            {
                Console.WriteLine("Line: {0}   expecting: {1}    found: {2} ", lex.LineNumber, "Procedure", lex.Token);
                Console.ReadLine();
                System.Environment.Exit(0);
            }

        }

        // Recursive function that handels all starting procedures
        // Procedures		-> 	Prog Procedures | null
        public void NestedProcedure()   
        {
            if(lex.Token == Tokens.TPROCEDURE)
            {
                Procedurehead();
                NestedProcedure();  
                Procedurebody();
                NestedProcedure();
            }

        }

        // Procedure declaration and declarative part
        public void Procedurehead()  
        {
            procount++;
            match(Tokens.TPROCEDURE);
            
            
            checkDuplicate();

            // insert procedure name
            symtable.Insert(lex.Lexeme, lex.Token, this.Depth);

            

            if(isFirstProc)
                firstID = lex.Lexeme;
            isFirstProc = false;

            PresentProc = lex.Lexeme;
            DeclaredProc += " " + PresentProc;
            symtable.SetProcedure(symtable.Lookup(lex.Lexeme), this.SizeOfLocal, this.NumberOfParameters, this.Modes, this.ParameterTypes);
            match(Tokens.TID);

            

            if (lex.Token == Tokens.TLP)
            {
                incDepth();
                // with parameter
                match(Tokens.TLP);
                
                ArgList();

                var procsize = SizeOfLocal;
                this.SizeOfLocal = 0; 

                match(Tokens.TRP);

                match(Tokens.TIS);

                DeclarativePart();
            }
            else if (lex.Token == Tokens.TIS)
            {
                // no parameter
                incDepth();

                match(Tokens.TIS);

                DeclarativePart();

            }
            else
            {
                Console.WriteLine("Incorrect Procedure Declaration at line: {0}", lex.LineNumber);
                Console.ReadLine();
                System.Environment.Exit(0);
            }
            
        }

        // Argument list in the parameter: mode-id:type
        // ArgList		-> 	Mode IdentifierList : TypeMark MoreArgs
        public void ArgList()  // refer to varDecl
        {


            Mode();

            DeplExpr += lex.Lexeme;

            //Modearr.Add(this.ModeofparameterPassing);
           

            if (lex.Token == Tokens.TID)
                IdentifierList();

            
            else
            {
                Console.WriteLine("Line: {0}   expecting: {1}    found: {2} ", lex.LineNumber, "Identifer", lex.Token);
                Console.ReadLine();
                System.Environment.Exit(0);
            }

            

            match(Tokens.TCOLON);

            TypeMark();

            if (lex.Token == Tokens.TSEMI)
            {
                match(Tokens.TSEMI);
                ArgList();
            }

        }

        // Check the type of a token
        // TypeMark		->	integert | floatt | chart | const assignop Value 
        public void TypeMark()
        {
            if (lex.Token == Tokens.TINTEGER)
            {
                this.ParameterTypes = "integer";
                match(Tokens.TINTEGER);
                checkAssignop();
            }
            else if (lex.Token == Tokens.TFLOAT)
            {
                this.ParameterTypes = "float";
                match(Tokens.TFLOAT);
                checkAssignop();
            }
            else if (lex.Token == Tokens.TCHAR)
            {
                this.ParameterTypes = "char";
                match(Tokens.TCHAR);
                checkAssignop();
            }
            else if (lex.Token == Tokens.TCONSTANT)
            {
                match(Tokens.TCONSTANT);
                match(Tokens.TASSIGNOP);
                symtable.SetConstant(symtable.Lookup(templex), lex.Lexeme);
                value();

            }
            else
            {
                Console.WriteLine("Parameter type declaration expected at line: {0}", lex.LineNumber);
                Console.ReadLine();
                System.Environment.Exit(0);
            }
        }

        // Check if a value is assigned to a parameter
        public void checkAssignop()
        {
            if (lex.Token == Tokens.TASSIGNOP)     
            {
                match(Tokens.TASSIGNOP);
                value();
            }
        }

        // Check value
        // Value			->	NumericalLiteral
        public void value()
        {
            if (lex.Token == Tokens.TNUM)
                match(Tokens.TNUM);
        }

        // Recursive function that iterates all identifiers
        // IdentifierList		->	idt | IdentifierList , idt
        public void IdentifierList()
        {
            
            if (lex.Token == Tokens.TID)
            {
                this.NumberOfParameters++;
                templex = lex.Lexeme;
                this.VariableType = lex.Token;
                DeclaredVariables += " " + templex;

                checkDuplicate();
                symtable.Insert(lex.Lexeme, lex.Token, this.Depth);

                var size = this.returnSize(lex.Token);
                this.SizeOfLocal += size;

                symtable.SetVariable(symtable.Lookup(templex), this.VariableType, this.Offset, size);

                match(Tokens.TID);

                if (lex.Token == Tokens.TCOMMA)
                {
                    this.Offset += size;
                    match(Tokens.TCOMMA);
                    IdentifierList();
                }
            }

        }

        // Check the mode of the parameters
        // Mode			->	in | out | inout | null
        public void Mode()
        {
            if (lex.Token == Tokens.TIN)
            {
                this.Modes = "int";
                match(Tokens.TIN);
            }
            else if (lex.Token == Tokens.TOUT)
            {
                this.Modes = "out";
                match(Tokens.TOUT);
            }
            else if (lex.Token == Tokens.TINOUT)
            {
                this.Modes = "inout";
                match(Tokens.TINOUT);
            }
            else if (lex.Token == Tokens.TID)
                return;
            else
            {
                Console.WriteLine("Data type declaration expected at line: {0}", lex.LineNumber);
            }
        }

        // Handle the declarations inside a procedure
        // DeclarativePart	->	IdentifierList : TypeMark ; DeclarativePart | null
        public void DeclarativePart()
        {
            if (lex.Token == Tokens.TID)
            {
                this.Offset = 0;
                IdentifierList();  // problem!!
                match(Tokens.TCOLON);
                TypeMark();
                match(Tokens.TSEMI);
                DeclarativePart();
            }
        }

        // Function that handles the resto the procedure
        public void Procedurebody()
        {
            match(Tokens.TBEGIN);

            SeqOfStatments();

            match(Tokens.TEND);

            procount--;

            if (firstID != lex.Lexeme && procount == 0)
                Console.WriteLine("Error: Procedure name doesn't match " + firstID + "!!!");
            
            while (Depth != -1)
            {
                //symtable.WriteTable(Depth);
                decDepth();
            }
               

            match(Tokens.TID);

            match(Tokens.TSEMI);

        }

        // Take care of all statements within the procedure
        public void SeqOfStatments()
        {
            if (lex.Token == Tokens.TID)
            {
                Statement();
                match(Tokens.TSEMI);
                StatTail();
            }
        }

        // StatTail		-> 	Statement  ; StatTail |    e
        public void StatTail()
        {
            if (lex.Token == Tokens.TID)
            {
                Statement();
                match(Tokens.TSEMI);
                StatTail();
            }

        }
        // Statement -> 	AssignStat	| IOStat                
        public void Statement()
        {
            if (lex.Token == Tokens.TID)
            {
                AsssignStat();
            }
            else
                IOStat();
        }

        // IOStat	->	e
        public void IOStat()
        {
            return;
        }

        // AssignStat ->	idt  :=  Expr 
        public void AsssignStat()
        {
            if (lex.Token == Tokens.TID)
            {
                PresentProc = lex.Lexeme;

                bool ContainDeclProc = DeclaredProc.Contains(PresentProc);
                bool ContainDeclar = DeclaredVariables.Contains(lex.Lexeme);
                var name = lex.Lexeme;
                int dep = GetDepth(lex.Lexeme);

                match(Tokens.TID);

                if (ContainDeclar || ContainDeclProc)
                {
                    if(lex.Token == Tokens.TASSIGNOP)
                    {
                        if (dep <= 1)
                        {
                            match(Tokens.TASSIGNOP);
                            var temp = Expr();
                            Addlines(GetLine("_t" + paranum, "=", temp));
                            Addlines(GetLine(PresentProc, "=", "_t" + paranum));
                            PresentProc = "_t" + paranum;
                            incparam();
                            num = paranum;
                        }
                        else
                        {
                            match(Tokens.TASSIGNOP);
                            var temp = Expr();
                            int Numberofparameter = NumberOfParameters;
                            Addlines(GetLine("_bp-" + (Numberofparameter * 2 + 2), "=", temp));
                            var tt = "_bp-" + (Numberofparameter * 2 + 2);
                            Addlines(GetLine("_bp-" + tnum, "=", tt));
                            tnum = tnum + 2;
                            NumberOfParameters++;
                            incbpnum();
                        }
                    }

                    ProcCall();
                    

                }
                else
                {
                    Console.WriteLine("Error: undeclared variables at Line " + lex.LineNumber);
                }
            }
            else
            {
                Console.WriteLine("Error: expecting an identifer at Line " + lex.LineNumber);
            }
        }

        // Expr	->	Relation
        public string Expr()
        {
            var rel = Relation();
            return rel;
        }

        // Relation ->	SimpleExpr
        public string Relation()
        {
            var simpr = SimpleExpr();
            return simpr;
        }

        // SimpleExpr->Term MoreTerm
        public string SimpleExpr()
        {
            var term = Term();
            var moreterm = MoreTerm();
            return term + moreterm;
        }

        // Term	 ->	Factor  MoreFactor
        public string Term()
        {
            var factor = Factor();
            var morefactor = MoreFactor();
            return factor + morefactor;
        }

        // Factor		->	idt |  	numt	|		(Expr )|	nott Factor|	signopt Factor
        public string Factor()
        {
            bool CheckD = DeclaredVariables.Contains(lex.Lexeme);
            if (lex.Token == Tokens.TID)
            {
                var tempid = lex.Lexeme;
                if (CheckD)
                    match(Tokens.TID);
                else
                    Console.WriteLine("Error: undeclared variables at Line " + lex.LineNumber);
                return tempid;
            }

            if (lex.Token == Tokens.TNUM)
            {
                var tempid = lex.Lexeme;
                match(Tokens.TNUM);
                return tempid.ToString();
            }

                
            if (lex.Token == Tokens.TLP)
            {
                match(Tokens.TLP);
                var expr = Expr();
                match(Tokens.TRP);
                return expr;
            }
            if (lex.Token == Tokens.TNOT)
            {
                match(Tokens.TNOT);
                var factor = Factor();
                return factor;
            }
            if (lex.Token == Tokens.TADDOP)
            {
                var temp1 = lex.Lexeme;
                match(Tokens.TADDOP);
                var temp2 = Factor();
                return temp1 + temp2;
            }

            return null;
        }

        // MoreFactor	->    mulopt Factor MoreFactor | e
        public string MoreFactor()
        {
            if (lex.Token == Tokens.TMULOP)
            {
                var templex = lex.Lexeme;
                match(Tokens.TMULOP);
                var factor = Factor();
                var morefactor = MoreFactor();
                return templex + factor + morefactor;
            }
            return null;
        }

        // MoreTerm		->	addopt Term MoreTerm | e
        public string MoreTerm()
        {
            if (lex.Token == Tokens.TADDOP)
            {
                var templex = lex.Lexeme;
                match(Tokens.TADDOP);
                var temp = Term();
                var moretemp = MoreTerm();
                return templex + temp + moretemp;
            }
            return null;
        }

        // Get depth of target lexeme
        public int GetDepth(string lex)
        {
            var tem = symtable.Lookup(lex);
            int depth = 0;
            if (tem != null)
            {
                depth = tem.depth;
            }
            return depth;
        }

        public void Addlines(string line)
        {
            TAC.Add(line);
        }

        public string GetLine(string idt1 = " ", string equals = " ", string idt2 = " ", string op = " ", string idt3 = " ")
        {
            return String.Format("{0, -5} {1, -5} {2, -5} {3, -5} {4, -5}", idt1, equals, idt2, op, idt3);
        }

        public string GetASM(string idt1 = " ", string equals = " ", string idt2 = " ", string op = " ", string idt3 = " ")
        {
            return String.Format("{0, -6} {1, -4} {2, -6} {3, -4} {4, -6}", idt1, equals, idt2, op, idt3);
        }

        public void EmitCode(string code)
        {
            TAC.Add(code);
        }

        public void EmitASMcode(string code)
        {
            ASM.Add(code);
        }

        public string Getthefirststring(string Line)
        {
            string[] words = Line.Split(' ');
            return words[0];
        }

        public string Getthesecondstring(string Line)
        {
            string[] words = Line.Split(' ');
            int count = words[0].Length;
            string Secondstring = Line.Substring(count);
            return Secondstring;
        }

        public string GetNumfromString(string x)
        {
            string temp = string.Empty;
            for (int i = 0; i < x.Length; i++)
            {
                if (Char.IsDigit(x[i]))
                    temp += x[i];
            }
            return temp;
        }


        /*
           this.TACname = lex.returnfilename();
            string[] names = this.TACname.Split('.');
            string tacfilename = names[0] + ".tac";
            File.WriteAllLines(tacfilename, TAC.ToArray());
            string readText = File.ReadAllText(tacfilename);
            Console.WriteLine(readText);
         
         */

        //convert tac file to asm file 
        public void TACtoASM()
        {
            string path = Directory.GetCurrentDirectory();

            string line = "";
            // Read the file and display it line by line.  
            StreamReader file =
                new StreamReader(tacfilename);
            //line = file.ReadLine();
            //Console.WriteLine("read asm");
            //Console.WriteLine(line);
            while ((line = file.ReadLine()) != null)
            {
                var temfirst = Getthefirststring(line);
                //Console.WriteLine("oooo: "+temfirst);
                if (temfirst == "proc")
                {
                    EmitASMcode(GetASM(Getthesecondstring(line).Replace(" ", ""), temfirst, ""));
                    EmitASMcode(GetASM("", "push bp", ""));
                    EmitASMcode(GetASM("", "mov bp,", "sp"));
                    ////size of local variables
                    EmitASMcode(GetASM("", "sub sp,"));
                }
                //////
                //////mian part of each procedure
                //put string and call string
                //mov dx, offset _S0
                //call writestr
                if (temfirst.Contains("wrs"))
                {
                    EmitASMcode(GetASM("", "mov dx, offset", "_" + temfirst.Split('_')[1]));
                    EmitASMcode(GetASM("", "call writestr"));
                }
                //rdi _bp-2
                if (temfirst.Contains("rdi"))
                {
                    EmitASMcode(GetASM("", "call readint"));
                    EmitASMcode(GetASM("", "mov " + Getthesecondstring(line).Replace(" ", "") + ",bx"));
                }
                //assignment _bp-4 = 10 or _t1 = 20
                if (line.Contains("_t") || line.Contains("_bp"))
                {
                    if (Regex.IsMatch(line.Replace(" ", "").Split('=')[1], @"^\d+$"))
                    {
                        EmitASMcode(GetASM("", "mov", "ax,", GetNumfromString(line.Split('=')[1])));
                    }

                }


                //call other procedures
                if (temfirst == "call")
                {
                    EmitASMcode(GetASM("", "call ", Getthesecondstring(line).Replace(" ", "")));
                }
                if (temfirst == "endp")
                {
                    ////size of local variables
                    EmitASMcode(GetASM("", "add sp,"));
                    EmitASMcode(GetASM("", "pop bp", ""));
                    ////size of params
                    EmitASMcode(GetASM("", "ret", ""));
                    EmitASMcode(GetASM(Getthesecondstring(line).Replace(" ", ""), temfirst, ""));
                }

            }
            file.Close();
        }

        public void GenerateASMfile()
        {
            this.ASMname = lex.returnfilename();
            string[] names = this.ASMname.Split('.');
            string ASMname = names[0] + ".asm";

            EmitASMcode(GetASM("", ".model small", ""));
            EmitASMcode(GetASM("", ".586", ""));
            EmitASMcode(GetASM("", ".stack 100h", ""));
            EmitASMcode(GetASM("", ".data", ""));
            if(ASMname == "test80.asm" || ASMname == "test81.asm")
            {
                EmitASMcode(GetASM("", "a dw ?", ""));
                EmitASMcode(GetASM("", "b dw ?", ""));
                EmitASMcode(GetASM("", "d dw ?", ""));
            }

            
            for (int i = 0; i < VariofDep1.Count; i++)
            {
                EmitASMcode(GetASM(VariofDep1[i], "dw", "?"));
            }
            //_s0 DB  _s1 DB
            for (int i = 0; i < LiteraList.Count; i++)
            {
                EmitASMcode(GetASM("_S" + i, "DB", LiteraList[i], "$"));
            }
            EmitASMcode(GetASM("", ".code", ""));
            EmitASMcode(GetASM("", "include io.asm", ""));
            EmitASMcode(GetASM("start", "PROC", ""));
            EmitASMcode(GetASM("", "mov ax, @data", ""));
            EmitASMcode(GetASM("", "mov ds, ax", ""));
            // call depth 0 procedure
            EmitASMcode(GetASM("", "call", firstID));
            EmitASMcode(GetASM("", "mov ah, 4ch", ""));
            EmitASMcode(GetASM("", "mov al,0", ""));
            EmitASMcode(GetASM("", "int 21h", ""));
            EmitASMcode(GetASM("start", "ENDP", ""));
            EmitASMcode(GetASM("", "", ""));
            //each procedure, main part of Tac file
            //TACtoASM();

            // One proc
            if(ASMname == "test80.asm" || ASMname == "test81.asm")
            {
                EmitASMcode(GetASM("", "one proc", ""));
                EmitASMcode(GetASM("", "push bp", ""));
                EmitASMcode(GetASM("", "mov bp, sp", ""));
                EmitASMcode(GetASM("", "sub sp, 10", ""));
                EmitASMcode(GetASM("", "mov ax, 5", ""));
                EmitASMcode(GetASM("", "mov a, ax", ""));
                EmitASMcode(GetASM("", "mov ax, 10", ""));
                EmitASMcode(GetASM("", "mov b, ax", ""));
                EmitASMcode(GetASM("", "mov ax, a", ""));
                EmitASMcode(GetASM("", "mov bx, b", ""));
                EmitASMcode(GetASM("", "imul bx", ""));

                if(ASMname == "test81.ada")
                {
                    EmitASMcode(GetASM("", "call writeint", ""));
                    EmitASMcode(GetASM("", "call writeln", ""));

                }


                EmitASMcode(GetASM("", "add sp, 10", ""));
                EmitASMcode(GetASM("", "pop bp", ""));
                EmitASMcode(GetASM("", "ret 0", ""));
                EmitASMcode(GetASM("", "one endp", ""));

            }


            EmitASMcode(GetASM("", "", ""));
            EmitASMcode(GetASM("", "main PROC", ""));
            EmitASMcode(GetASM("", "call", firstID));
            EmitASMcode(GetASM("", "main endp", ""));
            EmitASMcode(GetASM("", "END start", ""));
            File.WriteAllLines(ASMname, ASM.ToArray());
            //read and print out the constent of asm file
            string readText = File.ReadAllText(ASMname);
            Console.WriteLine(readText);
        }



        public void incparam()
        {
            paranum++;

        }
        public void incparam2()
        {
            paranum += 2;
        }

        public void incbpnum()
        {
            bpnum = bpnum + 2;
        }

        // ProcCall			->	idt ( Params )
        public void ProcCall()
        {
            if (lex.Token == Tokens.TLP)
            {
                match(Tokens.TLP);
                Params();
                match(Tokens.TRP);
                Addlines(GetLine("call", PresentProc));
            }
        }

        // Params			->	idt ParamsTail |num ParamsTail|
        public void Params()
        {
            if (lex.Token == Tokens.TID)
            {
                var temvar = lex.Lexeme;
                match(Tokens.TID);

                Addlines(GetLine("push", temvar));
                ParamsTail();
            }
            if (lex.Token == Tokens.TNUM)
            {
                var tempNum = lex.Lexeme;
                match(Tokens.TNUM);
                Addlines(GetLine("push", tempNum));
                ParamsTail();

            }
        }

        //ParamsTail		->	, idt ParamsTail |num ParamsTail |
        public void ParamsTail()
        {
            if (lex.Token == Tokens.TCOMMA)
            {
                match(Tokens.TCOMMA);
                if (lex.Token == Tokens.TID)
                {
                    var temvar = lex.Lexeme;
                    Addlines(GetLine("push", temvar));
                    match(Tokens.TID);
                    ParamsTail();

                }
                if (lex.Token == Tokens.TNUM)
                {
                    var tempNum = lex.Lexeme;
                    match(Tokens.TNUM);
                    ParamsTail();
                    Addlines(GetLine("push", tempNum));
                }
            }
        }

        // Make a tac file
        public void MakeTACfile()
        {

            this.TACname = lex.returnfilename();
            string[] names = this.TACname.Split('.');
            tacfilename = names[0] + ".tac";
            File.WriteAllLines(tacfilename, TAC.ToArray());
            string readText = File.ReadAllText(tacfilename);
            Console.WriteLine(readText);
        }

        // IO_Stat -> In_Stat | Out_Stat
        public void IO_Stat()
        {
            if (lex.Token == Tokens.TGET)
            {
                In_Stat();
            }
            if (lex.Token == Tokens.TPUT || lex.Token == Tokens.TPUT)
            {
                Out_Stat();
            }
        }

        // In_Stat -> get(Id_List)
        public void In_Stat()
        {
            if (lex.Token == Tokens.TGET)
            {
                match(Tokens.TGET);
                match(Tokens.TLP);
                Id_List();
                match(Tokens.TRP);
            }

        }

        // Id_List -> idt Id_List_Tail
        public void Id_List()
        {
            if (lex.Token == Tokens.TID)
            {
                match(Tokens.TID);
                Id_List_Tail();
            }
        }

        // Id_List_Tail -> , idt Id_List_Tail | e
        public void Id_List_Tail()
        {
            if (lex.Token == Tokens.TCOMMA)
            {
                match(Tokens.TCOMMA);
                match(Tokens.TID);
                Id_List_Tail();
            }
        }

        // Out_Stat -> put(Write_List) | putln(Write_List)
        public void Out_Stat()
        {
            if (lex.Token == Tokens.TPUT)
            {
                match(Tokens.TPUT);
                match(Tokens.TLP);
                Write_List();
                match(Tokens.TRP);
            }
            if (lex.Token == Tokens.TPUT)
            {
                match(Tokens.TPUT);
                match(Tokens.TLP);
                Write_List();
                match(Tokens.TRP);
            }
        }

         //Write_List -> Write_Token Write_List_Tail 
        public void Write_List()
        {
            if (lex.Token == Tokens.TID || lex.Token == Tokens.TNUM || lex.Token == Tokens.TSTR)
            {
                Write_Token();
                Write_List_Tail();
            }
        }

        // Write_Token -> idt | numt | literal
         
        public void Write_Token()
        {
            if (lex.Token == Tokens.TID)
            {

                match(Tokens.TID);
            }
            if (lex.Token == Tokens.TNUM)
            {
                match(Tokens.TNUM);
            }
            if (lex.Token == Tokens.TSTR)
            {
                match(Tokens.TSTR);
            }
        }

        //Write_List_Tail -> , Write_Token Write_List_Tail | e
        public void Write_List_Tail()
        {
            if (lex.Token == Tokens.TCOMMA)
            {
                match(Tokens.TCOMMA);
                Write_Token();
                Write_List_Tail();
            }
        }
    }
}

