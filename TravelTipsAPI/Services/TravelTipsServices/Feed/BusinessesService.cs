using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Constants.Enums.AdEnum;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;

namespace TravelTipsAPI.Services.TravelTipsServices.Feed
{
    public class BusinessesService(TravelTipsContext context) : IBusinessesService
    {
        /// <summary>
        /// Find a business by id
        /// </summary>
        /// <param name="businessId">business id</param>
        /// <returns>the business with the id</returns>
        public Business? FindBusinessById(int businessId)
        {
            return context.Businesses.FirstOrDefault(b => b.Id == businessId);
        }

        /// <summary>
        /// Get a list of businesses id by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>a list of business id</returns>
        public IEnumerable<int> GetMyBusinesses(int userId)
        {
            return context.Businesses.Where(b => b.CreatedBy == userId).Select(b => b.Id).ToList();
        }

        /// <summary>
        /// Get a list of businesses by params
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="status">business status</param>
        /// <returns>a list of businesses that fit the params</returns>
        public IEnumerable<BusinessViewModel> GetBusinessesByParams(
            int? userId = null,
            AdStatus? status = null
        )
        {
            if (userId == null && status == null)
            {
                return [];
            }
            var query = context.Businesses.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(b => b.CreatedBy == userId.Value);
            }

            if (status.HasValue)
            {
                var statusStr = GetAdStatusStr(status);
                query = query.Where(b => b.Status == statusStr);
            }

            return query.Select(b => (BusinessViewModel)b).ToList();
        }

        /// <summary>
        /// Create a new business
        /// </summary>
        /// <param name="newBusiness">new business</param>
        /// <param name="userId">user id</param>
        /// <returns>the new business</returns>
        public async Task<BusinessViewModel> PostNewBusiness(
            BusinessPostViewModel newBusiness,
            int userId
        )
        {
            var business = new Business
            {
                Name = newBusiness.Name,
                Website = newBusiness.Website,
                Address = newBusiness.Address,
                Status = GetAdStatusStr(AdStatus.Pending)!,
                CreatedBy = userId,
            };

            context.Businesses.Add(business);
            await context.SaveChangesAsync();

            return (BusinessViewModel)business;
        }

        /// <summary>
        /// Update business details
        /// </summary>
        /// <param name="business">business</param>
        /// <param name="businessPatch">business details to be updated</param>
        /// <returns>updated business</returns>
        public async Task<BusinessViewModel> UpdateBusiness(
            Business business,
            BusinessPatchViewModel businessPatch
        )
        {
            business.Name = businessPatch.Name ?? business.Name;
            business.Website = businessPatch.Website ?? business.Website;
            business.Address = businessPatch.Address ?? business.Address;
            business.Status = GetAdStatusStr(AdStatus.Pending)!;

            await context.SaveChangesAsync();

            return (BusinessViewModel)business;
        }

        /// <summary>
        /// Update the business active status
        /// </summary>
        /// <param name="business">business</param>
        /// <param name="isActive">active status</param>
        /// <returns>the new status</returns>
        public async Task<string> UpdateBusinessActiveStatus(Business business, bool isActive)
        {
            // Only update status if the current status is Active or Inactive
            if (
                business.Status != GetAdStatusStr(AdStatus.Active)
                || business.Status != GetAdStatusStr(AdStatus.Inactive)
            )
            {
                throw new Exception(Messages.BusinessStatusCannotBeUpdated);
            }

            if (business.Status == GetAdStatusStr(AdStatus.Active) && isActive)
            {
                // If the business is already active and the new status is active, do nothing
                return business.Status;
            }

            if (business.Status == GetAdStatusStr(AdStatus.Inactive) && !isActive)
            {
                // If the business is already inactive and the new status is inactive, do nothing
                return business.Status;
            }

            business.Status = isActive
                ? GetAdStatusStr(AdStatus.Active)!
                : GetAdStatusStr(AdStatus.Inactive)!;

            if (business.Status == GetAdStatusStr(AdStatus.Inactive))
            {
                var ads = context
                    .Ads.Where(a =>
                        a.BusinessId == business.Id && a.Status == GetAdStatusStr(AdStatus.Active)
                    )
                    .ToList();

                // If the business is set to inactive, all its active ads will be set to inactive as well
                foreach (var ad in ads)
                {
                    ad.Status = GetAdStatusStr(AdStatus.Inactive)!;
                }
            }

            await context.SaveChangesAsync();
            return business.Status;
        }

        /// <summary>
        /// Update a business status
        /// </summary>
        /// <param name="business">business</param>
        /// <param name="status">new status</param>
        /// <returns>the new status</returns>
        public async Task<string> UpdateBusinessStatus(Business business, AdStatus status)
        {
            var statusStr = GetAdStatusStr(status);
            if (statusStr == null)
                throw new Exception(Messages.BusinessStatusInvalid);

            business.Status = statusStr;
            await context.SaveChangesAsync();

            return business.Status;
        }
    }
}
