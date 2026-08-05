using AblApi.Api;
using AblApi.Common.Extensions;
using AblApi.Core.AppGithubApi;
using AblApi.Core.AppGithubApi.Dtos;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Integration.Api.Core;

public class GithubApiServiceTests
{
    private readonly ILogger<GithubApiService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _httpClient;

    private readonly string _owner = "GTNewHorizons";
    private readonly string _repo = "DreamAssemblerXXL";
    private readonly string _workflowId = "daily-modpack-build.yml";
    private readonly long _runId = 30980037491L;

    public GithubApiServiceTests()
    {
        DotEnv.LoadEnvVariables();

        _logger = Substitute.For<ILogger<GithubApiService>>();
        _httpClient = new GithubHttpClient();
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _httpClientFactory.CreateClient("GithubApi").Returns(_httpClient);
    }

    [Fact]
    public async Task GetOneWorkflowRunAsync_WithoutFilter_ReturnsFirstRun()
    {
        // Arrange
        var service = new GithubApiService(_logger, _httpClientFactory);

        // Act
        var result = await service.GetOneWorkflowRunAsync(_owner, _repo, _workflowId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.UpdatedAt.IsWithinHoursOf(DateTime.UtcNow, 48));
    }

    [Fact]
    public async Task GetOneWorkflowRunAsync_WithSuccessFilter_ReturnsFirstSuccessfulRun()
    {
        // Arrange
        var service = new GithubApiService(_logger, _httpClientFactory);

        // Act
        var result = await service.GetOneWorkflowRunAsync(_owner, _repo, _workflowId, service.SuccessFilter, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("success", result.Conclusion);
    }

    [Fact]
    public async Task GetOneWorkflowRunAsync_WithFailureFilter_ReturnsFirstFailedRun()
    {
        // Arrange
        var service = new GithubApiService(_logger, _httpClientFactory);

        // Act
        var result = await service.GetOneWorkflowRunAsync(_owner, _repo, _workflowId, service.FailureFilter, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("failure", result.Conclusion);
    }

    [Fact]
    public async Task GetOneWorkflowRunAsync_NoMatchingRun_ReturnsNull()
    {
        // Arrange
        var filter = new Func<WorkflowRunDto, bool>(run => run.RunNumber == 9999);

        var service = new GithubApiService(_logger, _httpClientFactory);

        // Act
        var result = await service.GetOneWorkflowRunAsync(_owner, _repo, _workflowId, filter, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetWorkflowRunsAsync_WithSmallCount_ReturnsRequestedNumberOfRuns()
    {
        // Arrange
        var count = 3;

        var service = new GithubApiService(_logger, _httpClientFactory);

        // Act
        var result = await service.GetWorkflowRunsAsync(_owner, _repo, _workflowId, count, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(count, result.Count);
    }

    [Fact]
    public async Task GetWorkflowRunsAsync_WithCount_ReturnsRequestedNumberOfRuns()
    {
        // Arrange
        var count = 113;

        var service = new GithubApiService(_logger, _httpClientFactory);

        // Act
        var result = await service.GetWorkflowRunsAsync(_owner, _repo, _workflowId, count, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(count, result.Count);
    }

    [Fact]
    public async Task GetWorkflowRunsAsync_WithCountTooHigh_ReturnsSomeRuns()
    {
        // Arrange
        var count = 10;
        var filter = new Func<WorkflowRunDto, bool>(run => run.RunNumber == 111);

        var service = new GithubApiService(_logger, _httpClientFactory);

        // Act
        var result = await service.GetWorkflowRunsAsync(_owner, _repo, _workflowId, count, filter, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetWorkflowRunsAsync_WithFilter_ReturnsOnlyFilteredRuns()
    {
        // Arrange
        var count = 105;

        var service = new GithubApiService(_logger, _httpClientFactory);

        // Act
        var result = await service.GetWorkflowRunsAsync(_owner, _repo, _workflowId, count, service.SuccessFilter, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(105, result.Count);
        Assert.All(result, run => Assert.Equal("success", run.Conclusion));
    }

    [Fact]
    public async Task GetAllWorkflowRunsAsync_ReturnsAllRuns()
    {
        // Arrange
        var service = new GithubApiService(_logger, _httpClientFactory);

        // Act
        var result = await service.GetAllWorkflowRunsAsync(_owner, _repo, _workflowId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count > 100);
    }

    [Fact]
    public async Task GetWorkflowArtifactsAsync_ReturnsArtifacts()
    {
        // Arrange
        var service = new GithubApiService(_logger, _httpClientFactory);

        // Act
        var result = await service.GetWorkflowArtifactsAsync(_owner, _repo, _runId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(7, result.TotalCount);
    }
}
