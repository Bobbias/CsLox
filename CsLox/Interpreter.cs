using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    // Note: Stmt.IVisitor<object> should actually be Stmt.IVisitor<void>, but that is not possible in c#.
    //       Instead what we have to do is create a dummy return type (in this case, object) and return
    //       dummy values from our Visit functions (in this case null).
    //       This would be cleaner in either F# or any language that has Unit, or Void as a usable type
    //       in these situations.
    internal class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private LoxEnvironment env = new LoxEnvironment();

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

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return env.Get(expr.Name);
        }

        /// <summary>
        /// Interpret a Lox expression.
        /// </summary>
        /// <param name="expr"></param>
        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
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

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Stmts, new LoxEnvironment(env));
            return null;
        }

        private void ExecuteBlock(List<Stmt> stmts, LoxEnvironment env)
        {
            var previous = this.env;
            try
            {
                this.env = env;

                foreach (var stmt in stmts)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                this.env = previous;
            }
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

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.Expr);
            
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            var value = Evaluate(stmt.Expr);
            Console.WriteLine(Stringify(value));

            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            env.Define(stmt.Name.Lexeme, value);
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            var value = Evaluate(expr.Value);

            env.Assign(expr.Name, value);

            return value;
        }
    }
}
