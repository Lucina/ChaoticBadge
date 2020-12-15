#nullable enable
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ChaoticBadge;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using static ChaoticBadgeService.ChaoticBadgeServiceUtil;

namespace ChaoticBadgeService.Controllers
{
    [Route("github-workflow")]
    [ApiController]
    [ResponseCache(Duration = 120 * 60)]
    public class GithubWorkflowController : BadgeServiceControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GithubWorkflowController(IFileProvider fileProvider, IHttpClientFactory httpClientFactory) : base(
            fileProvider)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{owner}/{repo}/{workflow-file}")]
        public async Task<IActionResult> Retrieve(
            [FromRoute(Name = "owner")] string owner,
            [FromRoute(Name = "repo")] string repo,
            [FromRoute(Name = "workflow-file")] string workflowFile,
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
            Status resultKind;
            var c = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.github.com/repos/{owner}/{repo}/actions/workflows/{workflowFile}/runs?per_page=1");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "HttpClient");
            var response = await c.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                byte[] responseBody = await response.Content.ReadAsByteArrayAsync();
                bool? success = null;
                try
                {
                    success = CheckSuccess(responseBody);
                }
                catch
                {
                    // ignored
                }

                resultKind = success switch
                {
                    true => Status.Passing,
                    false => Status.Failing,
                    null => Status.Unknown
                };
            }
            else
                resultKind = Status.NotFound;

            var style = stupid ?? false ? StupidStyle : DefaultStyle;
            if (height > 4)
                style = style with {Height = height.Value};
            if (fontSize > 4)
                style = style with {FontSizePts = fontSize.Value};
            return Badge(style, this, customName ?? "GitHub Workflow",
                resultKind, leftColor: customLeftColor, rightColor: customRightColor, icon: icon);
        }

        private static bool? CheckSuccess(byte[] jsonContent)
        {
            // Effectively just needs to check {workflow_runs[{conclusion
            var reader = new Utf8JsonReader(jsonContent);

            reader.Read();
            if (reader.TokenType != JsonTokenType.StartObject) return null;

            while (reader.Read())
                if (reader.GetString() != "workflow_runs") reader.Skip();
                else break;

            reader.Read();
            if (reader.TokenType != JsonTokenType.StartArray) return null;

            reader.Read();
            if (reader.TokenType != JsonTokenType.StartObject) return null;

            while (reader.Read())
                if (reader.GetString() != "conclusion") reader.Skip();
                else break;

            reader.Read();
            return reader.GetString() switch
            {
                "success" => true,
                "failure" => false,
                "neutral" => null,
                "cancelled" => null,
                "skipped" => null,
                "timed_out" => null,
                "action_required" => null,
                _ => null
            };
        }
    }
}
