using AblApi.Core.AppGithub;
using AblApi.GTNH;
using Integration.Api.Fixture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using System.Net;

namespace Integration.Api.GTNH.Fixture;

public class GTNHTestApiFactory(string databaseName, string gtnhStableVersionApiUrl) : TestApiFactory(databaseName)
{
    private readonly string _gtnhStableVersionApiUrl = gtnhStableVersionApiUrl;

    protected override void SetupAdditionalServices(IServiceCollection services)
    {
        var existing = services.FirstOrDefault(d => d.ServiceType == typeof(GTNHAppSettings));
        var gtnhAppSettings = (existing?.ImplementationInstance as GTNHAppSettings) ?? new GTNHAppSettings();
        gtnhAppSettings.StableVersionApiUrl = _gtnhStableVersionApiUrl;

        services.RemoveAll<GTNHAppSettings>();
        services.AddSingleton(gtnhAppSettings);

        services.RemoveAll<IGithubHttpClient>();
        services.AddSingleton(CreateGithubHttpClientMock(gtnhAppSettings));
    }

    private static IGithubHttpClient CreateGithubHttpClientMock(GTNHAppSettings gtnhAppSettings)
    {
        var owner = gtnhAppSettings.DailyBuildsRepoOwner;
        var repo = gtnhAppSettings.DailyBuildsRepoName;
        var workflowId = gtnhAppSettings.DailyBuildsWorkflowId;

        var mockHttpClient = Substitute.For<IGithubHttpClient>();

        mockHttpClient.TestToken().Returns(Task.FromResult(true));

        mockHttpClient.GetAsync(
            $"repos/{owner}/{repo}/actions/workflows/{workflowId}/runs?page=1&per_page=1",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(JsonResponse("Data/workflowruns_1.json")));

        mockHttpClient.GetAsync(
            $"repos/{owner}/{repo}/actions/workflows/{workflowId}/runs?page=1&per_page=100",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(JsonResponse("Data/workflowruns_100_page1.json")));

        mockHttpClient.GetAsync(
            $"repos/{owner}/{repo}/actions/workflows/{workflowId}/runs?page=2&per_page=100",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(JsonResponse("Data/workflowruns_100_page2.json")));

        mockHttpClient.GetAsync(
            Arg.Is<string>(url => url != null && url.Contains("/artifacts")),
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(JsonResponse(content: "{\"total_count\":0,\"artifacts\":[]}")));

        return mockHttpClient;
    }

    private static HttpResponseMessage JsonResponse(string? filePath = null, string? content = null)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content ?? File.ReadAllText(filePath!))
        };
    }
}
