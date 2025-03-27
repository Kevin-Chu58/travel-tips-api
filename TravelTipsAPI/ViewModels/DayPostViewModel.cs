namespace TravelTipsAPI.ViewModels
{
    public class DayPostViewModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public TimeOnly Start {  get; set; }
        public TimeOnly End { get; set; }
        public int TripId { get; set; }
    }
}
