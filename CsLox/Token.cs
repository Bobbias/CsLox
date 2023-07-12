using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    internal class Token
    {
        public enum TokenType
        {
            // Single-character tokens.
            LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
            COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

            // One or two character tokens.
            BANG, BANG_EQUAL,
            EQUAL, EQUAL_EQUAL,
            GREATER, GREATER_EQUAL,
            LESS, LESS_EQUAL,

            // Literals.
            IDENTIFIER, STRING, NUMBER,

            // Keywords.
            AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
            PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,

            EOF
        }

        public TokenType Type { get; }
        public string Lexeme { get; }
        public object? Literal { get; }
        public int Line { get; }

        public Token(TokenType type, string lexeme, object? literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }
    }
}
