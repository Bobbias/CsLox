using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    internal static class ScriptRunner
    {
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
