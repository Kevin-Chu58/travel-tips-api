namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripDetailViewModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public UserViewModel? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public required IEnumerable<DayViewModel> Days { get; set; }
    }
}
