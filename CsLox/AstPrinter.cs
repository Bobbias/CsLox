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
        /// <param name="expr">The expressions to print.</param>
        /// <returns>A string representing the given expression.</returns>
        string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        /// <summary>
        /// Prints a binary expression.
        /// </summary>
        /// <param name="expr">The expressions to print.</param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.Oper.Lexeme, expr.Left, expr.Right);
        }

        /// <summary>
        /// Prints a grouping expression.
        /// </summary>
        /// <param name="expr">The expressions to print.</param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        /// <summary>
        /// Prints a literal value.
        /// </summary>
        /// <param name="expr">The expressions to print.</param>
        /// <returns>A string representing the given expression, or 'nil' if the object is <see langword="null"/> or <see cref="Object.ToString"/> fails.</returns>
        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null) return "nil";
            return expr.Value!.ToString() ?? "nil";
        }

        /// <summary>
        /// Prints unary expressions.
        /// </summary>
        /// <param name="expr">The expressions to print.</param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.Oper.Lexeme, expr.Right);
        }

        /// <summary>
        /// Prints variable expressions.
        /// </summary>
        /// <param name="expr">The expressions to print.</param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitVariableExpr(Expr.Variable expr)
        {
            return Parenthesize("var", expr);
        }

        /// <summary>
        /// Prints assignment expressions.
        /// </summary>
        /// <param name="expr">The expressions to print.</param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitAssignExpr(Expr.Assign expr)
        {
            return Parenthesize("assign", expr);
        }

        /// <summary>
        /// Prints logical expressions.
        /// </summary>
        /// <param name="expr">The expressions to print.</param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitLogicalExpr(Expr.Logical expr)
        {
            // Note: Consider expr.Oper.Type.ToString() instead of "logical"?
            return Parenthesize("logical", expr);
        }

        /// <summary>
        /// Prints call expressions.
        /// </summary>
        /// <param name="expr">The expressions to print.</param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitCallExpr(Expr.Call expr)
        {
            return $"{expr.Callee}()";
        }

        /// <summary>
        /// Prints get expressions.
        /// </summary>
        /// <param name="expr">The expressions to print.</param>
        /// <returns>A string representing the given expression.</returns>
        public string VisitGetExpr(Expr.Get expr)
        {
            // FIXME: Figure out if this gets the full object.property. If not, find out how to display that.
            return $"get {expr.Name.Lexeme}";
        }

        /// <summary>
        /// Prints a parenthesized expression.
        /// </summary>
        /// <param name="name">The name of the expression type.</param>
        /// <param name="exprs">The expressions to print.</param>
        /// <returns>A string representing the given expression.</returns>
        private string Parenthesize(string name, params Expr[] exprs)
        {
            var sb = new StringBuilder();

            sb.Append('(');
            sb.Append(name);
            foreach (var expr in exprs)
            {
                sb.Append(' ').Append(expr.Accept(this));
            }
            sb.Append(')');

            return sb.ToString();
        }

        /// <summary>
        /// Prints set expressions.
        /// </summary>
        /// <param name="expr">The expression to print.</param>
        /// <returns>A string representing the given expression.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string VisitSetExpr(Expr.Set expr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prints super expressions.
        /// </summary>
        /// <param name="expr">The expression to print.</param>
        /// <returns>A string representing the given expression.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string VisitSuperExpr(Expr.Super expr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prints this expressions.
        /// </summary>
        /// <param name="expr">The expression to print.</param>
        /// <returns>A string representing the given expression.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string VisitThisExpr(Expr.This expr)
        {
            throw new NotImplementedException();
        }
    }
}
