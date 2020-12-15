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
        [HttpGet("{name}")]
        public IActionResult Retrieve(
            string name,
            [FromQuery(Name = "status")] string? status = null,
            [FromQuery(Name = "left_color")] string? customLeftColor = null,
            [FromQuery(Name = "right_color")] string? customRightColor = null,
            [FromQuery(Name = "stupid")] bool? stupid = null
        ) =>
            Badge(this, name, BadgeType.Passing, status: status,
                customLeftColor: customLeftColor, customRightColor: customRightColor,
                typeMasp: stupid ?? false ? Badges.StupidMap : null);
    }
}
