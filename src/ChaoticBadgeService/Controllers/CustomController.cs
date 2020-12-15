#nullable enable
using ChaoticBadge;
using Microsoft.AspNetCore.Mvc;
using static ChaoticBadgeService.ChaoticBadgeServiceUtil;

namespace ChaoticBadgeService.Controllers
{
    [Route("custom")]
    [ApiController]
    [ResponseCache(Duration = 10 * 60)]
    public class CustomController : ControllerBase
    {
        [HttpGet("{name}/{status}")]
        public IActionResult Retrieve(
            [FromRoute(Name = "name")] string name,
            [FromRoute(Name = "status")] string? status = null,
            [FromQuery(Name = "left_color")] string? customLeftColor = null,
            [FromQuery(Name = "right_color")] string? customRightColor = null,
            [FromQuery(Name = "stupid")] bool? stupid = null
        ) =>
            Badge(stupid ?? false ? StupidStyle : DefaultStyle, this, name, Status.Passing, statusText: status,
                customLeftColor: customLeftColor, customRightColor: customRightColor);
    }
}
