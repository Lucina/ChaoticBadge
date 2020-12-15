#nullable enable
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ChaoticBadge;
using Microsoft.AspNetCore.Mvc;
using static ChaoticBadgeService.ChaoticBadgeServiceUtil;

namespace ChaoticBadgeService.Controllers
{
    [Route("github-workflow")]
    [ApiController]
    [ResponseCache(Duration = 10 * 60)]
    public class GithubWorkflowController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GithubWorkflowController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{owner}/{repo}/{workflow_file}")]
        public async Task<IActionResult> Retrieve(
            [FromRoute(Name = "owner")] string owner,
            [FromRoute(Name = "repo")] string repo,
            [FromRoute(Name = "workflow_file")] string workflowFile,
            [FromQuery(Name = "custom_name")] string? customName = null,
            [FromQuery(Name = "custom_left_color")]
            string? customLeftColor = null,
            [FromQuery(Name = "custom_right_color")]
            string? customRightColor = null,
            [FromQuery(Name = "stupid")] bool? stupid = null)
        {
            // TODO db for cache + filter
            BadgeType resultKind;
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
                    true => BadgeType.Passing,
                    false => BadgeType.Failing,
                    null => BadgeType.Unknown
                };
            }
            else
                resultKind = BadgeType.NotFound;

            return Badge(this, customName ?? "GitHub Workflow", resultKind,
                customLeftColor: customLeftColor, customRightColor: customRightColor,
                typeMasp: stupid ?? false ? Badges.StupidMap : null);
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
