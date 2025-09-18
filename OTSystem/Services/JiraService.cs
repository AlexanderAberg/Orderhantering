using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OTSystem.Configuration;

namespace OTSystem.Services
{
    public class JiraService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JiraSettings _settings;

        public JiraService(IHttpClientFactory httpClientFactory, IOptions<JiraSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
        }

        public async Task<string?> CreateIssueAsync(string summary, string description, string? issueType = null, string[]? labels = null)
        {
            var client = _httpClientFactory.CreateClient("Jira");

            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.Email}:{_settings.ApiToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            var payload = new
            {
                fields = new
                {
                    project = new { key = _settings.ProjectKey },
                    summary = summary,
                    issuetype = new { name = issueType ?? _settings.IssueType ?? "Task" },
                    labels = labels ?? Array.Empty<string>(),
                    description = BuildAdf(description)
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var resp = await client.PostAsync("rest/api/3/issue", new StringContent(json, Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            return doc.RootElement.TryGetProperty("key", out var keyEl) ? keyEl.GetString() : null;
        }

        private static object BuildAdf(string text) => new
        {
            type = "doc",
            version = 1,
            content = new object[]
            {
                new
                {
                    type = "paragraph",
                    content = new object[] { new { type = "text", text } }
                }
            }
        };
    }
}