using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// A basic PrettyPrinter. Implements the <see cref="Expr.IVisitor{T}"/> interface.
    /// </summary>
    internal class AstPrinter : Expr.IVisitor<string>
    {
        public AstPrinter() { }

        /// <summary>
        /// Prints an expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns>A string representing the given expression.</returns>
        string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        /// <summary>
        /// Prints a binary expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.Oper.Lexeme, expr.Left, expr.Right);
        }

        /// <summary>
        /// Prints a grouping expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        /// <summary>
        /// Prints a literal value.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns>A string representing the given expression, or 'nil' if the object is <see langword="null"/> or <see cref="Object.ToString"/> fails.</returns>
        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null) return "nil";
            return expr.Value!.ToString() ?? "nil";
        }

        /// <summary>
        /// Prints a unary expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.Oper.Lexeme, expr.Right);
        }

        /// <summary>
        /// Prints a variable expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitVariableExpr(Expr.Variable expr)
        {
            return Parenthesize("var", expr);
        }

        /// <summary>
        /// Prints an assignment expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitAssignExpr(Expr.Assign expr)
        {
            return Parenthesize("assign", expr);
        }

        /// <summary>
        /// Prints a logical expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitLogicalExpr(Expr.Logical expr)
        {
            // Note: Consider expr.Oper.Type.ToString() instead of "logical"?
            return Parenthesize("logical", expr);
        }

        /// <summary>
        /// Prints a parenthesized expression.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="exprs"></param>
        /// <returns>A string representing the given expression.</returns>
        private string Parenthesize(string name, params Expr[] exprs)
        {
            var sb = new StringBuilder();

            sb.Append("(");
            sb.Append(name);
            foreach (var expr in exprs)
            {
                sb.Append($" {expr.Accept(this)}");
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}
