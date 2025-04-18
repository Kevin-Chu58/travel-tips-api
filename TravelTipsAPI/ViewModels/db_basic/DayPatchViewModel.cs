namespace TravelTipsAPI.ViewModels.db_basic
{
    public class DayPatchViewModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public TimeOnly? Start {  get; set; }
        public TimeOnly? End { get; set; }
    }
}
