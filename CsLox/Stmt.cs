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
 * assignment     → IDENTIFIER "=" assignment | equality ;
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
            T VisitVarStmt(Var stmt);
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

        public class Var : Stmt
        {
            public Token Name { get; }
            public Expr Initializer { get; }

            public Var (Token name, Expr initializer)
            {
                Name = name;
                Initializer = initializer;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitVarStmt(this);
            }

        } // Var

        public abstract T Accept<T>(IVisitor<T> visitor);
    } // Stmt
} // namespace