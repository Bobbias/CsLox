using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// A utility class containing functions related to loading test fixture files.
    /// </summary>
    internal static class FixtureFileLoader
    {
        /// <summary>
        /// Searches in <paramref name="fixtureDir"/> for <paramref name="fileName"/>, and if found, returns it's contents as a string.
        /// </summary>
        /// <param name="fixtureDir">A directory within the Tests project.</param>
        /// <param name="fileName">The file to load, including the extension if present.</param>
        /// <returns>A <see langword="string"/> containing the contents of the file.</returns>
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
