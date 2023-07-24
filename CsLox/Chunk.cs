using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CsLox.Compiler
{
    /// <summary>
    /// Represents a chunk of bytecode data.
    /// </summary>
    public class Chunk
    {
        /// <summary>
        /// Represents a bytecode operation.
        /// </summary>
        public enum Opcode: byte
        {
            /// <summary>
            /// Constant value, 2 bytes.
            /// </summary>
            OP_CONSTANT,
            /// <summary>
            /// Return, 1 byte.
            /// </summary>
            OP_RETURN,
            /// <summary>
            /// Unknown, 1 byte.
            /// </summary>
            OP_UNKNOWN
        }

        /// <summary>
        /// Represents the raw bytes for a chunk of bytecode.
        /// </summary>
        public List<byte> Code { get; }

        /// <summary>
        /// Contains the line number in the original source that each instruction in <see cref="Code"/> corresponds to.
        /// </summary>
        public List<int> Lines { get; }

        /// <summary>
        /// Contains the constants defined in the source code.
        /// </summary>
        public ValueTable Constants { get; }

        /// <summary>
        /// Provides an indexer to access <see cref="Code"/> through the chunk.
        /// </summary>
        /// <param name="i">The index to access.</param>
        /// <returns>the value of code at the given index.</returns>
        public byte this[int i]
        {
            get { return Code[i]; }
            set { Code[i] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the Chunk class.
        /// </summary>
        public Chunk()
        {
            Code = new List<byte>();
            Lines = new List<int>();
            Constants = new ValueTable();
        }

        /// <summary>
        /// Wraps the <see cref="List{T}.Add(T)"/> function to keep the API closer to what the book describes. This method uses <see cref="MethodImplOptions.AggressiveInlining"/>
        /// to hopefully avoid unnecessary indirection.
        /// </summary>
        /// <param name="data">The byte to write into <see cref="Code"/>.</param>
        /// <param name="line">The line number in the source code associated with this instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunk(byte data, int line)
        {
            Code.Add(data);
            Lines.Add(line);

            // If Code and Lines ever get out of sync, that's a big problem.
            Debug.Assert(Code.Count == Lines.Count, $"Chunk.WriteChunk: Somehow Code.Count != Lines.Count!");
        }

        /// <summary>
        /// Adds <paramref name="value"/> to this chunk's list of constants. Returns the index at which the value was inserted.
        /// </summary>
        /// <param name="value">The value to insert.</param>
        /// <returns>The index at which the value was inserted.</returns>
        public int AddConstant(double value)
        {
            Constants.Values.Add(value);
            return Constants.Values.Count - 1;
        }


    }
}
