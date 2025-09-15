namespace TravelTipsAPI.ViewModels.HereMap
{
    public class HereRoutingInput
    {
        public required string TransportMode { get; set; }
        public double OriginLat { get; set; }
        public double OriginLng { get; set; }
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
    }

    public class HereRouting
    {
        public required HerePosition Position { get; set; }
        public required string TransportMode { get; set; }
    }

    public class HereRoutingResponse
    {
        public Notice[]? Notices { get; set; }
        public Route[]? Routes { get; set; }
    }

    public class Notice
    {
        public string? Title { get; set; }
        public string? Code { get; set; }
        public string? Severity { get; set; }
    }

    public class Route
    {
        public string? Id { get; set; }
        public Notice[]? Notices { get; set; }
        public List<Section>? Sections { get; set; }
    }

    public class Section
    {
        public string? Id { get; set; }
        public string? Type { get; set; }

        public List<RouteAction>? PreActions { get; set; }
        public List<RouteAction>? PostActions { get; set; }
        public List<RouteAction>? Actions { get; set; }
        public RouteEvent? Departure { get; set; }
        public RouteEvent? Arrival { get; set; }
        public RouteSummary? Summary { get; set; }
        public RouteSummary? TravelSummary { get; set; }
        public string? Polyline { get; set; }
        public Notice[]? Notices { get; set; }
        public RouteTransport? Transport { get; set; }
        public RouteIntermediateStop[]? IntermediateStops { get; set; }
        public RouteAgency? Agency { get; set; }
        public RouteAttribution[]? RouteAttributions { get; set; }
        public RouteIncident[]? RouteIncidents { get; set; }
    }

    public class RouteAction
    {
        public string? Action { get; set; }
        public int Duration { get; set; }
        public string? Instruction { get; set; }
        public int Offset { get; set; }
        public string? Direction { get; set; }
    }

    public class RouteEvent
    {
        public string? Time { get; set; }
        public RoutePlace? Place { get; set; }
    }

    public class RoutePlace
    {
        public string? Type { get; set; }
        public RouteLocation? Location { get; set; }
        public int? Delay { get; set; }
        public string? Status { get; set; }
    }

    public class RouteLocation
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class RouteSummary
    {
        public int Duration { get; set; }
        public int Length { get; set; }
    }

    public class RouteTransport
    {
        public string? Mode { get; set; }
        public string? Name { get; set; }
        public string? Headsign { get; set; }
        public string? Category { get; set; }
        public string? Color { get; set; }
        public string? TextColor { get; set; }
        public string? WheelchairAccessible { get; set; }
    }

    public class RouteIntermediateStop
    {
        public RouteEvent? Departure { get; set; }
    }

    public class RouteAgency
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Website { get; set; }
    }

    public class RouteAttribution
    {
        public string? Id { get; set; }
        public string? Text { get; set; }
    }

    public class RouteIncident
    {
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
    }
}
