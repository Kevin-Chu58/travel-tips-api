using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using static TravelTipsAPI.Constants.Enums.StripeEnum;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.PlanSchema;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    public class UserExtendsService(
        TravelTipsContext context,
        ISubscriptionsService subscriptionsService
    ) : IUserExtendsService
    {
        public UserSubExtend FindUserSubExtendByUserId(int userId)
        {
            var userSubExtend = context.UserSubExtends.FirstOrDefault(ue => ue.UserId == userId);
            if (userSubExtend == null)
                throw new Exception(Messages.UserNotFound);

            return userSubExtend;
        }

        public async Task<UserSubExtend> GetUpdatedUserSubExtendByUserId(int userId)
        {
            var userSubExtend = FindUserSubExtendByUserId(userId);

            // if userSubExtend doesn't exist, create a new one with default values
            //if (userSubExtend == null)
            //{
            //    var newUserSubExtend = new UserSubExtend
            //    {
            //        UserId = userId,
            //        CycleStart = null,
            //        MonthIndex = null,
            //        PdfDownloadCount = 0,
            //        TripCount = 0,
            //        MaxPdfDownloadCount = Global.MAX_PDF_GENERATION_PER_MONTH, // default limit for non-subscribed users
            //        MaxTripCount = Global.MAX_TRIPS, // default limit for non-subscribed users
            //    };
            //    context.UserSubExtends.Add(newUserSubExtend);
            //    await context.SaveChangesAsync();

            //    userSubExtend = newUserSubExtend;
            //}

            var now = DateTimeOffset.UtcNow;
            var activeSub = subscriptionsService.FindActiveSubscriptionByUserId(userId);

            if (activeSub != null && activeSub.End > now)
            {
                // if user has an active subscription, update the cycle start and month index
                var subStart = activeSub.Start;
                var monthIndex =
                    (now.Year - activeSub.Start.Year) * 12 + now.Month - activeSub.Start.Month;

                // If they started on the 31st but it's February, the anniversary is the 28th (or 29th).
                int daysInCurrentMonth = DateTime.DaysInMonth(now.Year, now.Month);
                int effectiveAnniversaryDay = Math.Min(subStart.Day, daysInCurrentMonth);

                // If we haven't reached the start day yet this month, roll back the index
                if (now.Day < effectiveAnniversaryDay)
                    monthIndex--;

                // Ensure we don't return a negative index (e.g., if now < subStart due to clock drift)
                monthIndex = Math.Max(0, monthIndex);

                if (userSubExtend.MonthIndex == monthIndex)
                    return userSubExtend; // no update needed

                userSubExtend = await UpdateSubExtendCycle(userSubExtend, subStart, monthIndex);
            }

            return userSubExtend;
        }

        public async Task<UserSubExtend> UpdateSubExtendCycle(
            UserSubExtend userSubExtend,
            DateTimeOffset? subStart,
            int? monthIndex,
            SubscriptionEnum? subscription = null
        )
        {
            userSubExtend.CycleStart =
                subStart != null && monthIndex != null
                    ? subStart.Value.AddMonths((int)monthIndex)
                    : null;
            userSubExtend.MonthIndex = monthIndex;

            userSubExtend.PdfDownloadCount = 0; // reset pdf download count when new cycle starts

            if (subStart != null && subscription != null)
            {
                switch (subscription)
                {
                    case SubscriptionEnum.MonthlyMember:
                    case SubscriptionEnum.ThreeMonthMember:
                    case SubscriptionEnum.SixMonthMember:
                    case SubscriptionEnum.YearlyMember:
                        userSubExtend.MaxPdfDownloadCount =
                            Global.MAX_PDF_GENERATION_PER_MONTH_MEMBER;
                        userSubExtend.MaxTripCount = Global.MAX_TRIPS_MEMBER;
                        break;
                }
            }
            else
            {
                userSubExtend.MaxPdfDownloadCount = Global.MAX_PDF_GENERATION_PER_MONTH; // default limit for non-subscribed users
                userSubExtend.MaxTripCount = Global.MAX_TRIPS; // default limit for non-subscribed users
            }

            await context.SaveChangesAsync();

            return userSubExtend;
        }

        public async Task UpdateSubExtendNewTripPdf(UserSubExtend userSubExtend)
        {
            userSubExtend.PdfDownloadCount += 1;
            await context.SaveChangesAsync();
        }

        public async Task UpdateSubExtendTripCount(UserSubExtend userSubExtend, int increment)
        {
            userSubExtend.TripCount += increment;
            await context.SaveChangesAsync();
        }
    }
}
