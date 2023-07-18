using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// Describes a callable object in Lox.
    /// </summary>
    public interface ILoxCallable
    {
        /// <summary>
        /// The number of arguments the callable takes.
        /// </summary>
        public int Arity { get; }

        /// <summary>
        /// Performs a function call.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="args"></param>
        /// <returns>An <see langword="object"/> containing the result of the function call.</returns>
        object Call(Interpreter interpreter, List<object> args);
    }
}
