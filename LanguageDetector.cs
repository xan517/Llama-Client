// LanguageDetector.cs
using System.Text.RegularExpressions;

namespace WindowsFormsApp1
{
    public static class LanguageDetector
    {
        /// <summary>
        /// Detects the programming language of a given code snippet using regex patterns.
        /// </summary>
        /// <param name="code">The code snippet to analyze.</param>
        /// <returns>The detected language as a string. Returns "plaintext" if no match is found.</returns>
        public static string DetectLanguage(string code)
        {
            // Trim leading/trailing whitespace
            code = code.Trim();

            // Define regex patterns for different languages
            var languagePatterns = new (string Language, string Pattern)[]
            {
                ("python", @"^\s*def\s+\w+\s*\("),
                ("csharp", @"^\s*(public|private|protected|internal)?\s*(class|struct|interface)\s+\w+"),
                ("ruby", @"^\s*def\s+\w+\s*(\(|$)"),
                ("javascript", @"^\s*function\s+\w+\s*\("),
                ("cpp", @"^\s*#include\s+<\w+\.h>"),
                ("php", @"^\s*<\?php"),
                ("java", @"^\s*public\s+class\s+\w+"),
                ("go", @"^\s*func\s+\w+\s*\("),
                ("typescript", @"^\s*function\s+\w+\s*\("),
                ("swift", @"^\s*func\s+\w+\s*\("),
                ("kotlin", @"^\s*fun\s+\w+\s*\("),
                ("plaintext", @"^") // Default to plaintext
            };

            foreach (var (Language, Pattern) in languagePatterns)
            {
                if (Regex.IsMatch(code, Pattern, RegexOptions.Multiline))
                {
                    return Language;
                }
            }

            return "plaintext"; // Fallback
        }
    }
}
