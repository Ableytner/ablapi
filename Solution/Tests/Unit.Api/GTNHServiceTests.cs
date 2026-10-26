using AblApi.Core.AppGithub;
using AblApi.Core.AppGithub.Dtos;
using AblApi.GTNH;
using AblApi.GTNH.Dtos;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Tests.Unit.Api;

public class GTNHServiceTests
{
    private const string ClientDownloadUrl = "https://example.com/client";
    private const string ClientJava8DownloadUrl = "https://example.com/client-java8";
    private const string ServerDownloadUrl = "https://example.com/server";
    private const string ServerJava8DownloadUrl = "https://example.com/server-java8";
    private const string RunUrl = "https://example.com/run";
    private const string RunHtmlUrl = "https://example.com/run-html";

    [Theory]
    [InlineData(1, "mmcprism-new-java", "mmcprism-java8", "server-new-java", "server")]
    [InlineData(501, "mmcprism-java17-25.zip", "mmcprism-java8.zip", "server-java17-25.zip", "server-java8.zip")]
    [InlineData(637, "mmcprism-java17-26.zip", "mmcprism-java8.zip", "server-java17-26.zip", "server-java8.zip")]
    public async Task MapWorkflowRunToDailyVersionDto_MapsDownloadUrls_ForSchemaVersion(
        int runNumber,
        string clientArchiveName,
        string clientJava8ArchiveName,
        string serverArchiveName,
        string serverJava8ArchiveName)
    {
        // Arrange
        var artifacts = new WorkflowArtifactsDto
        {
            TotalCount = 4,
            Artifacts =
            [
                CreateArtifact(clientArchiveName, ClientDownloadUrl),
                CreateArtifact(clientJava8ArchiveName, ClientJava8DownloadUrl),
                CreateArtifact(serverArchiveName, ServerDownloadUrl),
                CreateArtifact(serverJava8ArchiveName, ServerJava8DownloadUrl)
            ]
        };

        var gtnhConfiguration = new GTNHAppSettings
        {
            DailyBuildsRepoOwner = "owner",
            DailyBuildsRepoName = "repo",
            DailyBuildsWorkflowId = "workflow"
        };

        var githubService = Substitute.For<IGithubService>();
        githubService.GetWorkflowArtifactsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(artifacts);

        var service = new GTNHService(
            NullLogger<GTNHService>.Instance,
            gtnhConfiguration,
            githubService,
            gtNewHorizonsService: null!,
            ablRepository: null!);

        var workflowRun = new WorkflowRunDto
        {
            Id = 1,
            RunNumber = runNumber,
            Status = "completed",
            Conclusion = "success",
            Url = RunUrl,
            HtmlUrl = RunHtmlUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var dto = await InvokeMapWorkflowRunToDailyVersionDto(service, workflowRun);

        // Assert
        Assert.Equal(runNumber, dto.RunNumber);
        Assert.True(dto.Success);
        Assert.NotNull(dto.DownloadUrls);
        Assert.Equal(ClientDownloadUrl, dto.DownloadUrls!.Client);
        Assert.Equal(ClientJava8DownloadUrl, dto.DownloadUrls.ClientJava8);
        Assert.Equal(ServerDownloadUrl, dto.DownloadUrls.Server);
        Assert.Equal(ServerJava8DownloadUrl, dto.DownloadUrls.ServerJava8);
    }

    private static Task<DailyVersionDto> InvokeMapWorkflowRunToDailyVersionDto(GTNHService service, WorkflowRunDto workflowRun)
    {
        var method = typeof(GTNHService).GetMethod("MapWorkflowRunToDailyVersionDto", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (Task<DailyVersionDto>)method.Invoke(service, [workflowRun])!;
    }

    [Theory]
    [InlineData("gtnh-web-map-1.0.jar", "1.0")]
    [InlineData("gtnh-web-map-1.0.0.jar", "1.0.0")]
    [InlineData("gtnh-web-map-1.0-beta-1.jar", "1.0-beta-1")]
    [InlineData("gtnh-web-map-1.0.0-alpha-2.jar", "1.0.0-alpha-2")]
    [InlineData("gtnh-web-map-12.34.jar", "12.34")]
    [InlineData("gtnh-web-map-1.0.0-rc-100.jar", "1.0.0-rc-100")]
    public void WebMapVersionRegex_MatchesValidNames_CapturesVersion(string name, string expectedVersion)
    {
        // Arrange
        var regex = GetWebMapVersionRegex();

        // Act
        var match = regex.Match(name);

        // Assert
        Assert.True(match.Success);
        Assert.Equal(expectedVersion, match.Groups[1].Value);
    }

    [Theory]
    [InlineData("gtnh-web-map-1.0.zip")]
    [InlineData("gtnh-web-map-1.0")]
    [InlineData("gtnh-web-map-1.jar")]
    [InlineData("gtnh-web-map-1.0.0.0.jar")]
    [InlineData("web-map-1.0.jar")]
    [InlineData("gtnh-web-map-1.0-.jar")]
    public void WebMapVersionRegex_RejectsInvalidNames(string name)
    {
        // Arrange
        var regex = GetWebMapVersionRegex();

        // Act
        var match = regex.Match(name);

        // Assert
        Assert.False(match.Success);
    }

    private static Regex GetWebMapVersionRegex()
    {
        var field = typeof(GTNHService).GetField("WebMapVersionRegex", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (Regex)field.GetValue(null)!;
    }

    private static WorkflowArtifactDto CreateArtifact(string name, string archiveDownloadUrl)
    {
        return new WorkflowArtifactDto
        {
            Name = name,
            SizeInBytes = 0,
            Url = archiveDownloadUrl,
            ArchiveDownloadUrl = archiveDownloadUrl,
            Expired = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
    }
}
