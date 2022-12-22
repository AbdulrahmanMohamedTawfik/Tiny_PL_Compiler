using System;
using System.Collections.Generic;
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
            root = new Node("Program");//1.	Program → Functions MainFunction
            //root.Children.Add(Functions());
            root.Children.Add(MainFunction());
            return root;
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

            return function;
        }


        Node Function_Body()//10.	FunBody → { Statements ReturnStatement ; }
        {
            //10.	FunBody → { [ReturnStatement ; | ε] }
            Node fun_body = new Node("Functions Body");

            fun_body.Children.Add(match(Token_Class.Lbrace));//{
            // fun_body.Children.Add(Statements());
            fun_body.Children.Add(ReturnSt());
            fun_body.Children.Add(match(Token_Class.Semicolon));
            fun_body.Children.Add(match(Token_Class.Rbrace));//}
            //fun_body.Children.Add(match(Token_Class.Rbrace));
            return fun_body;
        }

        Node ReturnSt()//21.	ReturnSt → return Expression
        {
            Node return_st = new Node("Return Statement");
            return_st.Children.Add(match(Token_Class.Return));
            //return_st.Children.Add(Expression());
            return return_st;
        }

        //Node Function()
        //{

        //}

        //Node Datatype()//Datatype → int | float | string
        //{
        //    Node data_type = new Node("Data Type");
        //    if (TokenStream[InputPointer].token_type == Token_Class.Int)
        //    {
        //        data_type.Children.Add(match(Token_Class.Int));
        //    }
        //    else if (TokenStream[InputPointer].token_type == Token_Class.Float)
        //    {
        //        data_type.Children.Add(match(Token_Class.Float));
        //    }
        //    else if (TokenStream[InputPointer].token_type == Token_Class.String)
        //    {
        //        data_type.Children.Add(match(Token_Class.String));
        //    }
        //    return data_type;
        //}

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
