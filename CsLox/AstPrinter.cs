using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    internal class AstPrinter : Expr.IVisitor<string>
    {
        public AstPrinter() { }

        string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.Oper.Lexeme, expr.Left, expr.Right);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            var ret = expr.Value.ToString();
            
            if (ret == null)
            {
                return "nil";
            }

            return ret;
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.Oper.Lexeme, expr.Right);
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            var sb = new StringBuilder();

            sb.Append("(");
            foreach (var expr in exprs)
            {
                sb.Append($" {expr.Accept(this)}");
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}
