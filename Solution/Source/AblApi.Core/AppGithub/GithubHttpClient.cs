using AblApi.Common;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;

namespace AblApi.Core.AppGithub;

public class GithubHttpClient : CachedHttpClient, IGithubHttpClient
{
    public GithubHttpClient(GithubAppSettings githubAppSettings)
    {
        BaseAddress = new Uri("https://api.github.com/");

        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue(Encoding.UTF8.WebName));
        DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubAppSettings.Token);
        DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AblApi", "1.0"));
        DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
    }

    public async Task<bool> TestToken()
    {
        try
        {
            var response = GetAsync("").Result;
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
