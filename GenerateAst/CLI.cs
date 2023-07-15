using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateAst
{
    internal static class CLI
    {
        public static void HandleMainCommand()
        {
            var file = new FileInfo("Expr.cs");

            AstBuilder.DefineAst(file, new List<string> {
                "Assign   : Token name, Expr value",
                "Binary   : Expr left, Token oper, Expr right",
                "Grouping : Expr expression",
                "Literal  : object? value",
                "Unary    : Token oper, Expr right",
                "Variable : Token name"
            });

            file = new FileInfo("Stmt.cs");

            AstBuilder.DefineAst(file, new List<string> {
                "Expression : Expr expr",
                "Print      : Expr expr",
                "Var        : Token name, Expr initializer"
            });
        }
    }
}
