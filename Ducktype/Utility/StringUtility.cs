using System.Text.RegularExpressions;

namespace Utility
{
    static class StringUtility
    {
        static Regex MultipleSpacesExpression { get; } = new Regex(@"\s\s+", RegexOptions.Compiled);

        public static string Paragraph(this string content)
        {
            return MultipleSpacesExpression.Replace(content
                .Replace('\n', ' ')
                .Replace('\t', ' ')
                .Trim(), " ");
        }
    }
}
