using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Services.TravelTipsServices.Feed
{
    public class BannersService(TravelTipsContext context, IImagesService imagesService)
        : IBannersService
    {
        public JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        /// <summary>
        /// Find banner by id, return null if not found
        /// </summary>
        /// <param name="id">banner id</param>
        /// <returns>the banner with the id</returns>
        public Banner? FindBannerById(int id)
        {
            return context.Banners.Find(id);
        }

        /// <summary>
        /// Get a banner view model by id
        /// </summary>
        /// <param name="id">banner id</param>
        /// <returns>the banner view model with the id</returns>
        public async Task<BannerViewModel?> GetBannerViewModelById(int id)
        {
            var bannerData = await context
                .Banners.AsNoTracking()
                .Include(b => b.Styling)
                .Where(b => b.Id == id)
                .Select(b => new
                {
                    ViewModel = new BannerViewModel
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Overview = b.Overview,
                        Link = b.Link,
                        From = b.From,
                        To = b.To,
                        Label = b.Label,
                        SubLabel = b.SubLabel,
                        Styling = b.Styling != null ? (BannerStylingViewModel)b.Styling : null,
                    },
                    b.ImageId,
                })
                .FirstOrDefaultAsync();

            if (bannerData == null)
                return null;

            var images = await imagesService.GetImagesByIds([bannerData.ImageId]);
            bannerData.ViewModel.Picture = images.FirstOrDefault();

            return bannerData.ViewModel;
        }

        /// <summary>
        /// Get a list of public banners
        /// </summary>
        /// <returns>a list of public banners</returns>
        public async Task<IEnumerable<BannerViewModel>> GetPublicBannerViewModels()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var bannerInfo = context
                .Banners.AsNoTracking()
                .Include(b => b.Styling)
                .Where(b => b.From <= today && (b.To == null || b.To >= today))
                .Select(b => new
                {
                    banner = new BannerViewModel
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Overview = b.Overview,
                        Link = b.Link,
                        From = b.From,
                        To = b.To,
                        Label = b.Label,
                        SubLabel = b.SubLabel,
                        Styling = b.Styling != null ? (BannerStylingViewModel)b.Styling : null,
                    },
                    imageId = b.ImageId,
                })
                .ToList();

            var banners = bannerInfo.Select(b => b.banner).ToList();

            if (banners.Count == 0)
                return [];

            var imageIds = bannerInfo.Select(b => b.imageId).Distinct().ToList();
            if (imageIds.Count != 0)
            {
                var images = await imagesService.GetImagesByIds([.. imageIds]);
                var imageMap = images.ToDictionary(i => i.Id);

                banners = bannerInfo
                    .Select(bi =>
                    {
                        var vm = bi.banner; // Get the existing object
                        imageMap.TryGetValue(bi.imageId, out var image);
                        vm.Picture = image;
                        return vm;
                    })
                    .OrderByDescending(b => b.From)
                    .ToList();
            }

            return banners;
        }

        /// <summary>
        /// Get a list of banners with cursor
        /// </summary>
        /// <param name="cursor">general cursor</param>
        /// <param name="limit">limit</param>
        /// <returns>a list of banners</returns>
        public IEnumerable<BannerSimpleViewModel> GetBanners(
            GeneralCursor? cursor = null,
            int? limit = null
        )
        {
            var query = context.Banners.AsQueryable();
            if (cursor != null)
            {
                query = query.Where(b => b.Id < cursor.Id);
            }
            query = query.OrderByDescending(b => b.From);
            if (limit != null)
            {
                query = query.Take(limit.Value);
            }
            return query.Select(b => (BannerSimpleViewModel)b).ToList();
        }

        /// <summary>
        /// Create a new banner
        /// </summary>
        /// <param name="postViewModel">the new banner</param>
        /// <returns>the created banner</returns>
        public async Task<BannerSimpleViewModel> PostNewBanner(BannerPostViewModel postViewModel)
        {
            var banner = new Banner
            {
                Title = postViewModel.Title,
                Overview = postViewModel.Overview,
                ImageId = postViewModel.ImageId,
                Link = postViewModel.Link,
                From = postViewModel.From,
                To = postViewModel.To,
                Label = postViewModel.Label,
                SubLabel = postViewModel.SubLabel,
                StylingId = postViewModel.StylingId,
            };

            context.Banners.Add(banner);
            await context.SaveChangesAsync();

            return (BannerSimpleViewModel)banner;
        }

        /// <summary>
        /// Update a banner
        /// </summary>
        /// <param name="banner">the existing banner</param>
        /// <param name="bannerPatch">the updated banner</param>
        /// <returns></returns>
        public async Task UpdateBanner(Banner banner, BannerPatchViewModel bannerPatch)
        {
            banner.Title = bannerPatch.Title ?? banner.Title;
            banner.Overview = bannerPatch.Overview ?? banner.Overview;
            banner.ImageId = bannerPatch.ImageId ?? banner.ImageId;
            banner.Link = bannerPatch.Link ?? banner.Link;
            banner.From = bannerPatch.From ?? banner.From;
            banner.To = bannerPatch.To ?? banner.To;
            banner.Label = bannerPatch.Label ?? banner.Label;
            banner.SubLabel = bannerPatch.SubLabel ?? banner.SubLabel;
            banner.StylingId = bannerPatch.StylingId ?? banner.StylingId;

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a banner
        /// </summary>
        /// <param name="banner">banner to be deleted</param>
        /// <returns></returns>
        public async Task DeleteBanner(Banner banner)
        {
            context.Banners.Remove(banner);
            await context.SaveChangesAsync();
        }

        // styling

        public BannerStyling? FindBannerStylingById(int id)
        {
            return context.BannerStylings.Find(id);
        }

        public IEnumerable<BannerStylingSimpleViewModel> GetAllBannerStylings()
        {
            return context.BannerStylings.ToList().Select(bs => (BannerStylingSimpleViewModel)bs);
        }

        public async Task<BannerStylingViewModel> PostNewStyling(string name, string styling)
        {
            var bannerStyling = new BannerStyling { Name = name, Styling = styling };

            context.BannerStylings.Add(bannerStyling);
            await context.SaveChangesAsync();

            return (BannerStylingViewModel)bannerStyling;
        }

        public async Task<BannerStylingViewModel> UpdateStyling(
            BannerStyling bannerStyling,
            BannerStylingPatchViewModel bannerStylingPatch
        )
        {
            bannerStyling.Name = bannerStylingPatch.Name ?? bannerStyling.Name;
            bannerStyling.Styling = bannerStylingPatch.Styling ?? bannerStyling.Styling;
            await context.SaveChangesAsync();

            return (BannerStylingViewModel)bannerStyling;
        }

        /// <summary>
        /// validate styling as json object
        /// </summary>
        /// <param name="styling">styling content</param>
        /// <returns>whether the styling is valid or not</returns>
        public bool ValidateStyling(string? styling)
        {
            if (string.IsNullOrEmpty(styling))
                return true;
            try
            {
                using var doc = JsonDocument.Parse(styling);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
