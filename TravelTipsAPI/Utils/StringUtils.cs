using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace TravelTipsAPI.Utils
{
    public class StringUtils
    {
        public static string StrToSlug(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            // 1. Convert to lowercase
            text = text.ToLowerInvariant();

            // 2. Replace accented characters with ASCII equivalents
            text = RemoveDiacritics(text);

            // 3. Remove invalid characters (keep letters, numbers, spaces)
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");

            // 4. Replace multiple spaces or dashes with a single dash
            text = Regex.Replace(text, @"[\s-]+", "-");

            // 5. Trim leading/trailing dashes
            text = text.Trim('-');

            return text;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
