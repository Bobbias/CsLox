using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox.Compiler
{
    /// <summary>
    /// Provides utility functions for debugging purposes.
    /// </summary>
    public static class Debugging
    {
        /// <summary>
        /// Prints a disassembly listing for the <see cref="Chunk"/> provided, with <paramref name="name"/> as a header.
        /// </summary>
        /// <param name="chunk">The Chunk to disassemble.</param>
        /// <param name="name">The name of the Chunk.</param>
        public static void DisassembleChunk(Chunk chunk, string name)
        {
            Console.WriteLine($"== {name} ==");

            for (int offset = 0; offset < chunk.Code.Count;)
            {
                offset = DisassembleInstruction(chunk, offset);
            }
        }

        /// <summary>
        /// Prints a readable disassembly listing for the given instruction.
        /// </summary>
        /// <param name="chunk">The chunk containing instructions to disassemble.</param>
        /// <param name="offset">The offset into the chunk representing the instruction to disassemble.</param>
        /// <returns>The offset of the next instruction in the chunk.</returns>
        public static int DisassembleInstruction(Chunk chunk, int offset)
        {
            var value = chunk[offset];
            var opcode = value.ToOpcode();

            Console.Write($"{offset:D4} ");
            if (offset > 0 && chunk.Lines[offset] == chunk.Lines[offset - 1])
            {
                Console.Write("   | ");
            }
            else
            {
                Console.Write($"{chunk.Lines[offset],4} ");
            }
            switch (opcode) {
                case Chunk.Opcode.OP_CONSTANT:
                    return ConstantInstruction(opcode.ToString(), chunk, offset);
                case Chunk.Opcode.OP_RETURN:
                    return SimpleInstruction(opcode.ToString(), offset);
                default:
                    Console.WriteLine($"Unknown opcode {value}");
                    break;
            }

            return offset + 1;
        }

        /// <summary>
        /// Disassembles single byte instructions, and advances the offset appropriately.
        /// </summary>
        /// <param name="name">The name of the instruction being disassembled.</param>
        /// <param name="offset">The current offset in the chunk.</param>
        /// <returns>The offset of the next instruction, <paramref name="offset"/> + 1.</returns>
        public static int SimpleInstruction(string name, int offset)
        {
            Console.WriteLine(name);
            return offset + 1;
        }

        /// <summary>
        /// Disassembles <c>OP_CONSTANT</c> instructions, printing values in their smallest form. Advances the offset by 2.
        /// </summary>
        /// <param name="name">The name of the instruction being disassembled.</param>
        /// <param name="chunk">The Chunk being disassembled.</param>
        /// <param name="offset">The current offset in the chunk.</param>
        /// <returns>The offset of the next instruction, <paramref name="offset"/> + 2.</returns>
        public static int ConstantInstruction(string name, Chunk chunk, int offset)
        {
            byte index = chunk[offset + 1];
            Console.WriteLine($"{name,-16} {index,4} '{chunk.Constants.Values[index]:g}'");
            return offset + 2;
        }
    }
}
