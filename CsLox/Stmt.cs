using System;

namespace CsLox
{
/*
 * The following EBNF describes the disambiguated Lox grammar.
 * program        → statement* EOF ;
 * statement      → exprStmt | ifStmt | printStmt | block ;
 * exprStmt       → expression ";" ;
 * ifStmt         → "if" "(" expression ")" statement ( "else" statement )? ;
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
    abstract public class Stmt
    {
        public interface IVisitor<T>
        {
            T VisitBlockStmt(Block stmt);
            T VisitExpressionStmt(Expression stmt);
            T VisitIfStmt(If stmt);
            T VisitPrintStmt(Print stmt);
            T VisitVarStmt(Var stmt);
        } // iVisitor<T>

        public class Block : Stmt
        {
            public List<Stmt> Stmts { get; }

            public Block (List<Stmt> stmts)
            {
                Stmts = stmts;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }

        } // Block

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

        public class If : Stmt
        {
            public Expr Cond { get; }
            public Stmt ThenBranch { get; }
            public Stmt? ElseBranch { get; }

            public If (Expr cond, Stmt thenBranch, Stmt? ElseBranch)
            {
                Cond = cond;
                ThenBranch = thenBranch;
                ElseBranch = ElseBranch;
            }


            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitIfStmt(this);
            }

        } // If

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
