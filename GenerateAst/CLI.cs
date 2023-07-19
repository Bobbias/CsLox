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
                "Call     : Expr callee, Token paren, List<Expr> args",
                "Get      : Expr obj, Token name",
                "Grouping : Expr expression",
                "Literal  : object? value",
                "Logical  : Expr left, Token oper, Expr right",
                "Unary    : Token oper, Expr right",
                "Variable : Token name"
            });

            file = new FileInfo("Stmt.cs");

            AstBuilder.DefineAst(file, new List<string> {
                "Block      : List<Stmt> stmts",
                "Class      : Token name, List<Stmt.Function> methods",
                "Expression : Expr expr",
                "Function   : Token name, List<Token> parameters, List<Stmt> body",
                "If         : Expr cond, Stmt thenBranch, Stmt? elseBranch",
                "Print      : Expr expr",
                "Return     : Token keyword, Expr value",
                "Var        : Token name, Expr initializer",
                "While      : Expr cond, Stmt body"
            });
        }
    }
}
