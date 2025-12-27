using System.Text.RegularExpressions;

namespace University_Portal.Helpers
{
    public static class RemoveHtmlTagHelper
    {
        // Ta klasa pomocnicza służy do usuwania znaczników HTML z treści wyświetlanej w widoku Post/Index.cshtml
        public static string RemoveHtmlTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var withoutTags = Regex.Replace(
                input,
                "<.*?>",
                string.Empty,
                RegexOptions.Singleline
            );

            return withoutTags;
        }
    }
}
