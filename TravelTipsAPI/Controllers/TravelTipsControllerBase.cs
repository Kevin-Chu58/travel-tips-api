using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelTipsAPI.Controllers;

[ApiController]
[Authorize]
public class TravelTipsControllerBase : ControllerBase { }
