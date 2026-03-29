using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_image;
using TravelTipsAPI.ViewModels.db_search;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripViewModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public UserSimpleViewModel? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
        public bool IsHidden { get; set; }
        public bool IsBookmarked { get; set; }
        public int? Budget { get; set; }
        public int? NumDays { get; set; }
        public int BookmarkCount { get; set; }
        public RegionCompleteViewModel? Region { get; set; }
        public IEnumerable<ImageViewModel>? Images { get; set; }
        public IEnumerable<UserSimpleViewModel>? SharedUsers { get; set; }
        public bool IsReadonly { get; set; }

        public static explicit operator TripViewModel(Trip trip)
        {
            return new TripViewModel
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                CreatedAt = trip.CreatedAt,
                IsPublic = trip.IsPublic,
                IsHidden = trip.IsHidden,
                IsBookmarked = false,
                Budget = trip.Budget,
                BookmarkCount = trip.BookmarkCount,
                IsReadonly = true,
            };
        }
    }
}
