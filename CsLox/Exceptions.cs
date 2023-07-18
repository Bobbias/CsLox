using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    // TODO: Comment these.

    public class ParseException : ApplicationException
    {
        public ParseException() : base()
        {
        }

        public ParseException(string? message) : base(message)
        {
        }

        public ParseException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    public class CsLoxRuntimeException: ApplicationException
    {
        public Token? Token { get; private set; }

        public CsLoxRuntimeException() : base()
        {
        }

        public CsLoxRuntimeException(string? message) : base(message)
        {
        }

        public CsLoxRuntimeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public CsLoxRuntimeException(Token? token, string? message) : base(message)
        {
            Token = token;
        }

        public CsLoxRuntimeException(Token? token, string? message, Exception? innerException) : base(message, innerException)
        {
            Token = token;
        }

        public bool HasToken()
        {
            return Token != null;
        }
    }

    /// <summary>
    /// This exception encapsulates a return value from a Lox function call.
    /// </summary>
    public class Return: ApplicationException
    {
        public readonly object? Value;

        public Return() : base()
        {
        }

        public Return(string? message) : base(message)
        {
        }

        public Return(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public Return(object? value) : base()
        {
            Value = value;
        }
    }
}
