using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class UserSubExtendViewModel
    {
        public int UserId { get; set; }

        public DateTimeOffset? CycleStart { get; set; }

        public int? MonthIndex { get; set; }

        public int PdfDownloadCount { get; set; }

        public int TripCount { get; set; }

        public int MaxPdfDownloadCount { get; set; }

        public int MaxTripCount { get; set; }

        public static explicit operator UserSubExtendViewModel?(UserSubExtend? userSubExtend)
        {
            if (userSubExtend == null)
            {
                return null;
            }

            return new UserSubExtendViewModel
            {
                UserId = userSubExtend.UserId,
                CycleStart = userSubExtend.CycleStart,
                MonthIndex = userSubExtend.MonthIndex,
                PdfDownloadCount = userSubExtend.PdfDownloadCount,
                TripCount = userSubExtend.TripCount,
                MaxPdfDownloadCount = userSubExtend.MaxPdfDownloadCount,
                MaxTripCount = userSubExtend.MaxTripCount,
            };
        }
    }
}
