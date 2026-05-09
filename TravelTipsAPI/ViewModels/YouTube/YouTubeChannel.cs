namespace TravelTipsAPI.ViewModels.YouTube
{
    public class YouTubeChannel
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required string ThumbnailUrl { get; set; }
        public required string UploadsPlaylistId { get; set; }
        public string? CustomUrl { get; set; }
        public IEnumerable<YouTubeVideo>? Videos { get; set; }
    }
}
