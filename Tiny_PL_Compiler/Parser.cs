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

                if ((TokenStream[InputPointer].token_type == Token_Class.Int) || (TokenStream[InputPointer].token_type == Token_Class.Float) || (TokenStream[InputPointer].token_type == Token_Class.String))
                {
                    int tmp = InputPointer + 1;
                    if (tmp < TokenStream.Count)
                    {
                        if (TokenStream[tmp].token_type == Token_Class.Main)
                        {
                            is_main = true;
                            Console.WriteLine("is main");
                        }
                    }
                }
                if (is_main)
                {
                    root.Children.Add(MainFunction());
                }
                else
                {
                    Console.WriteLine("is not main");
                    root.Children.Add(Functions());//not completed
                    root.Children.Add(MainFunction());
                }
                return root;
            }
            return null;
        }

        Node MainFunction()//2.	MainFunction → int Main () FunBody
        {
            Node mainfunction = new Node("MainFunction");
            mainfunction.Children.Add(match(Token_Class.Int));
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
            //  2.Funcs → Function Functions | ε

            Node function = new Node("Functions");
            function.Children.Add(Function());
            //if (TokenStream[InputPointer].token_type == Token_Class.Int)
            //{
            //    if (InputPointer + 1 < TokenStream.Count)
            //    {
            //        if (TokenStream[InputPointer + 1].token_type == Token_Class.Main)
            //        {
            //            Console.WriteLine("stop");
            //            return null;
            //        }
            //    }
            //}
            function.Children.Add(Funcs());
            return function;
        }



        Node Function()//4.	Function → FunDeclaration FunBody
        {
            Node function = new Node("Function");
            Console.WriteLine("2");
            function.Children.Add(FunDeclaration());
            function.Children.Add(Function_Body());
            return function;
        }

        Node FunDeclaration()//5. FunDeclaration → DataType FunName Parameter
        {
            Node fun_declare = new Node("Function Declaration");
            Console.WriteLine("3");
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
            //8.Parameters → DataType identifier, Parameters | DataType identifier | ε
            //    1.Parameters → DataType identifier Parameters_
            //    2.Parameters_ → , Parameters | ε
            Node node = new Node("Parameters");

            Console.WriteLine("aa");
            node.Children.Add(Datatype());
            Console.WriteLine("bb");
            node.Children.Add(match(Token_Class.Idenifier));
            Console.WriteLine("cc");
            if (TokenStream[InputPointer].token_type != Token_Class.RParanthesis)
            {
                Console.WriteLine("dd");
                node.Children.Add(Parameters_());
            }
            return node;
        }

        private Node Parameters_()//    2.Parameters_ → , Parameters | ε
        {
            Node node = new Node("Parameters_");
            if (TokenStream[InputPointer].token_type == Token_Class.Comma)
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
            Console.WriteLine("again1");
            Console.WriteLine(TokenStream[InputPointer].token_type);
            if (TokenStream[InputPointer].token_type == Token_Class.Int)
            {
                if (InputPointer + 1 < TokenStream.Count)
                {
                    if (TokenStream[InputPointer + 1].token_type == Token_Class.Main)
                    {
                        Console.WriteLine("stop");
                        return null;
                    }
                }
            }
            Console.WriteLine("again2");
            funcs.Children.Add(Function());
            funcs.Children.Add(Functions());
            return funcs;

        }
        Node Function_Body()//10.	FunBody → { Statements ReturnStatement }
        {
            Node fun_body = new Node("Function Body");
            //Console.WriteLine("6");
            fun_body.Children.Add(match(Token_Class.Lbrace));//{

            fun_body.Children.Add(Statements());

            // fun_body.Children.Add(LastReturnSt());

            fun_body.Children.Add(match(Token_Class.Rbrace));//}
            return fun_body;
        }

        private Node LastReturnSt()//21.	LastReturnSt → return Expression ;
        {
            //return    return;     return 2

            Node return_st = new Node("Last Return Statement");
            //Console.WriteLine("7");
            if ((TokenStream[InputPointer].token_type == Token_Class.Return) && (TokenStream[InputPointer - 1].token_type != Token_Class.Lbrace))
            {
                return_st.Children.Add(match(Token_Class.Return));
                return_st.Children.Add(Expressions());
                return_st.Children.Add(match(Token_Class.Semicolon));
                return return_st;
            }
            else
            {
                Errors.Error_List.Add("Function should return something");
                return null;
            }
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
            if ((TokenStream[InputPointer].token_type == Token_Class.Semicolon))
            {
                node.Children.Add(match(Token_Class.Semicolon));
                state_again = true;
            }
            if (state_again)
            {
                Console.WriteLine("a");
                state_again = false;
                node.Children.Add(Statement());
                node.Children.Add(State());

                return node;
            }
            return null;
        }

        private Node Statement()//12.	Statement → AssignmentSt | DeclarationStatement | WriteSt | ReadSt | ReturnSt | IfStatement | Repeat | FunctionCall | ε
        {
            Node node = new Node("Statement");
            //while(InputPointer<TokenStream.Count)
            if (TokenStream[InputPointer].token_type != Token_Class.Rbrace)
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
            //return    return;     return 2

            Node return_st = new Node("Return Statement");
            if ((TokenStream[InputPointer].token_type == Token_Class.Return))//&&(TokenStream[InputPointer-1].token_type != Token_Class.Lbrace))
            {
                //Console.WriteLine("7");
                return_st.Children.Add(match(Token_Class.Return));
                return_st.Children.Add(Expressions());

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
            if (TokenStream[InputPointer].token_type == Token_Class.Semicolon)
            {
                Errors.Error_List.Add("Expression value Not Found");
                return null;
            }
            if (TokenStream[InputPointer].token_type == Token_Class.String)
            {
                expression.Children.Add(match(Token_Class.String));
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
            if (TokenStream[InputPointer].token_type == Token_Class.MultiplyOp || TokenStream[InputPointer].token_type == Token_Class.DivideOp)
            {
                //return 1+2*5;
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
            //op.Children.Add();
            if (TokenStream[InputPointer].token_type == Token_Class.PlusOp)
            {
                op.Children.Add(match(Token_Class.PlusOp));
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.MinusOp)
            {
                op.Children.Add(match(Token_Class.MinusOp));
            }
            return op;
        }
        Node MultOp()//16.	MultOp → * | /
        {
            Node op = new Node("Operators");
            //op.Children.Add();
            if (TokenStream[InputPointer].token_type == Token_Class.MultiplyOp)
            {
                op.Children.Add(match(Token_Class.MultiplyOp));
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.DivideOp)
            {
                op.Children.Add(match(Token_Class.DivideOp));
            }
            return op;
        }

        Node Factor()//17.	Factor → identifier | constant | FunctionCall
        {
            Node factor = new Node("Factor");
            //factor.Children.Add(FunctionCall());
            //if (TokenStream[InputPointer].token_type == Token_Class.Idenifier)
            //{
            //    if (InputPointer + 1 < TokenStream.Count)
            //    {
            //        if (TokenStream[InputPointer + 1].token_type == Token_Class.LParanthesis)
            //        {
            //            factor.Children.Add(FunctionCall());
            //        }
            //    }
            //}
            if (TokenStream[InputPointer].token_type == Token_Class.Idenifier)
            {
                factor.Children.Add(match(Token_Class.Idenifier));
                //return factor;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Constant)
            {
                factor.Children.Add(match(Token_Class.Constant));
                //return factor;
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
            if (TokenStream[InputPointer].token_type == Token_Class.PlusOp || TokenStream[InputPointer].token_type == Token_Class.MinusOp)
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
            //Console.WriteLine("4");
            if (TokenStream[InputPointer].token_type == Token_Class.Int)
            {
                data_type.Children.Add(match(Token_Class.Int));
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Float)
            {
                data_type.Children.Add(match(Token_Class.Float));
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.String)
            {
                data_type.Children.Add(match(Token_Class.String));
            }
            return data_type;
        }

        Node Repeat()//30.	Repeat → repeat Statements until ConditionSt
        {
            Node repeat_declare = new Node("Repeat");
            if (TokenStream[InputPointer].token_type == Token_Class.Repeat)
            {
                repeat_declare.Children.Add(match(Token_Class.Repeat));
                repeat_declare.Children.Add(Statements());
                if (TokenStream[InputPointer].token_type == Token_Class.Until)
                {
                    repeat_declare.Children.Add(match(Token_Class.Until));
                    state_again = true;
                }
                repeat_declare.Children.Add(ConditionSt());
                return repeat_declare;
            }
            return null;
        }

        Node FunctionCall()//31.	FunctionCall → identifier ArgList
        {
            Node FunctionCall_declare = new Node("Function Call");
            if (TokenStream[InputPointer].token_type == Token_Class.Idenifier)
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

            if (TokenStream[InputPointer].token_type == Token_Class.LParanthesis)
            {
                ArgList_declare.Children.Add(match(Token_Class.LParanthesis));
                ArgList_declare.Children.Add(Arguments());
                ArgList_declare.Children.Add(match(Token_Class.RParanthesis));
                //ArgList_declare.Children.Add(match(Token_Class.Semicolon));
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
                if (TokenStream[InputPointer].token_type == Token_Class.Comma)
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

        //****************************************************************

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
            if ((TokenStream[InputPointer].token_type == Token_Class.AndOp) || (TokenStream[InputPointer].token_type == Token_Class.OrOp))
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
            if (TokenStream[InputPointer].token_type == Token_Class.IsEqualOp)
            {
                conditionOp.Children.Add(match(Token_Class.IsEqualOp));
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.NotEqualOp)
            {
                conditionOp.Children.Add(match(Token_Class.NotEqualOp));
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.LessThanOp)
            {
                conditionOp.Children.Add(match(Token_Class.LessThanOp));
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.GreaterThanOp)
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

            if (TokenStream[InputPointer].token_type == Token_Class.OrOp)
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
            if (TokenStream[InputPointer].token_type == Token_Class.If)
            {
                ifStatement.Children.Add(match(Token_Class.If));
                ifStatement.Children.Add(ConditionSt());
                ifStatement.Children.Add(match(Token_Class.Then));
                ifStatement.Children.Add(Statements());
                ifStatement.Children.Add(ElseIfStatement());
                ifStatement.Children.Add(ElseStatement());
                if (TokenStream[InputPointer].token_type == Token_Class.End)
                {
                    ifStatement.Children.Add(match(Token_Class.End));
                    state_again = true;
                }
                //else
                //{
                //    Errors.Error_List.Add("End not found");
                //    return null;
                //}
                return ifStatement;
            }
            return null;

        }
        Node ElseStatement() //29.	ElseStatement → else Statements end
        {
            Node elseStatement = new Node("ElseStatement");
            if (TokenStream[InputPointer].token_type != Token_Class.Else)
            {
                return null;
            }
            elseStatement.Children.Add(match(Token_Class.Else));
            elseStatement.Children.Add(Statements());

            //elseStatement.Children.Add(match(Token_Class.End));
            return elseStatement;

        }
        Node ElseIfStatement() //28.	ElseIfStatement → elseif ConditionSt then Statements ElseIfStatement  ElseStatement end | ε
        {
            Node elseIfStatement = new Node("ElseIfStatement");
            if (TokenStream[InputPointer].token_type != Token_Class.Elseif)
            {
                return null;
            }
            elseIfStatement.Children.Add(match(Token_Class.Elseif));
            elseIfStatement.Children.Add(ConditionSt());
            elseIfStatement.Children.Add(match(Token_Class.Then));
            elseIfStatement.Children.Add(Statements());
            if (TokenStream[InputPointer].token_type == Token_Class.Elseif)
            {
                elseIfStatement.Children.Add(ElseIfStatement());
            }

            //elseIfStatement.Children.Add(ElseStatement());


            //elseIfStatement.Children.Add(match(Token_Class.End));
            return elseIfStatement;

        }
        //****************************************************************

        private Node ReadSt()
        {
            //ReadSt → read identifier
            Node node = new Node("ReadSt");
            if (TokenStream[InputPointer].token_type == Token_Class.Read)
            {
                node.Children.Add(match(Token_Class.Read));
                node.Children.Add(match((Token_Class.Idenifier)));
                //node.Children.Add(match((Token_Class.Semicolon)));
                return node;
            }
            return null;
        }

        private Node WriteSt()
        {
            //WriteSt → write Expression endl
            //WriteSt → write [endl | Expression] ;
            Node node = new Node("WriteSt");
            if (TokenStream[InputPointer].token_type == Token_Class.Write)
            {
                node.Children.Add(match(Token_Class.Write));
                if (TokenStream[InputPointer].token_type == Token_Class.Endl)
                {
                    node.Children.Add(match(Token_Class.Endl));
                }
                else
                    node.Children.Add(Expressions());
                //node.Children.Add(match(Token_Class.Semicolon));
                return node;
            }
            return null;
        }

        private Node DeclarationStatement()//19.	DeclarationStatement → DataType DeclarationSt ;
        {
            Node node = new Node("Declaration Statement");
            if ((TokenStream[InputPointer].token_type == Token_Class.Int) || (TokenStream[InputPointer].token_type == Token_Class.Float) || (TokenStream[InputPointer].token_type == Token_Class.String))
            {
                node.Children.Add(Datatype());
                node.Children.Add(DeclarationSt());
                //node.Children.Add(match(Token_Class.Semicolon));
                return node;
            }
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
            if (TokenStream[InputPointer].token_type == Token_Class.Comma)
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
            if (TokenStream[InputPointer].token_type == Token_Class.EqualOp)
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
            if (TokenStream[InputPointer].token_type == Token_Class.Idenifier)
            {
                int tmp = InputPointer + 1;
                if (tmp < TokenStream.Count)
                {
                    if (TokenStream[tmp].token_type == Token_Class.EqualOp)
                    {
                        node.Children.Add(match(Token_Class.Idenifier));
                        node.Children.Add(match(Token_Class.EqualOp));
                        node.Children.Add(Expressions());
                        //node.Children.Add(match(Token_Class.Semicolon));
                        return node;
                    }
                }
            }
            return null;

        }

        //_______________________________________________________________________________________________

        public Node match(Token_Class ExpectedToken)
        {
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
