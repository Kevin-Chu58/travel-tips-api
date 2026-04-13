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
            return type switch
            {
                ImageType.Banner => "banner",
                ImageType.Business => "business",
                ImageType.Ad => "ad",
                _ => null,
            };
        }
    }
}
