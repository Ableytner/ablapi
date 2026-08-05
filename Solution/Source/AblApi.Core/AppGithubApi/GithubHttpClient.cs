using AblApi.Common;
using Microsoft.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.InMemory;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace AblApi.Core.AppGithubApi;

public class GithubHttpClient : CachedHttpClient
{
    public GithubHttpClient()
    {
        string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? throw new InvalidOperationException("GITHUB_TOKEN environment variable is not set.");

        BaseAddress = new Uri("https://api.github.com/");

        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue(Encoding.UTF8.WebName));
        DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AblApi", "1.0"));
        DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
    }
}
