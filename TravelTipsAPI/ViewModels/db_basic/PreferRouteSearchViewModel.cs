namespace TravelTipsAPI.ViewModels.db_basic
{
    public class PreferRouteSearchViewModel
    {
        public int TimeStamp { get; set; }
        public required IEnumerable<PreferRouteViewModel> PreferRoutes { get; set; }
    }
}
