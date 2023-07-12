using System;

namespace CsLox
{
    abstract internal class Expr
    {
        public interface IVisitor<T>
        {
            T VisitBinaryExpr(Binary expr);
            T VisitGroupingExpr(Grouping expr);
            T VisitLiteralExpr(Literal expr);
            T VisitUnaryExpr(Unary expr);
        } // iVisitor<T>

        public class Binary : Expr
        {
            public override T Accept<T>(IVisitor<T> visitor) {
                return visitor.VisitBinaryExpr(this);
            }

            public Expr Left { get; }
            public Token Oper { get; }
            public Expr Right { get; }
            public Binary (Expr left, Token oper, Expr right)
            {
                Left = left;
                Oper = oper;
                Right = right;
            } // ctor

        } // Binary

        public class Grouping : Expr
        {
            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

            public Expr Expression { get; }
            public Grouping (Expr expression)
            {
                Expression = expression;
            } // ctor

        } // Grouping

        public class Literal : Expr
        {
            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            public object Value { get; }
            public Literal (object value)
            {
                Value = value;
            } // ctor

        } // Literal

        public class Unary : Expr
        {
            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

            public Token Oper { get; }
            public Expr Right { get; }
            public Unary (Token oper, Expr right)
            {
                Oper = oper;
                Right = right;
            } // ctor

        } // Unary

    public abstract T Accept<T>(IVisitor<T> visitor);
    } // Expr
} // namespace
