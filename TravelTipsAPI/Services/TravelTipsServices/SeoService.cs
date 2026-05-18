using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    public class SeoService(TravelTipsContext context) : ISeoService
    {
        // sitemap

        public string GenerateSitemapXml()
        {
            var publicTrips = context
                .Trips.Where(t => t.IsPublic)
                .Select(t => new { t.Id, t.CreatedAt })
                .ToList();

            var urls = publicTrips.Select(trip =>
                $@"
                    <url>
                        <loc>https://traveltipsgo.com/trip/{trip.Id}</loc>
                        <lastmod>{trip.CreatedAt:yyyy-MM-dd}</lastmod>
                        <changefreq>weekly</changefreq>
                        <priority>0.8</priority>
                    </url>
                "
            );

            var sitemap =
                $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.2"">
                    <url>
                        <loc>https://traveltipsgo.com/</loc>
                        <priority>1.0</priority>
                    </url>
                    {string.Join("", urls)}
                </urlset>";

            return sitemap;
        }

        // html
        public string GenerateHomePageHtml()
        {
            var homeHtml =
                @"
                <html>
                    <head>
                        <title>Travel Tips Go - Your Ultimate Guide to Travel</title>
                        <meta name='description' content='Discover essential travel tips, regional guides, and expert advice to make your next trip unforgettable.' />
                        <meta name='keywords' content='travel tips, travel guide, destination advice, travel planning, travel hacks, christian travel' />
                    </head>
                    <body>
                        <h1>Travel Tips Go</h1>
                        <p>Your journey. Your privacy. Your peace.</p>
                        <p>Start Planning Free</p>

                        <h2>Keep the magic alives!</h2>
                        <h3>Plan Surprise Trips</h3>
                        <p>Romance 💑, Birthday 🎂, Family 👨‍👩‍👧</p>
                        <p>Share your trip with trusted people. Hide destination from everyone else.</p>

                        <h2>We Don't Track You</h2>
                        <p>No more behavioral tracking</p>
                        <p>No more third party ads</p>
                        <p>Contextual ads only!</p>
                        <p>Built by someone who hates ads too..</p>

                        <h2>Core Features</h2>
                        <h3>Selective Event Privacy</h3>
                        <p>Lock individual stops within your trip. Public travelers see your route — trusted people see everything. You control who sees what.</p>
                        <h3>Trusted Sharing</h3>
                        <p>Share your full trip — hidden events included — with specific people only. Perfect for co-planners, travel partners, and mission teams.</p>
                        <h3>Smart Routing</h3>
                        <p>Get multiple route options between your stops. Pick the one that fits. Open in Google Maps when you're ready to navigate live.</p>
                        <h3>Budget Tagging</h3>
                        <p>Tag every trip from budget (1) to luxury (5). Discover trips that match how you like to travel — not just where.</p>
                        <h3>Unlimited PDF Downloads</h3>
                        <p>Download your full itinerary anytime, no limits. Perfect for remote areas, printed handouts, or sharing with people who aren't on the platform.</p>
                        <h3>Region Discovery</h3>
                        <p>Tag trips by region and let others discover your journey. Build a public travel story while keeping your private moments hidden.</p>
                        <p>Simple pricing. No hidden fees. No data games.</p>
                        <button>Subscribe. Up To $9/Month.</button>

                        <h2>Christian Values</h2>
                        <h3>Built with integrity. Open to all.</h3>
                        <h4>Values Statement</h4>
                        <p>
                            We believe how you build something matters as much as what you build.
                            <br/><br/>
                            Every ad is reviewed. Every user is respected. Every journey is yours.
                        </p>
                        <h4>Weekly Gospel</h4>
                        <p>
                            Every week the founder shares a reflection connecting faith, life, and the journey ahead. Free for everyone — no subscription required.
                            <br><br>
                            Whether you share the faith or simply appreciate thoughtful writing — you're welcome here.
                        </p>
                        <p>Travel intentionally. Live purposefully.</p>

                        <h2>Made For You</h2>
                        <h3>Romantic & Surprise Planners</h3>
                        <p>You have something special in mind. Plan every detail privately, reveal it at exactly the right moment. No spoilers.</p>
                        <h3>Church Groups & Mission Teams</h3>
                        <p>Coordinate complex group itineraries with sensitive locations. Share logistics with your team while keeping certain stops private from participants.</p>
                        <h3>Travel Bloggers</h3>
                        <p>Build a beautiful public trip portfolio. Keep your secret spots hidden from followers while sharing everything with trusted collaborators.</p>
                        <h3>Family Reunion Organizers</h3>
                        <p>One person handles everything so the family just shows up. Share the full plan with co-organizers, keep the surprise for everyone else.</p>
                        <h3>Privacy-Conscious Traveler</h3>
                        <p>You're tired of being tracked, profiled, and sold to. Here you're just a traveler — nothing more, nothing less.</p>
                        <p>Whoever you are — you travel your way here.</p>

                        <h2>Promote Your Business</h2>
                        <h3>Fair & Transparent</h3>
                        <p>Purchase ad weight on the search parameters that matter to your business. More weight means more visibility — simple, honest, no auction games.</p>
                        <h3>Context-Based Targeting</h3>
                        <p>Your ad appears when travelers are actively planning trips in your region and budget range. No behavioral profiling. No creepy retargeting. Just the right moment.</p>
                        <h3>Reviewed & Trusted</h3>
                        <p>Every business and ad is manually reviewed before going live. Your brand appears alongside content that shares your values — not just anything.</p>
                        <h2>How it works</h2>
                        <h3>Step 1</h3>
                        <p>Create your business profile</p>
                        <h3>Step 2</h3>
                        <p>
                            Purchase ad weight on your target parameters
                            <br>
                            e.g. region, budget, keyword, user
                        </p>
                        <h3>Step 3</h3>
                        <p>Appear naturally in relevant trip searches</p>
                        <h2>Examples</h2>
                        <h3>TravelTipsGo Membership</h3>
                        <p>Make your destination private.</p>
                        <p>Sponsored • TravelTipsGo</p>
                        <h3>Partner with us!</h3>
                        <p>TravelTipsGo</p>
                        <p>Join Now</p>
                        <p>Sponsored</p>
                        <button>Register your business →</button>
                        <p>Ad weights start at $10 each. Minimum 10 weights per target. <br> Premium pricing applies to high-demand regions.</p>
                    </body>
                </html>";

            return homeHtml;
        }
    }
}
