using CsLox.Compiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// Contains all the tests for the VM.
    /// </summary>
    public class VMTests
    {
        [OneTimeSetUp]
        public void StartTest()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        [OneTimeTearDown]
        public void EndTest()
        {
            Trace.Flush();
        }

        [Test]
        public void DisassembleConstantAndReturnTest()
        {
            var chunk = new Chunk();
            var constant = chunk.AddConstant(1.2);
            chunk.WriteChunk((byte)Chunk.Opcode.OP_CONSTANT, 123);
            chunk.WriteChunk((byte)constant, 123);
            chunk.WriteChunk((byte)Chunk.Opcode.OP_RETURN, 123);

            // For displaying in the test runner.
            Debugging.DisassembleChunk(chunk, "test chunk");

            string output;
            var expected = "== test chunk ==\r\n0000  123 OP_CONSTANT         0 '1.2'\r\n0002    | OP_RETURN\r\n";

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                Debugging.DisassembleChunk(chunk, "test chunk");
                output = sw.ToString();
                Assert.That(output, Is.EqualTo(expected));
            }
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));
        }
    }
}
