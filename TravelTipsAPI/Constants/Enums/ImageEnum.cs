namespace TravelTipsAPI.Constants.Enums
{
    public class ImageEnum
    {
        public enum ImageType
        {
            Banner,
            Business,
            Ad,
        };

        public static string? GetImageTypeStr(ImageType? type)
        {
            switch (type)
            {
                case ImageType.Banner:
                    return "banner";
                case ImageType.Business:
                    return "business";
                case ImageType.Ad:
                    return "ad";
                default:
                    return null;
            }
        }
    }
}
