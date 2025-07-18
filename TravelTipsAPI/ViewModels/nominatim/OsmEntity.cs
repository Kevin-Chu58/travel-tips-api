namespace TravelTipsAPI.ViewModels.nominatim
{
    public class OsmEntity
    {
        public long Place_id { get; set; }
        public string? License { get; set; }
        public required string Osm_type { get; set; }
        public long Osm_id { get; set; }
        public required string Lat { get; set; }
        public required string Lon { get; set; }
        public string? Class { get; set; }
        public string? Type { get; set; }
        public int? Place_rank { get; set; }
        public double? Importance { get; set; }
        public string? Addresstype { get; set; }
        public string? Name { get; set; }
        public required string Display_name { get; set; }
        public Dictionary<string, string>? Extratags { get; set; }
        public List<string>? BoundingBox { get; set; }
    }
}
