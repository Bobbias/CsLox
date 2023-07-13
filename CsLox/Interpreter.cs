using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    internal class Interpreter : Expr.IVisitor<object>
    {
        public object VisitBinaryExpr(Expr.Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Oper.Type)
            {
                case Token.TokenType.GREATER:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left > (double)right;
                case Token.TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left >= (double)right;
                case Token.TokenType.LESS:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left < (double)right;
                case Token.TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left <= (double)right;
                case Token.TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case Token.TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case Token.TokenType.MINUS:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left - (double)right;
                case Token.TokenType.PLUS:
                    {
                        if (left is double && right is double)
                        {
                            return (double)left + (double)right;
                        }
                        if (left is string && right is string)
                        {
                            return (string)left + (string)right;
                        }

                        throw new CsLoxRuntimeException(expr.Oper, "Operands must be two numbers or two strings.");
                    }
                case Token.TokenType.SLASH:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left / (double)right;
                case Token.TokenType.STAR:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left * (double)right;
            }

            // Should be unreachable
            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value!;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Oper.Type)
            {
                case Token.TokenType.BANG:
                    return !IsTruthy(right);
                case Token.TokenType.MINUS:
                    CheckNumberOperand(expr.Oper, right);
                    return -(double)right;


            }

            // unreachable
            return null;
        }

        /// <summary>
        /// Interpret a Lox expression.
        /// </summary>
        /// <param name="expr"></param>
        public void Interpret(Expr expr)
        {
            try
            {
                var value = Evaluate(expr);
                Console.WriteLine(Stringify(value));
            }
            catch (CsLoxRuntimeException e)
            {
                CLI.RuntimeError(e);
            }
        }

        /// <summary>
        /// Evaluate a given Lox expression.
        /// </summary>
        /// <param name="expr">An expression to evaluate.</param>
        /// <returns>An <see langword="object"/> containing the result of evaluating expr.</returns>
        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        /// <summary>
        /// Determines whether a Lox object is truthy or falsey.
        /// </summary>
        /// <param name="obj">An object to test for truthines.</param>
        /// <returns><see langword="false"/> if obj is <see langword="null"/> or <see langword="false"/>, otherwise <see langword="true"/>.</returns>
        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;
            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// <para>
        /// Unlike the reference Java implementation, <see cref="Object.Equals"/> has the same semantics as Lox; two <see langword="null"/> objects are equal.
        /// </para>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns><see langword="true"/> if the specified object is equal to the current object, otherwise <see langword="false"/>.</returns>
        private bool IsEqual(object left, object right)
        {
            return left.Equals(right);
        }

        private void CheckNumberOperand(Token oper, object operand)
        {
            if (operand is double) return;
            throw new CsLoxRuntimeException(oper, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token oper, object left, object right)
        {
            if (left is double && right is double) return;
            throw new CsLoxRuntimeException(oper, "Operands must be numbers.");
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "nil";

            if (obj is double)
            {
                return string.Format("{0:G29}", (double)obj);
            }

            return obj.ToString() ?? "nil";
        }
    }
}
