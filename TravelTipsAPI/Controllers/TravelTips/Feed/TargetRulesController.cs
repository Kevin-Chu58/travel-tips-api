using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Constants.Enums;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Constants.Enums.AdTargetEnum;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;

namespace TravelTipsAPI.Controllers.TravelTips.Feed
{
    [Route("api/[controller]")]
    public class TargetRulesController(ITargetRulesService targetRulesService)
        : TravelTipsControllerBase
    {
        /// <summary>
        /// Get a list of target rules by target type
        /// </summary>
        /// <param name="type">target type</param>
        /// <returns>a list of target rules</returns>
        [HttpGet]
        [Route("{type}")]
        [HasRole(Role = UserRoles.ADMIN)]
        public ActionResult<IEnumerable<TargetRuleViewModel>> GetTargetRulesByType(AdTarget type)
        {
            var result = targetRulesService.GetTargetRulesByType(type);
            return Ok(result);
        }

        /// <summary>
        /// Get a target rule by target type and target value. If not found, return null
        /// </summary>
        /// <param name="type">target type</param>
        /// <param name="value">target value</param>
        /// <returns>a target rule</returns>
        [HttpGet]
        [Route("{type}/{value}")]
        [HasRole(Role = UserRoles.ADMIN)]
        public ActionResult<TargetRuleViewModel?> GetTargetRule(AdTarget type, string? value)
        {
            var typeStr = GetAdTargetStr(type);
            if (typeStr == null)
                return BadRequest(Messages.AdTargetTypeInvalid);

            var result = targetRulesService.GetTargetRule(typeStr, value);

            // If not found, try to find the default target rule with the same target type
            // (default target rule has null target value)
            if (result == null)
                result = targetRulesService.GetTargetRule(typeStr, null);

            return Ok(result);
        }

        /// <summary>
        /// Create a new target rule
        /// </summary>
        /// <param name="targetType">target type</param>
        /// <param name="targetValue">target value</param>
        /// <param name="minWeight">min weight</param>
        /// <returns>the newly created target rule</returns>
        [HttpPost]
        [Route("")]
        [HasRole(Role = UserRoles.ADMIN)]
        public async Task<ActionResult<TargetRuleViewModel>> PostNewTargetRule(
            [FromQuery] AdTarget targetType,
            string? targetValue,
            int minWeight
        )
        {
            var result = await targetRulesService.PostNewTargetRule(
                targetType,
                targetValue,
                minWeight
            );
            return Ok(result);
        }

        /// <summary>
        /// Update an existing target rule
        /// </summary>
        /// <param name="id">target rule id</param>
        /// <param name="targetType">target type</param>
        /// <param name="targetValue">target value</param>
        /// <param name="minWeight">min weight</param>
        /// <returns>the updated target rule</returns>
        [HttpPatch]
        [Route("{id}")]
        [HasRole(Role = UserRoles.ADMIN)]
        public async Task<ActionResult<TargetRuleViewModel>> UpdateTargetRuleMinWeight(
            int id,
            [FromQuery] AdTarget targetType,
            string? targetValue,
            int minWeight
        )
        {
            var targetRule = targetRulesService.FindTargetRuleById(id);
            if (targetRule == null)
                return NotFound(Messages.TargetRuleNotFound);

            var result = await targetRulesService.UpdateTargetRule(
                targetRule,
                targetType,
                targetValue,
                minWeight
            );
            return Ok(result);
        }

        /// <summary>
        /// Delete an existing target rule
        /// </summary>
        /// <param name="id">target rule id</param>
        /// <returns>the deleted target rule id</returns>
        [HttpDelete]
        [Route("{id}")]
        [HasRole(Role = UserRoles.ADMIN)]
        public async Task<ActionResult<int>> DeleteTargetRule(int id)
        {
            var targetRule = targetRulesService.FindTargetRuleById(id);
            if (targetRule == null)
                return NotFound(Messages.TargetRuleNotFound);

            var result = await targetRulesService.DeleteTargetRule(targetRule);
            return Ok(result);
        }
    }
}
