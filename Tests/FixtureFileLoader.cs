using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    internal static class FixtureFileLoader
    {
        public static string LoadFileToString(string fixtureDir, string fileName)
        {
            var runningDir = TestContext.CurrentContext.TestDirectory;
            var projectDir = Directory.GetParent(runningDir)!.Parent!.Parent!.FullName;

            var fullFilePath = Path.Join(projectDir, fixtureDir, fileName);

            return File.ReadAllText(
                fullFilePath
            );
        }
    }
}
