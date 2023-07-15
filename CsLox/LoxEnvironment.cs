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

        public void Define(string name, object value)
        {
            Values.Add(name, value);
        }

        public object Get(Token name)
        {
            if (Values.ContainsKey(name.Lexeme)) return Values[name.Lexeme];

            if (Enclosing != null)
            {
                return Enclosing.Get(name);
            }

            throw new CsLoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public void Assign(Token name, object value)
        {
            if (Values.ContainsKey(name.Lexeme))
            {
                Values.Add(name.Lexeme, value);
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
