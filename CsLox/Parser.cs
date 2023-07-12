using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static CsLox.Token;
using static System.Runtime.InteropServices.JavaScript.JSType;

/*
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

        public Expr? Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseException e)
            {
                return null;
            }
        }

        private Expr Expression()
        {
            return Equality();
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

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expected expression.");
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Token Advance()
        {
            if (!IsAtEnd()) Current++;
            return Previous();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Token Peek()
        {
            return Tokens.ElementAt(Current);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Token Previous()
        {
            return Tokens.ElementAt(Current - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private ParseException Error(Token token, string message)
        {
            CLI.Error(token, message);
            return new ParseException(message);
        }

        /// <summary>
        /// 
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
