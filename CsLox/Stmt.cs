using System;

namespace CsLox
{
/*
 * The following EBNF describes the disambiguated Lox grammar.
 * program        → statement* EOF ;
 * statement      → exprStmt | printStmt ;
 * exprStmt       → expression ";" ;
 * printStmt      → "print" expression ";" ;
 * expression     → equality ;
 * equality       → comparison ( ( "!=" | "==" ) comparison )* ;
 * comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
 * term           → factor ( ( "-" | "+" ) factor )* ;
 * factor         → unary ( ( "/" | "*" ) unary )* ;
 * unary          → ( "!" | "-" ) unary
 *                | primary ;
 * primary        → NUMBER | STRING | "true" | "false" | "nil"
 *                | "(" expression ")" ;
 */
    abstract internal class Stmt
    {
        public interface IVisitor<T>
        {
            T VisitExpressionStmt(Expression stmt);
            T VisitPrintStmt(Print stmt);
        } // iVisitor<T>

        public class Expression : Stmt
        {
            public Expr Expr { get; }

            public Expression (Expr expr)
            {
                Expr = expr;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }

        } // Expression

        public class Print : Stmt
        {
            public Expr Expr { get; }

            public Print (Expr expr)
            {
                Expr = expr;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }

        } // Print

        public abstract T Accept<T>(IVisitor<T> visitor);
    } // Stmt
} // namespace
