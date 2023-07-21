using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// Represents a failure in the parsing phase when reading Lox source.
    /// </summary>
    public class ParseException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the ParseException class.
        /// </summary>
        public ParseException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ParseException class with a specified error message.
        /// </summary>
        public ParseException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ParseException class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        public ParseException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Represents an error during runtime.
    /// </summary>
    public class CsLoxRuntimeException: ApplicationException
    {
        /// <summary>
        /// The token which this exception is associated with.
        /// </summary>
        public Token? Token { get; private set; }

        /// <summary>
        /// Initializes a new instance of the CsLoxRuntimeException class.
        /// </summary>
        public CsLoxRuntimeException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the CsLoxRuntimeException class with a specified error message.
        /// </summary>
        public CsLoxRuntimeException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CsLoxRuntimeException class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        public CsLoxRuntimeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CsLoxRuntimeException class with a reference to a token indicating the cause of this error, and a specified error message.
        /// </summary>
        public CsLoxRuntimeException(Token? token, string? message) : base(message)
        {
            Token = token;
        }

        /// <summary>
        /// Initializes a new instance of the CsLoxRuntimeException class with a reference to a token indicating the cause of this error, a specified error message, and a reference
        /// to the inner exception that is the cause of this exception.
        /// </summary>
        public CsLoxRuntimeException(Token? token, string? message, Exception? innerException) : base(message, innerException)
        {
            Token = token;
        }

        /// <summary>
        /// Indicates whether the exception was given a reference to a Token or not.
        /// </summary>
        /// <returns><see langword="true"/> if token is nonnull, otherwise <see langword="false"/>.</returns>
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
        /// <summary>
        /// Holds whatever value is returned from the Lox function which threw this exception.
        /// </summary>
        public readonly object? Value;

        /// <summary>
        /// Initializes a new instance of the Return class.
        /// </summary>
        public Return() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Return class with a specified message.
        /// </summary>
        public Return(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Return class with a specified message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The specified message.</param>
        /// <param name="innerException">A reference to the exception that is the cause of this exception.</param>
        public Return(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Return class with a specified return value from the Lox function which threw this exception.
        /// </summary>
        /// <param name="value">The return value from the Lox function which threw this exception.</param>
        public Return(object? value) : base()
        {
            Value = value;
        }
    }
}
