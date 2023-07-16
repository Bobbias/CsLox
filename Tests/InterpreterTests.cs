namespace Tests
{
    /// <summary>
    /// Contains all the tests for the <see cref="CsLox.Interpreter"/>. These are effectively integration tests since they
    /// effectively test a large portion of the code base at once. These are mainly to avoid bugs and regressions during
    /// development.
    /// </summary>
    public class InterpreterTests
    {
        private string printsSource;
        private string variablesSource;
        private string shadowingSource;
        private string fibSource;

        /// <summary>
        /// Loads all the scripts into strings so they can be run later.
        /// </summary>
        [OneTimeSetUp]
        public void SetupFiles()
        {
            printsSource = FixtureFileLoader.LoadFileToString("Scripts", "prints.lox");
            variablesSource = FixtureFileLoader.LoadFileToString("Scripts", "variables.lox");
            shadowingSource = FixtureFileLoader.LoadFileToString("Scripts", "shadowing.lox");
            fibSource = FixtureFileLoader.LoadFileToString("Scripts", "fib.lox");
        }

        /// <summary>
        /// Ensures that no variables remain in the static root interpreter environment between tests.
        /// </summary>
        [SetUp]
        public void BeforeEach()
        {
            CLI.Interpreter.ClearEnvironment();
        }

        /// <summary>
        /// Runs prints.lox, which tests the print statement with several different types. A string, the boolean value true,
        /// and the result of the expression '2 + 1' which should be the number 3.
        /// </summary>
        [Test]
        public void PrintsScript()
        {
            var expected = "one\r\nTrue\r\n3\r\n";

            ScriptRunner.TestScriptOutput(printsSource, expected);
        }

        /// <summary>
        /// Runs variables.lox, which should add 2 numbers together and print the result.
        /// </summary>
        [Test]
        public void PrintsVariables()
        {    
            var expected = "3\r\n";

            ScriptRunner.TestScriptOutput(variablesSource, expected);
        }

        /// <summary>
        /// Runs shadowing.lox, a simple script to demonstrate variable shadowing in different scopes.
        /// </summary>
        [Test]
        public void Shadowing()
        {
            var expected = "inner a\r\nouter b\r\nglobal c\r\nouter a\r\nouter b\r\nglobal c\r\nglobal a\r\nglobal b\r\nglobal c\r\n";

            ScriptRunner.TestScriptOutput(shadowingSource, expected);
        }

        /// <summary>
        /// Runs fib.lox, which should generate the first 21 numbers in the Fibonacci sequence.
        /// </summary>
        [Test]
        public void Fib()
        {
            var expected = "0\r\n1\r\n1\r\n2\r\n3\r\n5\r\n8\r\n13\r\n21\r\n34\r\n55\r\n89\r\n144\r\n233\r\n377\r\n610\r\n987\r\n1597\r\n2584\r\n4181\r\n6765\r\n";

            ScriptRunner.TestScriptOutput(fibSource, expected);
        }
    }
}