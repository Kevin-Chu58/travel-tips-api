namespace TravelTipsAPI.ViewModels.HereMap
{
    public class HereDiscoverResponse
    {
        public List<HerePlace> Items { get; set; } = [];
    }

    public class HerePlace
    {
        public string Title { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string ResultType { get; set; } = string.Empty;
        public required HereAddress Address { get; set; }
        public required HerePosition Position { get; set; }
        public List<HerePosition>? Access { get; set; }
        public int? Distance { get; set; }
        public List<HereCategory>? Categories { get; set; }
        public List<HereReference>? References { get; set; }
        public List<HereContact>? Contacts { get; set; }
        public List<HereOpeningHours>? OpeningHours { get; set; }
        public List<HereFoodTypes>? FoodTypes { get; set; }
    }

    public class HereAddress
    {
        public required string Label { get; set; }
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public string? StateCode { get; set; }
        public string? State { get; set; }
        public string? County { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Street { get; set; }
        public string? PostalCode { get; set; }
        public string? HouseNumber { get; set; }
    }

    public class HerePosition
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class HereCategory
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool? Primary { get; set; }
    }

    public class HereReference
    {
        public HereSupplier? Supplier { get; set; }
        public string Id { get; set; } = string.Empty;
    }

    public class HereSupplier
    {
        public string Id { get; set; } = string.Empty; // "core", "tripadvisor", "yelp", etc.
    }

    public class HereContact
    {
        public List<HereValue>? Phone { get; set; }
        public List<HereValue>? Www { get; set; }
    }

    public class HereOpeningHours
    {
        public List<string>? Text { get; set; }
    }

    public class HereFoodTypes
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool? Primary { get; set; }
    }

    public class HereValue
    {
        public string Value { get; set; } = string.Empty;
    }
}
