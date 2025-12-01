namespace TravelTipsAPI.Services.UtilServices
{
    public class UtilSchema
    {
        public interface ISpellCheckerService
        {
            string? GetBestSuggestion(string input);
            string CorrectSentence(string input);
        }
    }
}
