using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Responses;
using TravelTipsAPI.Controllers.TravelTips;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.HereMap;
using static TravelTipsAPI.Services.HereMapServices.HereMapSchema;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Utils.ObjectUtils;

namespace TravelTipsAPI.Controllers.HereMap
{
    /// <summary>
    /// The controller of Here Map API
    /// </summary>
    /// <param name="hereMapDiscoverService">here map discover service</param>
    [Route("api/[controller]")]
    public class HereMapController(
        IHereMapDiscoverService hereMapDiscoverService,
        IHereMapLookupService hereMapLookupService,
        IHereMapRoutingService hereMapRoutingService,
        ITripAttractionOrdersService tripAttractionOrdersService,
        IConfiguration config
    ) : TravelTipsControllerBase
    {
#pragma warning disable OPENAI001
        private readonly static string model = "gpt-4.1-nano";
        private readonly OpenAIResponseClient _client = new(
            model: model,
            apiKey: config["OpenAI:ApiKey"]
        );

        [HttpGet]
        [Route("suggestion")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetSuggestionFromGPT(
            [FromQuery] string input
        )
        {
            // Prompt instructs AI to output JSON array of strings
            string prompt =
                $"Give 5 short search suggestions for the user input \"{input}\". Return ONLY a JSON array of strings of locations.";

            OpenAIResponse response = await _client.CreateResponseAsync(prompt);

            string outputText = response.GetOutputText();

            string outputArray = ExtractJsonArray(outputText);

            try
            {
                // Convert string to List<string>
                List<string> suggestions =
                    JsonSerializer.Deserialize<List<string>>(outputArray) ?? [];

                return Ok(suggestions);
            }
            catch (Exception)
            {
                {
                    return Ok();
                }
            }
        }

        /// <summary>
        /// Find a list of HerePlace by query name
        /// </summary>
        /// <param name="query">search name</param>
        /// <param name="lat">lat to search from</param>
        /// <param name="lng">lng to search from</param>
        /// <param name="limit">returned number of items</param>
        /// <returns>a list of HerePlace</returns>
        [HttpGet]
        [Route("discover")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AttractionViewModel>>> SearchPlaceByNameAsync(
            [FromQuery] string query,
            decimal lat,
            decimal lng,
            int? limit
        )
        {
            try
            {
                var attractions = await hereMapDiscoverService.SearchPlaceByNameAsync(
                    query,
                    lat,
                    lng,
                    limit
                );
                return Ok(attractions);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("routing/{taoId}")]
        [AllowAnonymous]
        public async Task<ActionResult<HereRoutingResponse?>> GetRoutingOnTaoAsync(int taoId)
        {
            var hereRoutingInput = tripAttractionOrdersService.GetHereRoutingInputByTaoId(taoId);

            if (hereRoutingInput == null)
                return Ok(null);

            try
            {
                var hereRoutingResponse = await hereMapRoutingService.GetRouteAsync(
                    hereRoutingInput
                );
                return Ok(hereRoutingResponse);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("routing/day/{dayId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<HereRoutingResponse?>>> GetRoutingsOnDayAsync(
            int dayId
        )
        {
            try
            {
                var attractionRoutings = tripAttractionOrdersService
                    .GetAttractionRoutingsByDayId(dayId)
                    .ToList();
                List<HereRoutingInput> routeInputs = [];

                for (var i = 1; i < attractionRoutings.Count; i++)
                {
                    var prevRouting = attractionRoutings[i - 1];
                    var curRouting = attractionRoutings[i];

                    var routeInput = new HereRoutingInput
                    {
                        TransportMode = curRouting.TransportMode,
                        OriginLat = prevRouting.Position.Lat,
                        OriginLng = prevRouting.Position.Lng,
                        DestinationLat = curRouting.Position.Lat,
                        DestinationLng = curRouting.Position.Lng,
                    };

                    routeInputs.Add(routeInput);
                }

                var hereRoutingResponses = await hereMapRoutingService.GetRoutesAsync(routeInputs);
                return Ok(hereRoutingResponses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
