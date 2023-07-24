using CsLox.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// Container class for extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extends byte to be convertable to an opcode.
        /// </summary>
        /// <remarks>
        /// The one downside of this method is that this conversion is only one way, due to the many to one relationship
        /// between undefined bytes and the single <c>OP_UNDEFINED</c> value we cast them to.
        /// </remarks>
        /// <param name="value">The byte to convert to an Opcode.</param>
        /// <returns>The corresponding opcode value if there is one, <c>OP_UNDEFINED</c> if not.</returns>
        public static Chunk.Opcode ToOpcode(this byte value)
        {
            if (Enum.IsDefined(typeof(Chunk.Opcode), value))
            {
                return (Chunk.Opcode)value;
            }
            else
            {
                return Chunk.Opcode.OP_UNKNOWN;
            }
        }
    }
}
