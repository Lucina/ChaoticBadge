#nullable enable
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ChaoticBadge;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Semver;
using static ChaoticBadgeService.ChaoticBadgeServiceUtil;

namespace ChaoticBadgeService.Controllers
{
    [Route("nuget")]
    [ApiController]
    [ResponseCache(Duration = 120 * 60)]
    public class NugetController : BadgeServiceControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public NugetController(IFileProvider fileProvider, IHttpClientFactory httpClientFactory) : base(fileProvider)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Retrieve(
            [FromRoute(Name = "id")] string id,
            [FromQuery(Name = "name")] string? customName = null,
            [FromQuery(Name = "left-color")] string? customLeftColor = null,
            [FromQuery(Name = "right-color")] string? customRightColor = null,
            [FromQuery(Name = "stupid")] bool? stupid = null,
            [FromQuery(Name = "icon")] string? icon = null,
            [FromQuery(Name = "height")] int? height = null,
            [FromQuery(Name = "font-size")] int? fontSize = null
        )
        {
            // TODO db for cache + filter
            string? ver;
            Status status;
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
                        (ver, status) = (latest.ToString(),
                            string.IsNullOrEmpty(latest.Prerelease)
                                ? Status.Release
                                : Status.PreRelease);
                    }
                    else
                        (ver, status) = (null, Status.Error);
                }
                catch
                {
                    (ver, status) = (null, Status.Error);
                }
            }
            else
                (ver, status) = (null, Status.NotFound);

            return Badge(this, customName ?? id, status, stupid: stupid, height: height, fontSize: fontSize,
                statusText: ver, leftColor: customLeftColor, rightColor: customRightColor, icon: icon);
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
