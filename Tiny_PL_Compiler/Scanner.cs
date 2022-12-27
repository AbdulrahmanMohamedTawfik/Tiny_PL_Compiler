using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

public enum Token_Class
{
    Int_Token, Float_Token, String_Token, Read_Token, Write_Token, Repeat_Token, Until_Token, If_Token, Elseif_Token, 
    Else_Token, Then_Token, End_Token, Return_Token, Endl_Token, Parameters_Token, Semicolon, Comma, LParanthesis, RParanthesis, 
    Lbrace, Rbrace, IsEqualOp, EqualOp, LessThanOp, GreaterThanOp, NotEqualOp, PlusOp, MinusOp, MultiplyOp, DivideOp, AndOp, OrOp, 
    Idenifier, Constant, Main
}
namespace TinyCompiler
{


    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("int", Token_Class.Int_Token);
            ReservedWords.Add("float", Token_Class.Float_Token);
            ReservedWords.Add("string", Token_Class.String_Token);
            ReservedWords.Add("read", Token_Class.Read_Token);
            ReservedWords.Add("write", Token_Class.Write_Token);
            ReservedWords.Add("repeat", Token_Class.Repeat_Token);
            ReservedWords.Add("until", Token_Class.Until_Token);
            ReservedWords.Add("if", Token_Class.If_Token);
            ReservedWords.Add("else", Token_Class.Else_Token);
            ReservedWords.Add("elseif", Token_Class.Elseif_Token);
            ReservedWords.Add("then", Token_Class.Then_Token);
            ReservedWords.Add("end", Token_Class.End_Token);
            ReservedWords.Add("return", Token_Class.Return_Token);
            ReservedWords.Add("endl", Token_Class.Endl_Token);
            ReservedWords.Add("parameters", Token_Class.Parameters_Token);
            ReservedWords.Add("Main", Token_Class.Main);

            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add("(", Token_Class.LParanthesis);
            Operators.Add(")", Token_Class.RParanthesis);
            Operators.Add("{", Token_Class.Lbrace);
            Operators.Add("}", Token_Class.Rbrace);
            Operators.Add("=", Token_Class.IsEqualOp);
            Operators.Add(":=", Token_Class.EqualOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("<>", Token_Class.NotEqualOp);
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add("&&", Token_Class.AndOp);
            Operators.Add("||", Token_Class.OrOp);
        }

        public void StartScanning(string SourceCode)
        {
            for (int i = 0; i < SourceCode.Length; i++)
            {
                int j = i;
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = CurrentChar.ToString();
                //space or new line
                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\t' || CurrentChar == '\n')
                    continue;

                //comment case
                //    /*int x;
                else if (CurrentChar == '/')//if you find '/'
                {
                    //Console.WriteLine("you find '/'\n");
                    bool is_complete_comment = false;//opened and closed
                    bool opened_comment = false;//only opened (not closed)
                    //start from char after '/'
                    j = i + 1;
                    if (j < SourceCode.Length)//if there is a char after '/'
                    {
                        //Console.WriteLine("there is a char after '/'\n");

                        if (SourceCode[j] == '*')//if there is '*' after '/'
                        {
                            CurrentChar = SourceCode[j];
                            CurrentLexeme += CurrentChar.ToString();
                            //Console.WriteLine("there is '*' after '/'\n");
                            opened_comment = true;
                            j++;
                            for (; j < SourceCode.Length; j++)
                            {
                                //Console.WriteLine("j= ");
                                //Console.WriteLine(j);
                                //Console.WriteLine("\n");
                                CurrentChar = SourceCode[j];
                                CurrentLexeme += CurrentChar.ToString();
                                if (CurrentChar != '*')
                                {
                                    continue;
                                }
                                else//if you found '*' ex)/*aaa*
                                {
                                    //Console.WriteLine("you found '*' ex)/*aaa*\n");
                                    //start from char after '*'
                                    int x = j + 1;
                                    if (x < SourceCode.Length)//if there is a char after '*'
                                    {
                                        //Console.WriteLine("there is a char after '*'\n");
                                        CurrentChar = SourceCode[x];
                                        CurrentLexeme += CurrentChar.ToString();
                                        if (CurrentChar == '/')//if you found '/' ex)/*aaa*/
                                        {
                                            //Console.WriteLine("you found '/' ex)/*aaa*/\n");
                                            is_complete_comment = true;//يبقي ده كومنت
                                            break;
                                        }
                                        else//not '/'   ex)/*aaa*a
                                        {
                                            continue;
                                        }
                                    }
                                    else//'*' at the end   /*aaa*
                                    {
                                        FindTokenClass(CurrentLexeme);
                                    }
                                }
                            }
                        }
                        else//there is no '*' after '/'   ex)'/a' or '/4'
                        {
                            is_complete_comment = false;
                            opened_comment = false;
                        }
                    }
                    //else// '/' at the end
                    //    FindTokenClass(CurrentChar.ToString());
                    if (is_complete_comment)
                    {
                        //Console.WriteLine("comment\n");
                        i = j + 1;
                    }
                    else if (opened_comment)
                    {
                        i = j + 1;
                        FindTokenClass(CurrentLexeme);
                    }
                    else
                        FindTokenClass(CurrentChar.ToString());
                }

                //equal operator :=
                else if (CurrentChar == ':')
                {
                    j = i + 1;
                    if (j < SourceCode.Length)
                    {
                        CurrentChar = SourceCode[j];
                        if (CurrentChar == '=')
                        {
                            CurrentLexeme += CurrentChar.ToString();
                            FindTokenClass(CurrentLexeme);
                        }
                        i = j;
                    }
                    else
                    {
                        //Console.WriteLine(": is the last char\n");
                        FindTokenClass(CurrentLexeme);
                    }
                }
                //not equal operator <>
                else if (CurrentChar == '<')
                {
                    j = i + 1;
                    if (j < SourceCode.Length)
                    {
                        CurrentChar = SourceCode[j];
                        if (CurrentChar == '>')//not equal <>
                        {
                            CurrentLexeme += CurrentChar.ToString();
                            FindTokenClass(CurrentLexeme);
                            i = j;
                        }
                        else if (CurrentChar == '=')//<= is not accepted
                        {
                            CurrentLexeme += CurrentChar.ToString();
                            Errors.Error_List.Add("Unidentified Token \'" + CurrentLexeme + "\'");
                            i = j;
                        }
                        else//read it as less than operator <
                        {
                            FindTokenClass(CurrentLexeme);
                        }
                    }
                    else
                    {
                        //Console.WriteLine("< is the last char\n");
                        FindTokenClass(CurrentLexeme);
                    }

                }
                //>= is not accepted
                else if (CurrentChar == '>')
                {
                    j = i + 1;
                    if (j < SourceCode.Length)
                        CurrentLexeme += SourceCode[j];
                    if (CurrentLexeme == ">=")
                    {
                        Errors.Error_List.Add("Unidentified Token: (" + CurrentLexeme + ")");
                        i = j;
                    }
                    else
                    {
                        FindTokenClass(">");
                    }
                }
                // AND operator
                else if (CurrentChar == '&')
                {
                    j = i + 1;
                    if (j < SourceCode.Length)
                        CurrentLexeme += SourceCode[j];
                    i = j + 1;
                    FindTokenClass(CurrentLexeme);
                }
                // OR opeartor
                else if (CurrentChar == '|' && SourceCode[i + 1] == '|')
                {
                    j = i + 1;
                    if (j < SourceCode.Length)
                        CurrentLexeme += SourceCode[j];
                    i = j + 1;
                    FindTokenClass(CurrentLexeme);
                }
                //string case
                else if (CurrentChar == '\"')
                {
                    j = i + 1;
                    if (j < SourceCode.Length)
                    {
                        CurrentChar = SourceCode[j];
                        CurrentLexeme += CurrentChar.ToString();
                        while (CurrentChar != '\"')
                        {
                            j++;
                            if (j < SourceCode.Length)
                            {
                                CurrentChar = SourceCode[j];
                                CurrentLexeme += CurrentChar.ToString();
                            }
                            else
                                break;
                        }
                    }
                    i = j;
                    FindTokenClass(CurrentLexeme);
                }

                //if you read a character
                else if (CurrentChar >= 'A' && CurrentChar <= 'z')
                {
                    j = i + 1;
                    if (j < SourceCode.Length)
                    {
                        CurrentChar = SourceCode[j];

                        while ((CurrentChar >= 'A' && CurrentChar <= 'z') || CurrentChar >= '0' && CurrentChar <= '9')
                        {
                            CurrentLexeme += CurrentChar.ToString();
                            j++;
                            if (j >= SourceCode.Length)
                                break;
                            CurrentChar = SourceCode[j];
                        }
                    }
                    FindTokenClass(CurrentLexeme);
                    i = j - 1;
                }
                //if you read a number
                else if (CurrentChar >= '0' && CurrentChar <= '9' || CurrentChar == '.')
                {
                    j = i + 1;
                    if (j < SourceCode.Length)
                    {
                        CurrentChar = SourceCode[j];

                        while ((CurrentChar >= '0' && CurrentChar <= '9') || (CurrentChar >= 'A' && CurrentChar <= 'z') || CurrentChar.Equals('.'))
                        {
                            CurrentLexeme += CurrentChar.ToString();

                            j++;
                            if (j >= SourceCode.Length)
                                break;
                            if (j < SourceCode.Length)

                                CurrentChar = SourceCode[j];

                        }
                    }

                    FindTokenClass(CurrentLexeme);
                    i = j - 1;
                }
                else
                {
                    FindTokenClass(CurrentLexeme);
                }
            }

            Tiny_PL_compiler.TokenStream = Tokens;
        }
        void FindTokenClass(string Lex)
        {
            //var rx_string = new Regex(@"[a - z | 0 - 9]+", RegexOptions.Compiled);

            Token_Class TC;
            Token Tok = new Token();
            Tok.lex = Lex;
            //is it a string?
            if (Lex[0] == '"' && Lex[Lex.Length - 1] == '"')
            {
                TC = Token_Class.String_Token;
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }
            //Is it a reserved word?   (include equal := and not equal <>)
            else if (ReservedWords.ContainsKey(Lex))
            {
                TC = ReservedWords[Lex];
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }
            else if (Lex == "main")
            {
                TC = Token_Class.Main;
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }
            //Is it an Identifier?
            else if (isIdentifier(Lex))
            {
                //Console.WriteLine("identifier");
                TC = Token_Class.Idenifier;
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }
            //Is it an Operator?
            else if (Operators.ContainsKey(Lex))
            {
                TC = Operators[Lex];
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }

            //Is it a Constant?
            else if (isConstant(Lex))
            {
                //Console.WriteLine("constant");
                TC = Token_Class.Constant;
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }

            else
            {
                Errors.Error_List.Add("Unidentified Token: (" + Lex + ")");
            }

        }

        bool isIdentifier(string lex)
        {
            bool isValid = true;
            if (!(lex[0] >= 'A' && lex[0] <= 'z'))
            { isValid = false; }
            else if (lex == "main")
            {
                isValid = false;
            }
            else
            {
                for (int i = 1; i < lex.Length; i++)
                {
                    if (!(lex[i] >= 'A' && lex[i] <= 'z')
                        && !(lex[i] >= '0' && lex[i] <= '9'))
                    {
                        isValid = false;
                    }
                }
            }
            return isValid;
        }
        bool isConstant(string lex)
        {
            bool isValid = true;
            //starting with '.'
            if (lex[0] == '.')
            {
                isValid = false;
            }
            //ending with '.'
            else if (lex[lex.Length - 1] == '.')
            {
                isValid = false;
            }
            else
            {
                bool dot = false;
                for (int i = 0; i < lex.Length; i++)
                {
                    //all is numbers or '.'
                    if (!((lex[i] >= '0' && lex[i] <= '9') || lex[i] == '.'))
                    {
                        isValid = false;
                    }
                    //handle two dots (1.18.123)
                    if (dot && lex[i] == '.')
                    {
                        isValid = false;
                    }
                    if (lex[i] == '.')
                    {
                        dot = true;
                    }
                }
            }
            return isValid;
        }
    }
}
