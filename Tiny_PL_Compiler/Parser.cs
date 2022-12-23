using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            root = new Node("Program");//1.	Program → Functions MainFunction | MainFunction
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
                //root.Children.Add(Functions());//not completed
                root.Children.Add(MainFunction());
            }
            return root;
        }

        Node MainFunction()//2.	MainFunction → int Main () FunBody
        {
            Node mainfunction = new Node("MainFunction");
            mainfunction.Children.Add(match(Token_Class.Int));
            mainfunction.Children.Add(match(Token_Class.Main));
            mainfunction.Children.Add(match(Token_Class.LParanthesis));
            mainfunction.Children.Add(match(Token_Class.RParanthesis));
            //mainfunction.Children.Add(Function_Body());

            return mainfunction;
        }

        Node Functions()
        {
            //3.Functions → Functions Function | Function | ε
            //  1.Functions → Function Funcs
            //  2.Funcs → Function Functions | ε

            Node function = new Node("Functions");

            function.Children.Add(Function());
            function.Children.Add(Funcs());
            return function;
        }

        Node Funcs()//  2.Funcs → Function Functions | ε
        {
            Node funcs = new Node("Funcs");
            funcs.Children.Add(Function());
            funcs.Children.Add(Functions());
            return funcs;
        }

        Node Function()//4.	Function → FunDeclaration FunBody
        {
            Node function = new Node("Function");
            function.Children.Add(FunDeclaration());
            function.Children.Add(Function_Body());
            return function;
        }

        Node FunDeclaration()//5. FunDeclaration → DataType FunName Parameter
        {
            Node fun_declare = new Node("Function Declaration");
            fun_declare.Children.Add(Datatype());
            fun_declare.Children.Add(FunName());
            //fun_declare.Children.Add(Parameter());
            return fun_declare;
        }
        Node FunName()//7.	FunName → identifier
        {
            Node fun_declare = new Node("Function Declaration");
            fun_declare.Children.Add(match(Token_Class.Idenifier));
            return fun_declare;
        }

        Node Function_Body()//10.	FunBody → { Statements ReturnStatement }
        {
            Node fun_body = new Node("Function Body");

            fun_body.Children.Add(match(Token_Class.Lbrace));//{
            // fun_body.Children.Add(Statements());
            fun_body.Children.Add(ReturnSt());
            
            fun_body.Children.Add(match(Token_Class.Rbrace));//}
            //fun_body.Children.Add(match(Token_Class.Rbrace));
            return fun_body;
        }

        Node ReturnSt()//21.	ReturnSt → return Expression ;
        {
            Node return_st = new Node("Return Statement");
            return_st.Children.Add(match(Token_Class.Return));
            return_st.Children.Add(Expressions());
            return_st.Children.Add(match(Token_Class.Semicolon));
            return return_st;
        }

        Node Expressions()
        {
            //14.Expressions → Expressions Operators Term | Term
                //1.Expressions → Term Exp
                //2.Exp → Operators Term Exp | ε
            Node expression = new Node("Expression");
            expression.Children.Add(Term());
            return expression;
        }

        Node Term()
        {
            //16.Term  → Term Operators Factor | Factor
                //1.Term → Factor Ter
                //2.Ter → Operators Factor Ter | ε

            Node term = new Node("Term");
            term.Children.Add(Factor());
            term.Children.Add(Ter());

            return term;
        }

        Node Ter()//2.Ter → Operators Factor Ter | ε
        {
            Node ter = new Node("Ter");
            ter.Children.Add(Operators());
            ter.Children.Add(Factor());
            ter.Children.Add(Ter());

            return ter;
        }

        Node Operators()//15.	Operators → + | - | * | /
        {
            Node op = new Node("Operators");
            //op.Children.Add();
            return op;
        }

        Node Factor()//17.	Factor → identifier | constant
        {
            Node factor = new Node("Factor");
            //factor.Children.Add();
            return factor;
        }

        Node Exp()//2.Exp → Operators Term Exp | ε
        {

            Node exp = new Node("Exp");
            exp.Children.Add(Term());
            exp.Children.Add(Exp());

            return exp;
        }

        //Node Function()
        //{

        //}

        Node Datatype()//Datatype → int | float | string
        {
            Node data_type = new Node("Data Type");
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
