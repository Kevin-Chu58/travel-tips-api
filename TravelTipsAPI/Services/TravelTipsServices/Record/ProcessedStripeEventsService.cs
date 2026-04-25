using TravelTipsAPI.Models.TravelTipsModels;
using static TravelTipsAPI.Services.TravelTipsServices.RecordsSchema;

namespace TravelTipsAPI.Services.TravelTipsServices.Record
{
    public class ProcessedStripeEventsService(TravelTipsContext context)
        : IProcessedStripeEventsService
    {
        /// <summary>
        /// Check if a processed event with the given stripe event id already exists in the database
        /// </summary>
        /// <param name="stripeEventId">stripe event id</param>
        /// <returns>true if exists, false otherwise</returns>
        public bool DoesProcessedEventExist(string stripeEventId)
        {
            return context.ProcessedStripeEvents.Any(e => e.StripeEventId == stripeEventId);
        }

        /// <summary>
        /// Add a new processed event
        /// </summary>
        /// <param name="stripeEventId">stripe event id</param>
        /// <returns></returns>
        public async Task AddProcessedEvent(string stripeEventId)
        {
            var newProcessedEvent = new ProcessedStripeEvent
            {
                StripeEventId = stripeEventId,
                ProcessedAt = DateTime.UtcNow,
            };

            context.ProcessedStripeEvents.Add(newProcessedEvent);
            await context.SaveChangesAsync();
        }
    }
}
