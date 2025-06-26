namespace TravelTipsAPI.Constants
{
    public class TypeEnums
    {
        public static class OsmTypes
        {
            public const string Node = "node";
            public const string Way = "way";
            public const string Relation = "relation";

            public static readonly string[] All = [Node, Way, Relation];
        }
    }
}
