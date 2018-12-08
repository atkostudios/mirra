using System;
using System.Text.RegularExpressions;

namespace Utility
{
    static class StringUtility
    {
        static Regex MultipleSpacesExpression { get; } = new Regex(@"\s\s+", RegexOptions.Compiled);


        public static string SubstringAfter(this string content, string substring)
        {
            var index = content.IndexOf(substring, StringComparison.Ordinal);
            if (index < 0)
            {
                return content;
            }

            return content.Substring(index, content.Length - index);
        }

        public static string SubstringAfterLast(this string content, string substring)
        {
            var index = content.LastIndexOf(substring, StringComparison.Ordinal);
            if (index < 0)
            {
                return content;
            }

            return content.Substring(index + 1, content.Length - index - 1);
        }

        public static string Paragraph(this string content)
        {
            return MultipleSpacesExpression.Replace(content
                .Replace('\n', ' ')
                .Replace('\t', ' ')
                .Trim(), " ");
        }
    }
}
