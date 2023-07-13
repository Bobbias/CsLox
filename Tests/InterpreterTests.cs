namespace Tests
{
    public class InterpreterTests
    {
        private string printsSource;

        [SetUp]
        public void Setup()
        {

            printsSource = FixtureFileLoader.LoadFileToString("Scripts", "prints.lox");
        }

        [Test]
        public void PrintsScript()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                CLI.Run(printsSource);

                var expected = "one\r\nTrue\r\n3\r\n";
                Assert.That(sw.ToString(), Is.EqualTo(expected));
            }
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));
        }
    }
}