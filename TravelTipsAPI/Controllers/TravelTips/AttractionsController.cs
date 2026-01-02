using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.HereMap;
using static TravelTipsAPI.Services.HereMapServices.HereMapSchema;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

namespace TravelTipsAPI.Controllers.TravelTips
{
    /// <summary>
    /// The controller of Attractions
    /// </summary>
    /// <param name="attractionsService">attractions service</param>
    /// <param name="linksService">links service</param>
    [Route("api/[controller]")]
    public class AttractionsController(
        IAttractionsService attractionsService,
        IHereMapDiscoverService hereMapDiscoverService,
        IHereMapLookupService hereMapLookupService
    ) : TravelTipsControllerBase
    {
        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        public ActionResult<AttractionViewModel> GetAttractionById(int id)
        {
            try
            {
                var attraction = attractionsService.FindAttractionById(id);
                return Ok((AttractionViewModel)attraction);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("{id}/here-map")]
        [AllowAnonymous]
        public async Task<ActionResult<HerePlace>> GetHerePlaceByAttractionId(int id)
        {
            try
            {
                var attraction = attractionsService.FindAttractionById(id);
                var hereId = attraction.HereId;
                HerePlace herePlace;
                try
                {
                    herePlace = await hereMapLookupService.LookupPlaceByIdAsync(hereId);
                    return Ok(herePlace);
                }
                catch (Exception)
                {
                    // hereId is outdated, auto-update the hereId, Lat & Lng, and address of the attraction
                    var places = await hereMapDiscoverService.SearchPlaceByNameAsync(
                        attraction.Title,
                        attraction.Lat,
                        attraction.Lng
                    );
                    var newAttraction = places.First(place =>
                        place.City == attraction.City
                        && place.State == attraction.State
                        && place.Country == attraction.Country
                        && place.Title == attraction.Title
                        && place.Category == attraction.Category
                        && place.ResultType == attraction.ResultType
                    );

                    if (newAttraction != null)
                    {
                        attraction = await attractionsService.UpdateAttractionAsync(
                            attraction,
                            newAttraction
                        );
                        herePlace = await hereMapLookupService.LookupPlaceByIdAsync(
                            attraction.HereId
                        );
                        return Ok(herePlace);
                    }
                    else
                        return NotFound("Attraction no longer exist in Here Map.");
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Get the search result contains a list of attractions with filter params
        /// </summary>
        /// <param name="name">attraction name</param>
        /// <returns>a list of attractions that satisfy the condition</returns>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<AttractionViewModel>> GetAllAttractionsByParams(
            [FromQuery] string? name
        )
        {
            var attractionViewModels = attractionsService.GetAttractionsByParams(name, null);

            return Ok(attractionViewModels);
        }

        /// <summary>
        /// Get search result of your attractions with filter params
        /// </summary>
        /// <param name="name">attraction name</param>
        /// <returns>a list of attractions that satisfy the condition</returns>
        [HttpGet]
        [Route("my")]
        [IsOwner(Resource = Resources.NONE)]
        public ActionResult<IEnumerable<AttractionViewModel>> GetMyAttractionsByParams(
            [FromQuery] string? name
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var attractionViewModels = attractionsService.GetAttractionsByParams(name, userId);

            return Ok(attractionViewModels);
        }

        /// <summary>
        /// Create a new attraction by hereId
        /// </summary>
        /// <param name="hereId">hereId</param>
        /// <returns>the new attraction</returns>
        [HttpPost]
        [Route("{hereId}")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<AttractionViewModel>> PostNewAttraction(string hereId)
        {
            Attraction attraction;
            try
            {
                attraction = attractionsService.FindAttractionByHereId(hereId);
            }
            catch (Exception)
            {
                attraction = await attractionsService.PostNewAttractionAsync(hereId);
            }

            var attractionViewModel = (AttractionViewModel)attraction;

            return Ok(attractionViewModel);
        }
    }
}
