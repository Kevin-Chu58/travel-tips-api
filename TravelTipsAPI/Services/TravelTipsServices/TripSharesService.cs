using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    public class TripSharesService(TravelTipsContext context) : ITripSharesService
    {
        /// <summary>
        /// Find a trip share by trip id and user id
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <param name="userId">user id</param>
        /// <returns>the trip share</returns>
        public TripShare? FindTripShare(int tripId, int userId)
        {
            var tripShare = context.TripShares.FirstOrDefault(ts =>
                ts.TripId == tripId && ts.ShareWith == userId
            );
            return tripShare;
        }

        /// <summary>
        /// Fina a list of trip shares by trip id
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <returns>a list of trip shares</returns>
        public IEnumerable<TripShare> FindTripSharesByTripId(int tripId)
        {
            var tripShares = context.TripShares.Where(ts => ts.TripId == tripId).ToList();

            return tripShares;
        }

        /// <summary>
        /// Get a list of trip ids shared with the user
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>a list of trip ids</returns>
        public IEnumerable<int> GetSharedTripIdsByUserId(int userId)
        {
            var tripIds = context
                .TripShares.Where(ts => ts.ShareWith == userId)
                .Select(ts => ts.TripId)
                .Distinct()
                .ToList();
            return tripIds;
        }

        /// <summary>
        /// Get a list of user ids shared on the trip
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <returns>a list of user ids</returns>
        public IEnumerable<int> GetSharedUserIdsByTripId(int tripId)
        {
            var userIds = context
                .TripShares.Where(ts => ts.TripId == tripId)
                .Select(ts => ts.ShareWith)
                .Distinct()
                .ToList();
            return userIds;
        }

        /// <summary>
        /// Check if a trip is shared with a user
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <param name="userId">user id</param>
        /// <returns>whether trip is shared with user</returns>
        public bool IsTripSharedWithUser(int tripId, int userId)
        {
            var tripShare = FindTripShare(tripId, userId);
            return tripShare != null;
        }

        /// <summary>
        /// Share a trip with a user
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <param name="userId">user id</param>
        /// <returns></returns>
        public async Task ShareTripWithUser(int tripId, int userId)
        {
            var tripShare = FindTripShare(tripId, userId);

            if (tripShare != null)
                throw new Exception(Messages.TripAlreadyShared);

            var newTripShare = new TripShare { TripId = tripId, ShareWith = userId };

            context.TripShares.Add(newTripShare);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Unshare a trip with a user
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <param name="userId">user id</param>
        /// <returns></returns>
        public async Task UnshareTripWithUser(int tripId, int userId)
        {
            var tripShare = FindTripShare(tripId, userId);

            if (tripShare is null)
                throw new Exception(Messages.TripShareNotFound);

            context.Remove(tripShare);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Unshare a trip with all shared users
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <returns></returns>
        public async Task<int> UnshareTripWithAll(int tripId)
        {
            var tripShares = FindTripSharesByTripId(tripId);

            context.RemoveRange(tripShares);
            await context.SaveChangesAsync();

            return tripShares.Count();
        }
    }
}
