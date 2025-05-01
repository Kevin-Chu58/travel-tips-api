namespace TravelTipsAPI.Utils
{
    public class UrlUtils
    {
        public static bool IsValidUrlFormat(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (
                    uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps
                );
        }
    }
}
