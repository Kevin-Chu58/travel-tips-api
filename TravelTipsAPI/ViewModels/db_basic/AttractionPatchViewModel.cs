namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionPatchViewModel
    {
        // attractions
        public long OsmId { get; set; }
        public required string OsmType { get; set; }
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }

        // highlights
        public string? Description { get; set; }
        public int? LinkId { get; set; }

        public static explicit operator AttractionViewModel(
            AttractionPatchViewModel attractionPatch
        )
        {
            var attractionViewModel = new AttractionViewModel
            {
                OsmId = attractionPatch.OsmId,
                OsmType = attractionPatch.OsmType,
                Lng = attractionPatch.Lng,
                Lat = attractionPatch.Lat,
                Name = attractionPatch.Name,
                Address = attractionPatch.Address,
            };

            return attractionViewModel;
        }
    }
}
