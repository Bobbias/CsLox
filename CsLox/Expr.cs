using System;

namespace CsLox
{
/*
 * The following EBNF describes the disambiguated Lox grammar.
 * program        → statement* EOF ;
 * statement      → exprStmt | printStmt ;
 * exprStmt       → expression ";" ;
 * printStmt      → "print" expression ";" ;
 * expression     → assignment ;
 * assignment     → IDENTIFIER "=" assignment | logic_or ;
 * logic_or       → logic_and ( "or" logic_and )* ;
 * logic_and      → equality ( "and" equality )* ;
 * equality       → comparison ( ( "!=" | "==" ) comparison )* ;
 * comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
 * term           → factor ( ( "-" | "+" ) factor )* ;
 * factor         → unary ( ( "/" | "*" ) unary )* ;
 * unary          → ( "!" | "-" ) unary
 *                | primary ;
 * primary        → NUMBER | STRING | "true" | "false" | "nil"
 *                | "(" expression ")" ;
 */
    abstract public class Expr
    {
        public interface IVisitor<T>
        {
            T VisitAssignExpr(Assign expr);
            T VisitBinaryExpr(Binary expr);
            T VisitCallExpr(Call expr);
            T VisitGroupingExpr(Grouping expr);
            T VisitLiteralExpr(Literal expr);
            T VisitLogicalExpr(Logical expr);
            T VisitUnaryExpr(Unary expr);
            T VisitVariableExpr(Variable expr);
        } // iVisitor<T>

        public class Assign : Expr
        {
            public Token Name { get; }
            public Expr Value { get; }

            public Assign (Token name, Expr value)
            {
                Name = name;
                Value = value;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }

        } // Assign

        public class Binary : Expr
        {
            public Expr Left { get; }
            public Token Oper { get; }
            public Expr Right { get; }

            public Binary (Expr left, Token oper, Expr right)
            {
                Left = left;
                Oper = oper;
                Right = right;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }

        } // Binary

        public class Call : Expr
        {
            public Expr Callee { get; }
            public Token Paren { get; }
            public List<Expr> Args { get; }

            public Call (Expr callee, Token paren, List<Expr> args)
            {
                Callee = callee;
                Paren = paren;
                Args = args;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitCallExpr(this);
            }

        } // Call

        public class Grouping : Expr
        {
            public Expr Expression { get; }

            public Grouping (Expr expression)
            {
                Expression = expression;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

        } // Grouping

        public class Literal : Expr
        {
            public object? Value { get; }

            public Literal (object? value)
            {
                Value = value;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

        } // Literal

        public class Logical : Expr
        {
            public Expr Left { get; }
            public Token Oper { get; }
            public Expr Right { get; }

            public Logical (Expr left, Token oper, Expr right)
            {
                Left = left;
                Oper = oper;
                Right = right;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }

        } // Logical

        public class Unary : Expr
        {
            public Token Oper { get; }
            public Expr Right { get; }

            public Unary (Token oper, Expr right)
            {
                Oper = oper;
                Right = right;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

        } // Unary

        public class Variable : Expr
        {
            public Token Name { get; }

            public Variable (Token name)
            {
                Name = name;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }

        } // Variable

        public abstract T Accept<T>(IVisitor<T> visitor);
    } // Expr
} // namespace
