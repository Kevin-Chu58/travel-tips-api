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
    }
}
