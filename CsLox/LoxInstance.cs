using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// Represents an instance of a class in the Lox interpreter.
    /// </summary>
    public class LoxInstance
    {
        /// <summary>
        /// The class definition this instance is based on.
        /// </summary>
        private LoxClass @class;

        /// <summary>
        /// Creates a runtime instance of the given <see cref="LoxClass"/>.
        /// </summary>
        /// <param name="class"></param>
        LoxInstance(LoxClass @class)
        {
            this.@class = @class;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{@class.Name} instance";
        }
    }
}
