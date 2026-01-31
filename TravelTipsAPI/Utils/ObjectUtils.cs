using System.Text;
using System.Text.Json;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Utils
{
    public class ObjectUtils
    {
        public static string ExtractJsonArray(string raw)
        {
            int start = raw.IndexOf('[');
            int end = raw.LastIndexOf(']');

            if (start < 0 || end < 0 || end <= start)
                throw new Exception("No JSON array found.");

            return raw.Substring(start, end - start + 1);
        }

        /// encode/decode cursors

        public static string EncodeCursor<T>(T cursor)
        {
            // Serialize to JSON
            var json = JsonSerializer.Serialize(cursor);

            // Convert JSON string to Base64
            var bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }

        public static T? DecodeCursor<T>(string cursor)
        {
            if (string.IsNullOrEmpty(cursor))
                return default;

            try
            {
                // Base64 → JSON string
                var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));

                // JSON → TripCursor object
                var cursorObj = JsonSerializer.Deserialize<T>(jsonString);
                return cursorObj;
            }
            catch
            {
                // Invalid cursor format
                return default;
            }
        }
    }
}
