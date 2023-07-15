using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static CsLox.Token;
using static System.Runtime.InteropServices.JavaScript.JSType;

/*
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

namespace CsLox
{
    internal class Parser
    {
        private List<Token> Tokens { get; }
        private int Current { get; set; } = 0;

        public Parser(List<Token> tokens)
        {
            Tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        /// <summary>
        /// expression     → equality ;
        /// </summary>
        /// <returns></returns>
        private Expr Expression()
        {
            return Assignment();
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.VAR)) return VarDeclaration();

                return Statement();
            }
            catch (ParseException e)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt Statement()
        {
            if (Match(TokenType.PRINT))
            {
                return PrintStatement();
            }
            if (Match(TokenType.LEFT_BRACE))
            {
                return new Stmt.Block(Block());
            }

            return ExpressionStatement();
        }

        private Stmt PrintStatement()
        {
            var value = Expression();
            Consume(TokenType.SEMICOLON, "Expected ';' after value.");

            return new Stmt.Print(value);
        }

        private Stmt VarDeclaration()
        {
            var name = Consume(TokenType.IDENTIFIER, "Expected variable name.");

            Expr initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt ExpressionStatement()
        {
            var expr = Expression();
            Consume(TokenType.SEMICOLON, "Expected ';' after expression.");

            return new Stmt.Expression(expr);
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expected '}' after block.");
            return statements;
        }

        /// <summary>
        /// assignment     → IDENTIFIER "=" assignment | equality ;
        /// </summary>
        /// <returns></returns>
        private Expr Assignment()
        {
            var expr = Equality();

            if(Match(TokenType.EQUAL))
            {
                var equals = Previous();
                var value = Assignment();

                if (expr is Expr.Variable)
                {
                    var name = ((Expr.Variable)expr).Name;
                    return new Expr.Assign(name, value);
                }

                // Note: We report an error, but do not throw one here.
                //       This is because the parser is not in an unknown state and there is no need to attempt to recover.
                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        /// <summary>
        /// equality       → comparison ( ( "!=" | "==" ) comparison )* ;
        /// </summary>
        /// <returns></returns>
        private Expr Equality()
        {
            var expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                var oper = Previous();
                var right = Comparison();
                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        /// <summary>
        /// comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
        /// </summary>
        /// <returns></returns>
        private Expr Comparison()
        {
            var expr = Term();

            while(Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                var oper = Previous();
                var right = Term();
                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        /// <summary>
        /// term           → factor ( ( "-" | "+" ) factor )* ;
        /// </summary>
        /// <returns></returns>
        private Expr Term()
        {
            var expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                var oper = Previous();
                var right = Factor();
                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        /// <summary>
        /// factor         → unary ( ( "/" | "*" ) unary )* ;
        /// </summary>
        /// <returns></returns>
        private Expr Factor()
        {
            var expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                var oper = Previous();
                var right = Unary();
                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        /// <summary>
        /// unary          → ( "!" | "-" ) unary | primary ;
        /// </summary>
        /// <returns></returns>
        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                var oper = Previous();
                var right = Unary();
                return new Expr.Unary(oper, right);
            }

            return Primary();
        }

        /// <summary>
        /// primary        → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" ;
        /// </summary>
        /// <returns></returns>
        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().Literal);
            }

            if (Match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(Previous());
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expected expression.");
        }

        /// <summary>
        /// Checks whether the current <see cref="Token"/> matches any of the given <see cref="TokenType"/>s.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the current <see cref="Token"/> matches the given <see cref="TokenType"/>, and whether we are at the end.
        /// </summary>
        /// <param name="type"></param>
        /// <returns><see langword="true"/> if the current token matches the given type. If we are at the end of the input, or it does not match, returns <see langword="false"/> instead.</returns>
        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        /// <summary>
        /// As long as the Parser has not reached the end, advances to the next <see cref="Token"/>, returning it.
        /// </summary>
        /// <returns>The current <see cref="Token"/>.</returns>
        private Token Advance()
        {
            if (!IsAtEnd()) Current++;
            return Previous();
        }

        /// <summary>
        /// Checks if the current token is <see cref="TokenType.EOF"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the next token is <see cref="TokenType.EOF"/> else <see langword="false"/>.</returns>
        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        /// <summary>
        /// Returns the current <see cref="Token"/> without consuming it.
        /// </summary>
        /// <returns>The current <see cref="Token"/>.</returns>
        private Token Peek()
        {
            return Tokens.ElementAt(Current);
        }

        /// <summary>
        /// Returns the previous <see cref="Token"/>.
        /// </summary>
        /// <returns>The previous <see cref="Token"/>.</returns>
        private Token Previous()
        {
            return Tokens.ElementAt(Current - 1);
        }

        /// <summary>
        /// Consumes a <see cref="Token"/> of the given <see cref="TokenType"/> otherwise throws an exception.
        /// </summary>
        /// <param name="type">A <see cref="TokenType"/> to check the current token against.</param>
        /// <param name="message">A string containing an error message to display to the user in the case of a failure to consume the provided <see cref="TokenType"/>.</param>
        /// <returns>The next <see cref="Token"/>.</returns>
        /// <exception cref="ParseException">Thrown when the next <see cref="Token"/> does not match the provided <see cref="TokenType"/>.</exception>
        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        /// <summary>
        /// Reports an error to the end user and creates an exception.
        /// </summary>
        /// <param name="token">The <see cref="Token"/> where the error occurred.</param>
        /// <param name="message">A string containing an error message to display to the user.</param>
        /// <returns>A <see cref="ParseException"/> containing the given error message.</returns>
        private ParseException Error(Token token, string message)
        {
            CLI.Error(token, message);
            return new ParseException(message);
        }

        /// <summary>
        /// Attempts to recover from a parse error by skipping ahead to look for the start of a new expression.
        /// <para/>
        /// This may fail in certain situations, but it's not strictly necessary to ensure this always works.
        /// </summary>
        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.SEMICOLON) return;

                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }
    }
}
