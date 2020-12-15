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

        )
        {
            var style = stupid ?? false ? StupidStyle : DefaultStyle;
            if (height > 4)
                style = style with {Height = height.Value};
            if (fontSize > 4)
                style = style with {FontSizePts = fontSize.Value};
            return Badge(style, this, name, Status.Passing, statusText: status,
                leftColor: customLeftColor, rightColor: customRightColor, icon: icon);
        }
    }
}
