using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static CsLox.Token;

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
    /// <summary>
    /// Implements a revursive descent parser for the Lox language.
    /// </summary>
    internal class Parser
    {
        /// <summary>
        /// A list of tokens to parse.
        /// </summary>
        private List<Token> Tokens { get; }

        /// <summary>
        /// The index of the current Token.
        /// </summary>
        private int Current { get; set; } = 0;

        /// <summary>
        /// Constructs a Parser from a list of Tokens.
        /// </summary>
        /// <param name="tokens">The list of tokens to parse.</param>
        public Parser(List<Token> tokens)
        {
            Tokens = tokens;
        }

        /// <summary>
        /// Parses the tokens, and produces a list of statements.
        /// </summary>
        /// <returns>A list of <see cref="Stmt"/>.</returns>
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
        /// <returns>An <see cref="Expr"/>.</returns>
        private Expr Expression()
        {
            return Assignment();
        }

        /// <summary>
        /// Parses a variable declaration statement.
        /// </summary>
        /// <returns>A <see cref="Stmt"/>, or <see langword="null"/> if an error is encountered.</returns>
        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.FUN)) return Function("function");
                if (Match(TokenType.VAR)) return VarDeclaration();

                return Statement();
            }
            catch (ParseException e)
            {
                Synchronize();
                return null;
            }
        }

        /// <summary>
        /// Parses a statement.
        /// </summary>
        /// <returns>A <see cref="Stmt"/>.</returns>
        private Stmt Statement()
        {
            if (Match(TokenType.FOR))
            {
                return ForStatement();
            }
            if (Match(TokenType.IF))
            {
                return IfStatement();
            }
            if (Match(TokenType.PRINT))
            {
                return PrintStatement();
            }
            if (Match(TokenType.RETURN))
            {
                return ReturnStatement();
            }
            if (Match(TokenType.WHILE))
            {
                return WhileStatement();
            }
            if (Match(TokenType.LEFT_BRACE))
            {
                return new Stmt.Block(Block());
            }

            return ExpressionStatement();
        }

        /// <summary>
        /// Parses a for statement.
        /// </summary>
        /// <remarks>
        /// The parser here desugars for loops into while loops.
        /// </remarks>
        /// <returns>A <see cref="Stmt.While"/>.</returns>
        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after 'for'.");

            Stmt initializer;
            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if(Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr cond = null;
            if (!Check(TokenType.SEMICOLON))
            {
                cond = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expected ';' after loop condition.");

            Expr increment = null;
            if(!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after for clauses.");

            var body = Statement();

            if (increment != null)
            {
                body = new Stmt.Block(
                    new List<Stmt> {
                        body,
                        new Stmt.Expression(increment)
                    }
                );
            }

            if (cond == null)
            {
                cond = new Expr.Literal(true);
            }
            body = new Stmt.While(cond, body);

            if (initializer != null)
            {
                body = new Stmt.Block(
                    new List<Stmt> {
                        initializer,
                        body
                    }
                );
            }

            return body;
        }

        /// <summary>
        /// Parses an if statement.
        /// </summary>
        /// <returns>A <see cref="Stmt.If"/>.</returns>
        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after 'if'.");
            var condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after if condition.");

            var thenBranch = Statement();
            Stmt? elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        /// <summary>
        /// Parses a print statement.
        /// </summary>
        /// <returns>A <see cref="Stmt.Print"/>.</returns>
        private Stmt PrintStatement()
        {
            var value = Expression();
            Consume(TokenType.SEMICOLON, "Expected ';' after value.");

            return new Stmt.Print(value);
        }

        /// <summary>
        /// Parses a return statement.
        /// </summary>
        /// <returns>A <see cref="Stmt.Return"/>.</returns>
        private Stmt ReturnStatement()
        {
            var keyword = Previous();
            Expr value = null;
            if(!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expected ';' after return value.");
            return new Stmt.Return(keyword, value);
        }

        /// <summary>
        /// Parses a while statement.
        /// </summary>
        /// <returns>A <see cref="Stmt.While"/>.</returns>
        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after 'while'.");
            var cond = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after condition.");
            var body = Statement();

            return new Stmt.While(cond, body);
        }

        /// <summary>
        /// Parses a variable declaration statement.
        /// </summary>
        /// <returns>A <see cref="Stmt"/>.</returns>
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

        /// <summary>
        /// Parses an expression statement.
        /// </summary>
        /// <returns>A <see cref="Stmt"/></returns>
        private Stmt ExpressionStatement()
        {
            var expr = Expression();
            Consume(TokenType.SEMICOLON, "Expected ';' after expression.");

            return new Stmt.Expression(expr);
        }

        private Stmt.Function Function(string kind)
        {
            var name = Consume(TokenType.IDENTIFIER, $"Expected {kind} name.");
            Consume(TokenType.LEFT_PAREN, $"Expected '(' after {kind} name.");
            var parameters = new List<Token>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        // FIXME: does this need to throw?
                        Error(Peek(), "Can't have more than 255 parameters.");
                    }

                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expected parameter name."));
                } while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after parameters");

            Consume(TokenType.LEFT_BRACE, $"Expected '}}' before {kind} body.");
            var body = Block();
            return new Stmt.Function(name, parameters, body);
        }

        /// <summary>
        /// Parses a block statement.
        /// </summary>
        /// <returns>A list of <see cref="Stmt"/>s.</returns>
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
        /// <returns>A <see cref="Stmt"/>.</returns>
        private Expr Assignment()
        {
            var expr = Or();

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
        /// Parses an or expression.
        /// </summary>
        /// <returns>An <see cref="Expr"/>.</returns>
        private Expr Or()
        {
            var expr = And();

            while (Match(TokenType.OR))
            {
                var oper = Previous();
                var right = And();
                expr = new Expr.Logical(expr, oper, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses an and expression.
        /// </summary>
        /// <returns>An <see cref="Expr"/>.</returns>
        private Expr And()
        {
            var expr = Equality();

            while (Match(TokenType.AND))
            {
                var oper = Previous();
                var right = Equality();
                expr = new Expr.Logical(expr, oper, right);
            }

            return expr;
        }

        /// <summary>
        /// equality       → comparison ( ( "!=" | "==" ) comparison )* ;
        /// </summary>
        /// <returns>An <see cref="Expr"/>.</returns>
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
        /// <returns>An <see cref="Expr"/>.</returns>
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
        /// <returns>An <see cref="Expr"/>.</returns>
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
        /// <returns>An <see cref="Expr"/>.</returns>
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
        /// <returns>An <see cref="Expr"/>.</returns>
        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                var oper = Previous();
                var right = Unary();
                return new Expr.Unary(oper, right);
            }

            return Call();
        }

        /// <summary>
        /// call           → primary ( "(" arguments? ")" )* ;
        /// </summary>
        /// <returns>An <see cref="Expr"/>.</returns>
        private Expr Call()
        {
            var expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        /// <summary>
        /// Utility function to handle parsing the argument list.
        /// </summary>
        /// <param name="callee"></param>
        /// <returns>
        /// An <see cref="Expr.Call"/> containing the callee passed from <see cref="Call"/>,
        /// the closing parenthesis for error reporting, and the argument list.
        /// </returns>
        private Expr FinishCall(Expr callee)
        {
            var args = new List<Expr>();

            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if(args.Count >= 255)
                    {
                        // FIXME: Does this need to throw?
                        Error(Peek(), "Can't have more than 255 arguments.");
                    }
                    args.Add(Expression());
                } while (Match(TokenType.COMMA));
            }

            var paren = Consume(TokenType.RIGHT_PAREN, "Expected ')' after arguments.");

            return new Expr.Call(callee, paren, args);
        }

        /// <summary>
        /// primary        → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" ;
        /// </summary>
        /// <returns>An <see cref="Expr"/>.</returns>
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
        /// <param name="types">Some number of <see cref="TokenType"/>s to match the current Token against.</param>
        /// <returns><see langword="true"/> if any of the provided <see cref="TokenType"/>s match, otherwise <see langword="false"/>. Advances to the next Token if <see langword="true"/>.</returns>
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
