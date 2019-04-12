using System;
using System.Text.RegularExpressions;

namespace Atko.Mirra.Utilities
{
    static class StringUtility
    {
        static Regex MultipleSpacesExpression { get; } = new Regex(@"\s\s+", RegexOptions.Compiled);

        public static string SubstringAfterLast(this string content, string substring)
        {
            var index = content.LastIndexOf(substring, StringComparison.Ordinal);
            if (index < 0)
            {
                return content;
            }

            return content.Substring(index + 1, content.Length - index - 1);
        }
    }
}