#nullable enable
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ChaoticBadge;
using Microsoft.AspNetCore.Mvc;
using Semver;
using static ChaoticBadgeService.ChaoticBadgeServiceUtil;

namespace ChaoticBadgeService.Controllers
{
    [Route("nuget")]
    [ApiController]
    [ResponseCache(Duration = 10 * 60)]
    public class NugetController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public NugetController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Retrieve(
            [FromRoute(Name = "id")] string id,
            [FromQuery(Name = "custom_name")] string? customName = null,
            [FromQuery(Name = "custom_left_color")]
            string? customLeftColor = null,
            [FromQuery(Name = "custom_right_color")]
            string? customRightColor = null,
            [FromQuery(Name = "stupid")] bool? stupid = null
        )
        {
            // TODO db for cache + filter
            string? ver;
            BadgeType badgeType;
            var c = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.nuget.org/v3-flatcontainer/{id}/index.json");
            request.Headers.Add("User-Agent", "HttpClient");
            var response = await c.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                byte[] responseBody = await response.Content.ReadAsByteArrayAsync();
                try
                {
                    var latest = GetLatestVersion(responseBody);
                    if (latest != null)
                    {
                        (ver, badgeType) = (latest.ToString(),
                            string.IsNullOrEmpty(latest.Prerelease)
                                ? BadgeType.Release
                                : BadgeType.PreRelease);
                    }
                    else
                        (ver, badgeType) = (null, BadgeType.Error);
                }
                catch
                {
                    (ver, badgeType) = (null, BadgeType.Error);
                }
            }
            else
                (ver, badgeType) = (null, BadgeType.NotFound);

            return Badge(this, customName ?? id, badgeType, ver,
                customLeftColor: customLeftColor, customRightColor: customRightColor,
                typeMasp: stupid ?? false ? Badges.StupidMap : null);
        }

        private static SemVersion? GetLatestVersion(byte[] jsonContent)
        {
            // Effectively just needs to check {workflow_runs[{conclusion
            var reader = new Utf8JsonReader(jsonContent);
            SemVersion? res = null;

            reader.Read();
            if (reader.TokenType != JsonTokenType.StartObject) return null;
            while (reader.Read())
                if (reader.GetString() != "versions")
                    reader.Skip();
                else break;

            reader.Read();
            if (reader.TokenType != JsonTokenType.StartArray) return null;
            while (reader.Read())
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndArray:
                        return res;
                    case JsonTokenType.String:
                    {
                        var sv = SemVersion.Parse(reader.GetString());
                        if (res == null)
                            res = sv;
                        else
                            res = sv > res ? sv : res;
                        break;
                    }
                    default:
                        return null;
                }

            return null;
        }
    }
}
