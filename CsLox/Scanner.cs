﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using TokenType = CsLox.Token.TokenType;

namespace CsLox
{
    /// <summary>
    /// Implements a Tokenizer for the Lox language.
    /// </summary>
    internal class Scanner
    {
        private static Dictionary<string, TokenType> Keywords { get; } = new Dictionary<string, TokenType>();

        private string Source { get; }
        private List<Token> Tokens { get; } = new List<Token>();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        /// <summary>
        /// Constructs a <see cref="Scanner"/> from the given string.
        /// </summary>
        /// <param name="source">A string representing some Lox source code.</param>
        public Scanner(string source)
        {
            Source = source;

            Keywords.Add("and", TokenType.AND);
            Keywords.Add("class", TokenType.CLASS);
            Keywords.Add("else", TokenType.ELSE);
            Keywords.Add("false", TokenType.FALSE);
            Keywords.Add("for", TokenType.FOR);
            Keywords.Add("fun", TokenType.FUN);
            Keywords.Add("if", TokenType.IF);
            Keywords.Add("nil", TokenType.NIL);
            Keywords.Add("or", TokenType.OR);
            Keywords.Add("print", TokenType.PRINT);
            Keywords.Add("return", TokenType.RETURN);
            Keywords.Add("super", TokenType.SUPER);
            Keywords.Add("this", TokenType.THIS);
            Keywords.Add("true", TokenType.TRUE);
            Keywords.Add("var", TokenType.VAR);
            Keywords.Add("while", TokenType.WHILE);
        }

        /// <summary>
        /// Tokenizes the <see cref="Source"/>, inserting an EOF at the end.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of <see cref="Token"/> representing the <see cref="Source"/>.</returns>
        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            Tokens.Add(new Token(TokenType.EOF, "", null, line));
            return Tokens;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                // Single character tokens
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                // Single or double character tokens
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                // Slash or comment
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                // Whitespace
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;
                case '\n':
                    line++;
                    break;
                // Handle strings
                case '"': LexString(); break;
                    
                // Error
                default:
                    if (Char.IsDigit(c))
                    {
                        LexNumber();
                    }
                    else if (Char.IsLetter(c))
                    {
                        LexIdentifier();
                    }
                    else
                    {
                        CLI.Error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        /// <summary>
        /// Indicates whether we're at the end of the <see cref="Source"/>.
        /// </summary>
        /// <returns><see langword="true"/> if we're at the end, else <see langword="false"/></returns>
        private bool IsAtEnd()
        {
            return current >= Source.Length;
        }

        /// <summary>
        /// Consumes a character from <see cref="Source"/>
        /// </summary>
        /// <returns></returns>
        private char Advance()
        {
            return Source.ElementAt(current++);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return Source.ElementAt(current);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private char PeekNext()
        {
            if (current + 1 >= Source.Length) return '\0';
            return Source.ElementAt(current + 1);
        }

        /// <summary>
        /// Adds a token with a null value to <see cref="Tokens"/>.
        /// </summary>
        /// <param name="type"></param>
        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        /// <summary>
        /// Adds a token with a value to <see cref="Tokens"/>.
        /// </summary>
        /// <param name="type">A <see cref="TokenType"/> representing the current Token.</param>
        /// <param name="literal">An <see cref="object"/> representing a literal value.</param>
        private void AddToken(TokenType type, object? literal)
        {
            var text = Source.Substring(start, start - current);
            Tokens.Add(new Token(type, text, literal, line));
        }

        /// <summary>
        /// Returns a boolean indicating whether or not the next character in <see cref="Source"/> is <paramref name="expected"/> or not.
        /// Only consumes a character if <paramref name="expected"/> matches.
        /// </summary>
        /// <param name="expected">The expected next character</param>
        /// <returns>A bool indicating whether <paramref name="expected"/> is the next character in <see cref="Source"/>.</returns>
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (Source.ElementAt(current) != expected) return false;

            current++;
            return true;
        }

        private void LexString()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            if (IsAtEnd())
            {
                CLI.Error(line, "Unterminated string.");
                return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            var value = Source.Substring(start + 1, (current - 1) - start);
            AddToken(TokenType.STRING, value);
        }

        private void LexNumber()
        {
            while (Char.IsDigit(Peek())) Advance();

            // Look for a fractional part.
            if (Peek() == '.' && Char.IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (Char.IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.NUMBER,
                Double.Parse(Source.Substring(start, current - start)));
        }

        private void LexIdentifier()
        {
            while (Char.IsLetterOrDigit(Peek())) Advance();

            string text = Source.Substring(start, current - start);
            TokenType type;
            var result = Keywords.TryGetValue(text, out type);
            if (!result) type = TokenType.IDENTIFIER;
            AddToken(type);
        }

    }
}