namespace TravelTipsAPI.ViewModels.YouTube
{
    public class YouTubeVideo
    {
        public required string Title { get; set; }
        public required string VideoId { get; set; }
        public required string ChannelId { get; set; }
        public required string ThumbnailUrl { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}
