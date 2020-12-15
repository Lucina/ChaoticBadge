#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace ChaoticBadgeService
{
    [Route("custom")]
    [ApiController]
    [ResponseCache(Duration = 120 * 60)]
    public abstract class BadgeServiceControllerBase : ControllerBase
    {
        public IFileProvider FileProvider { get; }

        protected BadgeServiceControllerBase(IFileProvider fileProvider)
        {
            FileProvider = fileProvider;
        }
    }
}
