using AblApi.Common;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;

namespace AblApi.Core.AppGithub;

public class GithubHttpClient : CachedHttpClient
{
    private readonly ILogger<GithubHttpClient> _logger;

    public GithubHttpClient(ILogger<GithubHttpClient> logger, GithubAppSettings githubAppSettings)
    {
        _logger = logger;

        BaseAddress = new Uri("https://api.github.com/");

        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue(Encoding.UTF8.WebName));
        DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubAppSettings.Token);
        DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AblApi", "1.0"));
        DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");

        try
        {
            var response = GetAsync("").Result;
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Github API. Please check your Github token and network connectivity.");
        }
    }
}
