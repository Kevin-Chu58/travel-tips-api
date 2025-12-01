namespace TravelTipsAPI.Services.UtilServices
{
    using System.Collections.Generic;
    using WeCantSpell.Hunspell;
    using static TravelTipsAPI.Services.UtilServices.UtilSchema;

    public class SpellCheckerService : ISpellCheckerService
    {
        private readonly WordList _dictionary;
        private readonly string fileLocation = "Dictionaries/";

        public SpellCheckerService()
        {
            _dictionary = WordList.CreateFromFiles(
                fileLocation + "en-US.dic",
                fileLocation + "en-US.aff"
            );
        }

        /// <summary>
        /// Correct the word if it is NOT valid
        /// </summary>
        /// <param name="input">user input word</param>
        /// <returns>corrected word</returns>
        public string? GetBestSuggestion(string input)
        {
            var suggestions = _dictionary.Suggest(input).ToList();

            if (suggestions == null || suggestions.Count == 0)
                return null;

            return suggestions[0]; // best-ranked suggestion
        }

        /// <summary>
        /// Correct only the words that are NOT valid
        /// </summary>
        /// <param name="input">user input</param>
        /// <returns>corrected user input</returns>
        public string CorrectSentence(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var correctedWords = new List<string>(words.Length);

            foreach (var word in words)
            {
                // if correct → keep
                if (_dictionary.Check(word))
                {
                    correctedWords.Add(word);
                    continue;
                }

                // Otherwise, try to correct it
                var corrected = GetBestSuggestion(word);

                if (!string.IsNullOrEmpty(corrected))
                    correctedWords.Add(corrected);
                else
                    correctedWords.Add(word); // fallback if no suggestions
            }

            return string.Join(" ", correctedWords);
        }
    }
}
