using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateAst
{
    /// <summary>
    /// Extension methods for <see langword="string"/>.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Converts the first character of the given <see langword="string"/> to upper case, leaving the rest of the string untouched.
        /// </summary>
        /// <param name="input">The <see langword="string"/> to be modified.</param>
        /// <returns>A <see langword="string"/> with the first character converted to upper case.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="input"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="input"/> the empty string.</exception>
        public static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };
    }
}
