using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// Reresents a class in Lox.
    /// </summary>
    public class LoxClass: ILoxCallable
    {
        /// <summary>
        /// The name of the class.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc/>
        public int Arity { get; } = 0;

        /// <summary>
        /// Constructs a LoxClass with the given name.
        /// </summary>
        /// <param name="name"></param>
        public LoxClass(string name)
        {
            Name = name;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }


        public object Call(Interpreter interpreter, List<object> args)
        {
            var instance = new LoxInstance(this);

            return instance;
        }
    }
}
