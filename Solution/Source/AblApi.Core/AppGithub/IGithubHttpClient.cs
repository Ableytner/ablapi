namespace AblApi.Core.AppGithub;

public interface IGithubHttpClient
{
    Task<HttpResponseMessage> GetAsync(string? requestUri, CancellationToken cancellationToken);

    Task<bool> TestToken();
}
