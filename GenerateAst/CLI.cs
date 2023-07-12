using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateAst
{
    internal class CLI
    {
        public static void HandleMainCommand()
        {
            FileInfo file = new FileInfo("Expr.cs");

            AstBuilder.DefineAst(file, new List<string> {
                "Binary   : Expr left, Token oper, Expr right",
                "Grouping : Expr expression",
                "Literal  : object value",
                "Unary    : Token oper, Expr right"
            });
        }
    }
}
