using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// A static class containing helper functions related to running test Lox script files.
    /// </summary>
    internal static class ScriptRunner
    {
        /// <summary>
        /// Runs the <paramref name="scriptSource"/> while logging the console output, and compares it against <paramref name="expectedOutput"/>.
        /// </summary>
        /// <param name="scriptSource">The source code for the script being tested.</param>
        /// <param name="expectedOutput">The expected output to be printed after running the test.</param>
        public static void TestScriptOutput(string scriptSource, string expectedOutput)
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                CLI.Run(scriptSource);

                Assert.That(sw.ToString(), Is.EqualTo(expectedOutput));
            }
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));
        }
    }
}
