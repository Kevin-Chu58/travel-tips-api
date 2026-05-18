using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Services.TravelTipsServices;

namespace TravelTipsAPI.Controllers.TravelTips
{
    [Route("api/[controller]")]
    public class SeoController(ISeoService seoService) : TravelTipsControllerBase
    {
        // sitemap

        [HttpGet]
        [Route("sitemap.xml")]
        [AllowAnonymous]
        public IActionResult GetSitemap()
        {
            var sitemapXml = seoService.GenerateSitemapXml();
            return Content(sitemapXml, "application/xml");
        }

        // html

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public IActionResult GetSeo()
        {
            var html = seoService.GenerateHomePageHtml();
            return Content(html, "text/html");
        }
    }
}
