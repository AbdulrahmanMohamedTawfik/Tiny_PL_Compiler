using System;
using System.Collections.Generic;
using System.Linq;
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

        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
        }
        Node Program()//Program → Function
        {
            Node program = new Node("Program");
            program.Children.Add(Function());
            return program;
        }
        Node Function()//Function → Fun_call Fun_Body 
        {

            Node function = new Node("Function");
            function.Children.Add(Function_Call());
            function.Children.Add(Function_Body());
            return function;
        }

        Node Function_Call()//Function_Call → Datatype FunctionName ( parameteres )
        {
            Node fun_call = new Node("Function Call");
            fun_call.Children.Add(Datatype());
            fun_call.Children.Add(FunctionName());
            fun_call.Children.Add(match(Token_Class.LParanthesis));
            //parameters separated by comma (,)
            fun_call.Children.Add(match(Token_Class.RParanthesis));

            return fun_call;
        }

        Node FunctionName()//FunctionName → identifier
        {
            Node fun_name = new Node("Function Name");
            if (TokenStream[InputPointer].token_type == Token_Class.Idenifier)
            {
                fun_name.Children.Add(match(Token_Class.Idenifier));
            }
            return fun_name;
        }

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
        Node Function_Body()//Function_Body → { statements return }
        {
            Node fun_body = new Node("Function Body");
            fun_body.Children.Add(match(Token_Class.Lbrace));
            //set of statements 
            fun_body.Children.Add(match(Token_Class.Return));
            fun_body.Children.Add(match(Token_Class.Rbrace));
            return fun_body;
        }
 //_______________________________________________________________________________________________

        public Node match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
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
