using AblApi.Api;
using AblApi.Core.AppGithub;
using AblApi.Core.AppGithub.Dtos;
using Integration.Api.Fixture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Integration.Api;

public class GithubServiceTests : TestBase
{
    private readonly ILogger<GithubService> _logger;
    private readonly IGithubHttpClient _realHttpClient;
    private readonly IGithubHttpClient _mockHttpClient;

    private readonly string _owner = "GTNewHorizons";
    private readonly string _repo = "DreamAssemblerXXL";
    private readonly string _workflowId = "daily-modpack-build.yml";
    private readonly long _runId = 30980037491L;

    public GithubServiceTests()
    {
        DotEnv.LoadEnvVariables();

        _logger = Substitute.For<ILogger<GithubService>>();

        _realHttpClient = TestHelpers.ApiFactory.Services.GetService<IGithubHttpClient>();

        _mockHttpClient = Substitute.For<IGithubHttpClient>();
        _mockHttpClient.GetAsync(
            $"repos/{_owner}/{_repo}/actions/workflows/{_workflowId}/runs?page=1&per_page=1",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(GetWorkflowRuns_1())
        );
        _mockHttpClient.GetAsync(
            $"repos/{_owner}/{_repo}/actions/workflows/{_workflowId}/runs?page=1&per_page=100",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(GetWorkflowRuns_100_Page1())
        );
        _mockHttpClient.GetAsync(
            $"repos/{_owner}/{_repo}/actions/workflows/{_workflowId}/runs?page=2&per_page=100",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(GetWorkflowRuns_100_Page2())
        );
    }

    [Fact]
    public async Task GetOneWorkflowRunAsync_WithoutFilter_ReturnsFirstRun()
    {
        // Arrange
        var service = new GithubService(_logger, _mockHttpClient);

        // Act
        var result = await service.GetOneWorkflowRunAsync(_owner, _repo, _workflowId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetOneWorkflowRunAsync_WithSuccessFilter_ReturnsFirstSuccessfulRun()
    {
        // Arrange
        var service = new GithubService(_logger, _mockHttpClient);

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
        var service = new GithubService(_logger, _mockHttpClient);

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
        var service = new GithubService(_logger, _mockHttpClient);
        var filter = new Func<WorkflowRunDto, bool>(run => run.RunNumber == 9999);

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

        var service = new GithubService(_logger, _mockHttpClient);

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

        var service = new GithubService(_logger, _mockHttpClient);

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

        var filter = new Func<WorkflowRunDto, bool>(run => run.RunNumber == 666);
        var service = new GithubService(_logger, _mockHttpClient);

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

        var service = new GithubService(_logger, _mockHttpClient);

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
        var service = new GithubService(_logger, _mockHttpClient);

        // Act
        var result = await service.GetAllWorkflowRunsAsync(_owner, _repo, _workflowId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Count);
    }

    [Fact]
    public async Task GetWorkflowArtifactsAsync_ReturnsArtifacts()
    {
        // Arrange
        var service = new GithubService(_logger, _realHttpClient);

        // Act
        var result = await service.GetWorkflowArtifactsAsync(_owner, _repo, _runId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(7, result.TotalCount);
    }

    private HttpResponseMessage GetWorkflowRuns_1()
    {
        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(File.ReadAllText("Data/workflowruns_1.json"))
        };
    }

    private HttpResponseMessage GetWorkflowRuns_100_Page1()
    {
        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(File.ReadAllText("Data/workflowruns_100_page1.json"))
        };
    }

    private HttpResponseMessage GetWorkflowRuns_100_Page2()
    {
        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(File.ReadAllText("Data/workflowruns_100_page2.json"))
        };
    }
}
