using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelTipsAPI.Controllers.TravelTips;

[ApiController]
[Authorize]
public class TravelTipsControllerBase : ControllerBase { }
