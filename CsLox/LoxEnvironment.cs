using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    internal class LoxEnvironment
    {
        public LoxEnvironment? Enclosing { get; private set; }
        private Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public LoxEnvironment()
        {
            Enclosing = null;
        }

        public LoxEnvironment(LoxEnvironment enclosing)
        {
            Enclosing = enclosing;
        }

        /// <summary>
        /// Clears the dictionary.
        /// <para/>
        /// This is useful when running tests, as typically the root environment does not get cleared between calls to <see cref="CLI.RunFile(FileInfo)"/>.
        /// </summary>
        public void Clear()
        {
            Values.Clear();
        }

        /// <summary>
        /// Adds a variable definition with the provided name and value to the current environment.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value assigned to the variable.</param>
        public void Define(string name, object value)
        {
            Values.Add(name, value);
        }

        /// <summary>
        /// Retrieves a previously defined variable by name.
        /// </summary>
        /// <param name="name">The name of the variable in question.</param>
        /// <returns>An <see langword="object"/> containing the value of the variable.</returns>
        /// <exception cref="CsLoxRuntimeException">Throws an exception if the given variable is not defined on either this environment or an enclosing one.</exception>
        public object Get(Token name)
        {
            if (Values.ContainsKey(name.Lexeme)) return Values[name.Lexeme];

            if (Enclosing != null)
            {
                return Enclosing.Get(name);
            }

            throw new CsLoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }

        /// <summary>
        /// Reassigns an existing variable to a new value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="CsLoxRuntimeException">Throws an exception if the given variable is not defined on either this environment or an enclosing one.</exception>
        public void Assign(Token name, object value)
        {
            if (Values.ContainsKey(name.Lexeme))
            {
                Values[name.Lexeme] = value;
                return;
            }

            if (Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }

            throw new CsLoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }
}
