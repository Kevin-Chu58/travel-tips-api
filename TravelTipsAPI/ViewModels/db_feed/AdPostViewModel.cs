namespace TravelTipsAPI.ViewModels.db_feed
{
    public class AdPostViewModel
    {
        public int ImageId { get; set; }
        public required string Title { get; set; }
        public string? Text { get; set; }
        public string? ButtonLabel { get; set; }
        public string? Link { get; set; }
    }
}
