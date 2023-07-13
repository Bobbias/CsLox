using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
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
}
