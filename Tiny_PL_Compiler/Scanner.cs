using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum Token_Class
{
    Int, Float, String, Read, Write, Repeat, Until, If, Elseif, Else, Then, End, Return,
    Endl, Parameters, Semicolon, Comma, LParanthesis, RParanthesis, Lbrace, Rbrace, IsEqualOp, EqualOp, LessThanOp,
    GreaterThanOp, NotEqualOp, PlusOp, MinusOp, MultiplyOp, DivideOp, AndOp, OrOp, Idenifier, Constant
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
            ReservedWords.Add("int", Token_Class.Int);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("elseif", Token_Class.Elseif);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("end", Token_Class.End);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("endl", Token_Class.Endl);
            ReservedWords.Add("parameters", Token_Class.Parameters);

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
            Operators.Add("||", Token_Class.OrOp);
            Operators.Add("&&", Token_Class.AndOp);
        }

        public void StartScanning(string SourceCode)
        {
            for (int i = 0; i < SourceCode.Length; i++)
            {
                int j = i;
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = CurrentChar.ToString();
                //لو لاقيت مسافة او سطر جديد
                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n')
                    continue;

                //لو لاقيت كومنت
                else if (CurrentChar == '/')//لو لاقيت سلاش
                {
                    bool is_comment = false;
                    //امشي خطوة
                    j++;
                    CurrentChar = SourceCode[j];

                    if (CurrentChar == '*')//لو لاقيت نجمة
                    {
                        //امشي خطوة تاني
                        j++;
                        CurrentChar = SourceCode[j];
                        while (true)
                        {
                            if (CurrentChar == '*')//لو لاقيت نجمة
                            {
                                //امشي خطوة
                                j++;
                                CurrentChar = SourceCode[j];
                                if (CurrentChar == '/')//لو لاقيت سلاش
                                {
                                    is_comment = true;//يبقي ده كومنت
                                    break;
                                }
                            }
                            //امشي خطوة
                            j++;
                            CurrentChar = SourceCode[j];
                        }
                    }
                    if (is_comment)
                    {
                        i = j;
                    }
                }

                //لو لاقيت كلمة من حرف او اكتر
                else if (CurrentChar >= 'A' && CurrentChar <= 'z') //if you read a character
                {
                    j = i + 1;
                    if (j < SourceCode.Length)
                    {
                        CurrentChar = SourceCode[j];

                        while ((CurrentChar >= 'A' && CurrentChar <= 'z') || CurrentChar >= '0' && CurrentChar <= '9')
                        {
                            CurrentLexeme = CurrentLexeme + CurrentChar.ToString();
                            j++;
                            CurrentChar = SourceCode[j];
                        }
                    }
                    FindTokenClass(CurrentLexeme);
                    i = j - 1;
                }

                //لو لاقيت رقم من خانة واحدة او اكتر
                else if (CurrentChar >= '0' && CurrentChar <= '9')
                {
                    j = i + 1;
                    //CurrentLexeme = CurrentLexeme + CurrentChar.ToString();
                    CurrentChar = SourceCode[j];

                    while ((CurrentChar >= '0' && CurrentChar <= '9') || CurrentChar.Equals('.'))
                    {
                        CurrentLexeme = CurrentLexeme + CurrentChar.ToString();

                        j++;
                        if (j < SourceCode.Length)
                            CurrentChar = SourceCode[j];
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
            Token_Class TC;
            Token Tok = new Token();
            Tok.lex = Lex;
            //Is it a reserved word?
            if (ReservedWords.ContainsKey(Lex))
            {
                TC = ReservedWords[Lex];
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }
            //Is it an identifier?
            else if (isIdentifier(Lex))
            {
                TC = Token_Class.Idenifier;
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }

            else if (Operators.ContainsKey(Lex))
            {
                TC = Operators[Lex];
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }

            //Is it a Constant?
            else if (isConstant(Lex))
            {
                TC = Token_Class.Constant;
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }
            //Is it an operator?
            //Is it an assign operator? (:=)

            else
            {
                Errors.Error_List.Add("Unidentified Token \'" + Lex + "\'");
            }

        }

        bool isIdentifier(string lex)
        {
            bool isValid = true;
            if (!(lex[0] >= 'A' && lex[0] <= 'z'))
            { isValid = false; }

            else
            {
                for (int i = 1; i < lex.Length; i++)
                {
                    if (!(lex[i] >= 'A' && lex[i] <= 'z')
                        || (lex[i] >= '0' && lex[i] <= '9'))
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

            for (int i = 0; i < lex.Length; i++)
            {
                if (!((lex[i] >= '0' && lex[i] <= '9') || lex[i] == '.'))
                {
                    isValid = false;
                }
            }
            return isValid;
        }
    }
}
