using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TinyCompiler;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();

        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public Node root;
        public bool state_again = false;
        public bool return_flag = false;
        public bool main_exist = false;
        // public bool normal_statement = false;
        /*
        -----
        Note:
        -----
        Lbrace → {
        Rbrace → }
        LParanthesis → (
        RParanthesis → )
        */
        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            bool is_main = false;
            root = new Node("Program");//1.	Program → Functions MainFunction | MainFunction | ε
            if (TokenStream.Count == 0)
            {
                Errors.Error_List.Add("empty code");
            }
            else
            {

                for (int i = 0; i < TokenStream.Count - 1; i++)
                {
                    if (TokenStream[i].token_type == Token_Class.Int_Token)
                    {
                        if (TokenStream[i + 1].token_type == Token_Class.Main)
                        {
                            main_exist = true;
                        }
                    }
                }
                if ((InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Int_Token) || (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Float_Token) || (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.String_Token))
                {
                    int tmp = InputPointer + 1;
                    if (tmp < TokenStream.Count)
                    {
                        if (TokenStream[tmp].token_type == Token_Class.Main)
                        {
                            is_main = true;
                        }
                    }
                }
                if (is_main)
                {
                    root.Children.Add(MainFunction());
                }
                else
                {
                    root.Children.Add(Functions());//not completed
                    return_flag = false;
                    root.Children.Add(MainFunction());
                }
                return root;
            }
            return null;
        }

        Node MainFunction()//2.	MainFunction → int Main () FunBody
        {
            Node mainfunction = new Node("MainFunction");
            mainfunction.Children.Add(match(Token_Class.Int_Token));
            mainfunction.Children.Add(match(Token_Class.Main));
            mainfunction.Children.Add(match(Token_Class.LParanthesis));
            mainfunction.Children.Add(match(Token_Class.RParanthesis));
            mainfunction.Children.Add(Function_Body());

            return mainfunction;
        }

        Node Functions()
        {
            //3.Functions → Functions Function | Function | ε
            //  1.Functions → Function Funcs
            //  2.Funcs → Function Funcs | ε

            Node function = new Node("Functions");
            function.Children.Add(Function());


            function.Children.Add(Funcs());
            return function;
        }

        Node Function()//4.	Function → FunDeclaration FunBody
        {
            Node function = new Node("Function");
            function.Children.Add(FunDeclaration());
            function.Children.Add(Function_Body());
            return_flag = false;
            return function;
        }

        Node FunDeclaration()//5. FunDeclaration → DataType FunName Parameter
        {
            Node fun_declare = new Node("Function Declaration");
            fun_declare.Children.Add(Datatype());
            fun_declare.Children.Add(match(Token_Class.Idenifier));//function name
            fun_declare.Children.Add(Parameter());
            return fun_declare;
        }

        private Node Parameter()
        {
            Node node = new Node("Parameter");
            node.Children.Add(match(Token_Class.LParanthesis));
            node.Children.Add(Parameters());
            node.Children.Add(match(Token_Class.RParanthesis));
            return node;
        }

        private Node Parameters()
        {
            //8.Parameters_Token → DataType identifier, Parameters_Token | DataType identifier | ε
            //    1.Parameters_Token → DataType identifier Parameters_
            //    2.Parameters_ → , Parameters_Token | ε
            Node node = new Node("Parameters_Token");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Int_Token)
            {
                node.Children.Add(Datatype());
                node.Children.Add(match(Token_Class.Idenifier));
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Float_Token)
            {
                node.Children.Add(Datatype());
                node.Children.Add(match(Token_Class.Idenifier));

            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.String_Token)
            {
                node.Children.Add(Datatype());
            }
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type != Token_Class.RParanthesis)
            {
                node.Children.Add(Parameters_());
            }
            return node;
        }

        private Node Parameters_()//    2.Parameters_ → , Parameters_Token | ε
        {
            Node node = new Node("Parameters_");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                node.Children.Add(match(Token_Class.Comma));
                node.Children.Add(Parameters());
                return node;
            }
            return null;
        }

        Node Funcs()//  2.Funcs → Function Functions | ε
        {
            Node funcs = new Node("Funcs");

            if (!main_exist && InputPointer >= TokenStream.Count)
            {
                Errors.Error_List.Add("Main not found");
                return null;
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Int_Token)
            {
                if (InputPointer + 1 < TokenStream.Count)
                {
                    if (TokenStream[InputPointer + 1].token_type == Token_Class.Main)
                    {
                        return null;
                    }
                }
            }

            funcs.Children.Add(Function());
            return_flag = false;
            funcs.Children.Add(Funcs());
            return funcs;

        }
        Node Function_Body()//10.	FunBody → { Statements ReturnStatement }
        {
            Node fun_body = new Node("Function Body");
            fun_body.Children.Add(match(Token_Class.Lbrace));//{

            fun_body.Children.Add(Statements());

            if (return_flag == false)
            {
                Errors.Error_List.Add("Each Function should return something");
            }

            fun_body.Children.Add(match(Token_Class.Rbrace));//}
            return fun_body;
        }



        private Node Statements()
        {
            //11.Statements → Statements ; Statement | Statement
            //    1.Statements → Statement State
            //    2.State → ; Statement State | ε
            Node node = new Node("Statements");
            node.Children.Add(Statement());
            node.Children.Add(State());
            return node;
        }

        private Node State()//    2.State → ; Statement State | ε
        {
            Node node = new Node("State");
            if (state_again)
            {
                state_again = false;
                node.Children.Add(Statement());
                node.Children.Add(State());

                return node;
            }

            return null;
        }

        private Node Statement()//12.	Statement → AssignmentSt | FunctionCall | DeclarationStatement | WriteSt | ReadSt | ReturnSt | Repeat_Token | IfStatement | ε
        {
            Node node = new Node("Statement");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type != Token_Class.Rbrace)
            {
                node.Children.Add(AssignmentSt());

                node.Children.Add(FunctionCall());

                node.Children.Add(DeclarationStatement());

                node.Children.Add(WriteSt());

                node.Children.Add(ReadSt());

                node.Children.Add(ReturnSt());

                node.Children.Add(Repeat());

                node.Children.Add(IfStatement());

                return node;
            }
            return null;
        }

        Node ReturnSt()//21.	ReturnSt → return Expression 
        {
            Node return_st = new Node("Return_Token Statement");
            if ((InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Return_Token))//&&(TokenStream[InputPointer-1].token_type != Token_Class.Lbrace))
            {
                return_st.Children.Add(match(Token_Class.Return_Token));
                return_st.Children.Add(Expressions());
                return_st.Children.Add(match(Token_Class.Semicolon));
                return_flag = true;
                return return_st;
            }
            return null;
        }

        Node Expressions()
        {
            //14.Expression → Expression AddOp Term | Term | string
            //1.Expression → Term Exp
            //2.Exp → AddOp Term Exp | ε

            Node expression = new Node("Expression");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Semicolon)
            {
                Errors.Error_List.Add("Expression value Not Found");
                return null;
            }
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.String_Token)
            {
                expression.Children.Add(match(Token_Class.String_Token));
                return expression;
            }
            expression.Children.Add(Term());
            expression.Children.Add(Exp());
            return expression;
        }

        Node Term()
        {
            //15.	Term → Term MultOp | Factor
            //1.Term → Factor Ter
            //2.Ter → MultOp Factor Ter | ε

            Node term = new Node("Term");
            term.Children.Add(Factor());
            term.Children.Add(Ter());

            return term;
        }

        Node Ter()//2.Ter → MultOp Factor Ter | ε
        {
            Node ter = new Node("Ter");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.MultiplyOp || InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.DivideOp)
            {
                ter.Children.Add(MultOp());
                ter.Children.Add(Factor());
                ter.Children.Add(Ter());
                return ter;
            }
            return null;
        }

        Node AddOp()//15.	AddOp → + | - 
        {
            Node op = new Node("Operators");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.PlusOp)
            {
                op.Children.Add(match(Token_Class.PlusOp));
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.MinusOp)
            {
                op.Children.Add(match(Token_Class.MinusOp));
            }
            return op;
        }
        Node MultOp()//16.	MultOp → * | /
        {
            Node op = new Node("Operators");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.MultiplyOp)
            {
                op.Children.Add(match(Token_Class.MultiplyOp));
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.DivideOp)
            {
                op.Children.Add(match(Token_Class.DivideOp));
            }
            return op;
        }

        Node Factor()//17.	Factor → Equation | identifier | constant  | FunctionCall
        {
            Node factor = new Node("Factor");

            if (InputPointer < TokenStream.Count && InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LParanthesis)
            {
                factor.Children.Add(Equation());
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Idenifier)
            {
                if (InputPointer + 1 < TokenStream.Count)
                {
                    if (TokenStream[InputPointer + 1].token_type == Token_Class.LParanthesis)
                    {
                        factor.Children.Add(FunctionCall());
                    }
                    else
                        factor.Children.Add(match(Token_Class.Idenifier));
                }
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Constant)
            {
                factor.Children.Add(match(Token_Class.Constant));
            }
            else
            {
                Errors.Error_List.Add("Error in Returned value");
                return null;
            }

            return factor;
        }

        Node Exp()//2.Exp → AddOp Term Exp | ε
        {
            Node exp = new Node("Exp");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.PlusOp || InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.MinusOp)
            {
                exp.Children.Add(AddOp());
                exp.Children.Add(Term());
                exp.Children.Add(Exp());
                return exp;
            }
            return null;
        }

        Node Datatype()//Datatype → int | float | string
        {
            Node data_type = new Node("Data Type");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Int_Token)
            {
                data_type.Children.Add(match(Token_Class.Int_Token));
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Float_Token)
            {
                data_type.Children.Add(match(Token_Class.Float_Token));
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.String_Token)
            {
                data_type.Children.Add(match(Token_Class.String_Token));
            }
            return data_type;
        }

        Node Repeat()//30.	Repeat_Token → repeat Statements until ConditionSt
        {
            Node repeat_declare = new Node("Repeat_Token");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Repeat_Token)
            {
                repeat_declare.Children.Add(match(Token_Class.Repeat_Token));
                repeat_declare.Children.Add(Statements());
                if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Until_Token)
                {
                    repeat_declare.Children.Add(match(Token_Class.Until_Token));
                    state_again = true;
                    return_flag = false;
                }
                else
                {
                    Errors.Error_List.Add("No 'until' found");
                    return null;
                }
                repeat_declare.Children.Add(ConditionSt());
                return repeat_declare;
            }
            return null;
        }
        Node Equation()
        {
            Node equation = new Node("Equation");
            equation.Children.Add(match(Token_Class.LParanthesis));
            equation.Children.Add(Expressions());
            equation.Children.Add(match(Token_Class.RParanthesis));
            return equation;
        }
        Node FunctionCall()//31.	FunctionCall → identifier ArgList
        {
            Node FunctionCall_declare = new Node("Function Call");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Idenifier)
            {
                if (InputPointer + 1 < TokenStream.Count)
                {
                    if (TokenStream[InputPointer + 1].token_type == Token_Class.LParanthesis)
                    {
                        FunctionCall_declare.Children.Add(match(Token_Class.Idenifier));//function name
                        FunctionCall_declare.Children.Add(ArgList());

                        return FunctionCall_declare;
                    }
                }
            }
            return null;
        }

        Node ArgList()//32.	ArgList → ( Arguments) | ε
        {
            Node ArgList_declare = new Node("ArgList ");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LParanthesis)
            {
                ArgList_declare.Children.Add(match(Token_Class.LParanthesis));
                ArgList_declare.Children.Add(Arguments());
                ArgList_declare.Children.Add(match(Token_Class.RParanthesis));
                return ArgList_declare;
            }
            else
            {
                return null;
            }

        }

        Node Arguments()
        {
            //33.Arguments → Arguments , identifier | identifier
            //    1.Arguments → identifier Arg
            //    2.Arg → , identifier Arg | ε

            Node Arguments_declare = new Node("Arguments ");
            Arguments_declare.Children.Add(match(Token_Class.Idenifier));
            Arguments_declare.Children.Add(Arg());

            return Arguments_declare;
        }

        Node Arg()//2.Arg → , identifier Arg | ε
        {
            Node Arg_declare = new Node("Arg");
            int temp = InputPointer;
            if (TokenStream[temp].token_type == Token_Class.Comma)
            {
                Arg_declare.Children.Add(match(Token_Class.Comma));
                Arg_declare.Children.Add(match(Token_Class.Idenifier));
                if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
                {
                    Arg_declare.Children.Add(Arg());
                }
            }
            else
            {
                return null;
            }
            return Arg_declare;
        }

        Node ConditionSt()//1.	ConditionSt → Condition ConditionSt_ 
        {
            //21.ConditionSt → Condition BoolOp ConditionSt | Condition
            //    1.ConditionSt → Condition ConditionSt_
            //    2.ConditionSt_ → BoolOp ConditionSt | ε


            Node conditionSt = new Node("ConditionSt");
            conditionSt.Children.Add(Condition());
            conditionSt.Children.Add(ConditionSt_());
            return conditionSt;
        }
        Node ConditionSt_()//    2.ConditionSt_ → BoolOp ConditionSt | ε
        {
            Node conditionSt_ = new Node("ConditionSt_");
            if ((InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.AndOp) || (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.OrOp))
            {
                conditionSt_.Children.Add(BoolOp());
                conditionSt_.Children.Add(ConditionSt());
                return conditionSt_;
            }
            return null;
        }

        Node Condition()//25.	Condition → identifier ConditionOp Term
        {
            Node condition = new Node("Condition");
            condition.Children.Add(match(Token_Class.Idenifier));
            condition.Children.Add(ConditionOp());
            condition.Children.Add(Term());
            return condition;
        }
        Node ConditionOp()//26.	ConditionOp → < | > | = | <>  
        {
            Node conditionOp = new Node("ConditionOp");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.IsEqualOp)
            {
                conditionOp.Children.Add(match(Token_Class.IsEqualOp));
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.NotEqualOp)
            {
                conditionOp.Children.Add(match(Token_Class.NotEqualOp));
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LessThanOp)
            {
                conditionOp.Children.Add(match(Token_Class.LessThanOp));
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.GreaterThanOp)
            {
                conditionOp.Children.Add(match(Token_Class.GreaterThanOp));
            }
            else
                Errors.Error_List.Add("Invalid Conditional Operator (it should be '<', '>', '=' or '<>')");

            return conditionOp;
        }
        Node BoolOp()   //24.	BoolOp → && | ||
        {
            Node boolOp = new Node("BoolOp");

            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.OrOp)
            {
                boolOp.Children.Add(match(Token_Class.OrOp));
            }
            else
            {
                boolOp.Children.Add(match(Token_Class.AndOp));
            }
            return boolOp;
        }
        Node IfStatement() //27.	IfStatement → if ConditionSt then Statements ElseIfStatement  ElseStatement end
        {
            Node ifStatement = new Node("IfStatement");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.If_Token)
            {
                ifStatement.Children.Add(match(Token_Class.If_Token));
                ifStatement.Children.Add(ConditionSt());
                ifStatement.Children.Add(match(Token_Class.Then_Token));
                ifStatement.Children.Add(Statements());
                ifStatement.Children.Add(ElseIfStatement());
                ifStatement.Children.Add(ElseStatement());
                if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.End_Token)
                {
                    ifStatement.Children.Add(match(Token_Class.End_Token));
                    state_again = true;
                    return_flag = false;
                }
                else
                {
                    state_again = true;
                    Errors.Error_List.Add("End_Token not found");
                    return null;
                }
                return ifStatement;
            }
            return null;

        }
        Node ElseStatement() //29.	ElseStatement → else Statements end
        {
            Node elseStatement = new Node("ElseStatement");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type != Token_Class.Else_Token)
            {
                return null;
            }
            elseStatement.Children.Add(match(Token_Class.Else_Token));
            elseStatement.Children.Add(Statements());
            return elseStatement;

        }
        Node ElseIfStatement() //28.	ElseIfStatement → elseif ConditionSt then Statements ElseIfStatement  ElseStatement end | ε
        {
            Node elseIfStatement = new Node("ElseIfStatement");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type != Token_Class.Elseif_Token)
            {
                return null;
            }
            elseIfStatement.Children.Add(match(Token_Class.Elseif_Token));
            elseIfStatement.Children.Add(ConditionSt());
            elseIfStatement.Children.Add(match(Token_Class.Then_Token));
            elseIfStatement.Children.Add(Statements());
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Elseif_Token)
            {
                elseIfStatement.Children.Add(ElseIfStatement());
            }
            return elseIfStatement;

        }

        private Node ReadSt()//ReadSt → read identifier
        {
            Node node = new Node("ReadSt");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Read_Token)
            {
                node.Children.Add(match(Token_Class.Read_Token));
                node.Children.Add(match((Token_Class.Idenifier)));
                node.Children.Add(match((Token_Class.Semicolon)));
                state_again = true;
                return node;
            }
            return null;
        }

        private Node WriteSt()
        {
            //WriteSt → write Expression | write endl
            Node node = new Node("WriteSt");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Write_Token)
            {
                node.Children.Add(match(Token_Class.Write_Token));
                if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Endl_Token)
                {
                    node.Children.Add(match(Token_Class.Endl_Token));
                }
                else
                    node.Children.Add(Expressions());
                node.Children.Add(match(Token_Class.Semicolon));
                state_again = true;
                return node;
            }
            return null;
        }

        private Node DeclarationStatement()//19.	DeclarationStatement → DataType DeclarationSt ;
        {
            Node node = new Node("Declaration Statement");
            if ((InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Int_Token) || (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Float_Token) || (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.String_Token))
            {
                node.Children.Add(Datatype());
                node.Children.Add(DeclarationSt());
                node.Children.Add(match(Token_Class.Semicolon));
                state_again = true;

                return node;
            }
            //Errors.Error_List.Add("missing data type for declaration statement");
            return null;
        }

        private Node DeclarationSt()
        {
            //20.DeclarationSt → DeclarationS , DeclarationS | DeclarationS
            //    1.DeclarationSt → DeclarationS Dec
            //    2.Dec → , DeclarationS Dec | ε

            Node node = new Node("DeclarationSt");
            node.Children.Add(DeclarationS());
            node.Children.Add(Dec());
            return node;

        }

        private Node Dec()//    2.Dec → , DeclarationS Dec | ε
        {
            Node node = new Node("Dec");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                node.Children.Add(match(Token_Class.Comma));
                node.Children.Add(DeclarationS());
                node.Children.Add(Dec());
                return node;
            }
            return null;

        }

        private Node DeclarationS()
        {
            //21.DeclarationS → identifier := Expressions | identifier
            //    1.DeclarationS → identifier DeclarationS’
            //    2.DeclarationS’ → := Expressions | ε

            Node node = new Node("DeclarationS");
            node.Children.Add(match(Token_Class.Idenifier));
            node.Children.Add(DeclarationS_());

            return node;


        }

        private Node DeclarationS_() //    2.DeclarationS’ → := Expressions | ε
        {
            Node node = new Node("DeclarationStDash");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.EqualOp)
            {
                node.Children.Add(match(Token_Class.EqualOp));
                node.Children.Add(Expressions());
                return node;
            }
            return null;

        }

        private Node AssignmentSt()
        {
            //AssignmentSt → identifier := Expressions ;
            Node node = new Node("AssignmentSt");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Idenifier)
            {
                int tmp = InputPointer + 1;
                if (tmp < TokenStream.Count)
                {
                    if (TokenStream[tmp].token_type == Token_Class.EqualOp)
                    {
                        node.Children.Add(match(Token_Class.Idenifier));
                        node.Children.Add(match(Token_Class.EqualOp));
                        node.Children.Add(Expressions());
                        node.Children.Add(match(Token_Class.Semicolon));
                        state_again = true;

                        return node;
                    }
                    else
                    {
                        Errors.Error_List.Add("Assignment operator not found");
                    }
                }
            }

            return null;

        }

        //_______________________________________________________________________________________________

        public Node match(Token_Class ExpectedToken)
        {
            Console.WriteLine(InputPointer);
            //if there is more tokens i will continue else i will raise an error
            if (InputPointer < TokenStream.Count)
            {
                //if the token is as i expected(the passed parameter) then i will add it to tree else i will raise an error
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());
                    return newNode;
                }

                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected " + ExpectedToken.ToString() + " and " + TokenStream[InputPointer].token_type.ToString() + "  found\r\n");
                    InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected " + ExpectedToken.ToString() + "\r\n");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
