namespace TravelTipsAPI.Constants.Enums
{
    public class ImageEnum
    {
        public enum ImageType
        {
            Banner,
            Ad,
        };

        public static string? GetImageTypeStr(ImageType? type)
        {
            switch (type)
            {
                case ImageType.Banner:
                    return "Banner";
                case ImageType.Ad:
                    return "Ad";
                default:
                    return null;
            }
        }
    }
}
