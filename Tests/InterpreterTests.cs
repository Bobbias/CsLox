namespace Tests
{
    public class InterpreterTests
    {
        private string printsSource;
        private string variablesSource;
        private string shadowingSource;

        [SetUp]
        public void Setup()
        {

            printsSource = FixtureFileLoader.LoadFileToString("Scripts", "prints.lox");
            variablesSource = FixtureFileLoader.LoadFileToString("Scripts", "variables.lox");
            shadowingSource = FixtureFileLoader.LoadFileToString("Scripts", "shadowing.lox");
        }

        [Test]
        public void PrintsScript()
        {
            var expected = "one\r\nTrue\r\n3\r\n";

            ScriptRunner.TestScriptOutput(printsSource, expected);
        }

        [Test]
        public void PrintsVariables()
        {    
            var expected = "3\r\n";

            ScriptRunner.TestScriptOutput(variablesSource, expected);
        }

        [Test]
        public void Shadowing()
        {
            var expected = "inner a\r\nouter b\r\nglobal c\r\nouter a\r\nouter b\r\nglobal c\r\nglobal a\r\nglobal b\r\nglobal c\r\n";

            ScriptRunner.TestScriptOutput(shadowingSource, expected);
        }
    }
}