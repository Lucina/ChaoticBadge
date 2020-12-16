#nullable enable
using ChaoticBadge;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using static ChaoticBadgeService.ChaoticBadgeServiceUtil;

namespace ChaoticBadgeService.Controllers
{
    [Route("custom")]
    [ApiController]
    [ResponseCache(Duration = 120 * 60)]
    public class CustomController : BadgeServiceControllerBase
    {
        public CustomController(IFileProvider fileProvider) : base(fileProvider)
        {
        }

        [HttpGet("{name}/{status}")]
        public IActionResult Retrieve(
            [FromRoute(Name = "name")] string name,
            [FromRoute(Name = "status")] string? status = null,
            [FromQuery(Name = "left-color")] string? customLeftColor = null,
            [FromQuery(Name = "right-color")] string? customRightColor = null,
            [FromQuery(Name = "stupid")] bool? stupid = null,
            [FromQuery(Name = "icon")] string? icon = null,
            [FromQuery(Name = "height")] int? height = null,
            [FromQuery(Name = "font-size")] int? fontSize = null
        ) =>
            Badge(this, name, Status.Passing, stupid: stupid, height: height, fontSize: fontSize, statusText: status,
                leftColor: customLeftColor, rightColor: customRightColor, icon: icon);
    }
}
