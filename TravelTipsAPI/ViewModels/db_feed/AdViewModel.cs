using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class AdViewModel
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string? BusinessName { get; set; }
        public required string Title { get; set; }
        public int ImageId { get; set; }
        public string? Text { get; set; }
        public string? LinkLabel { get; set; }
        public string? Link { get; set; }
        public string? Picture { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public string? StripeItemId { get; set; }
        public string? SubStatus { get; set; }
        public required string Status { get; set; }
        public int TemplateId { get; set; }
        public bool RenewSub { get; set; }

        public static explicit operator AdViewModel(Ad ad)
        {
            return new AdViewModel
            {
                Id = ad.Id,
                BusinessId = ad.BusinessId,
                ImageId = ad.ImageId,
                Title = ad.Title,
                Text = ad.Text,
                LinkLabel = ad.LinkLabel,
                Link = ad.Link,
                StripeSubscriptionId = ad.StripeSubscriptionId,
                StripeItemId = ad.StripeItemId,
                SubStatus = ad.SubStatus,
                Status = ad.Status,
                TemplateId = ad.TemplateId,
                RenewSub = ad.RenewSub,
            };
        }
    }
}
