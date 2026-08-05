using AblApi.Core.AppGithubApi;
using AblApi.Core.AppGithubApi.Dtos;
using AblApi.GTNH.DailyVersionSchemas;
using AblApi.GTNH.Dtos;
using Microsoft.Extensions.Logging;

namespace AblApi.GTNH;

public class GTNHManager(ILogger<GTNHManager> logger, GTNHAppSettings gtnhConfiguration, IGithubApiService githubApiService) : IGTNHManager
{
    private readonly ILogger<GTNHManager> _logger = logger;
    private readonly GTNHAppSettings _gtnhConfiguration = gtnhConfiguration;
    private readonly IGithubApiService _githubApiService = githubApiService;

    public async Task<DailyVersionDto> GetLatestDailyVersionAsync(bool? success = null, CancellationToken cancellationToken = default)
    {
        Func<WorkflowRunDto, bool>? filter;
        if (success == null)
        {
            filter = null;
        }
        else
        {
            filter = success.Value ? _githubApiService.SuccessFilter : _githubApiService.FailureFilter;
        }

        var latestRun = await _githubApiService.GetOneWorkflowRunAsync(
            _gtnhConfiguration.DailyBuildsRepoOwner,
            _gtnhConfiguration.DailyBuildsRepoName,
            _gtnhConfiguration.DailyBuildsWorkflowId,
            filter,
            cancellationToken);

        if (latestRun == null)
        {
            throw new InvalidOperationException("No workflow runs found for the daily builds workflow. Did the name change?");
        }

        return await MapWorkflowRunToDailyVersionDto(latestRun);
    }

    public async Task<DailyVersionDto?> GetSpecificDailyVersionAsync(int dailyVersionNumber, CancellationToken cancellationToken = default)
    {
        var workflowRun = await _githubApiService.GetOneWorkflowRunAsync(
            _gtnhConfiguration.DailyBuildsRepoOwner,
            _gtnhConfiguration.DailyBuildsRepoName,
            _gtnhConfiguration.DailyBuildsWorkflowId,
            run => run.RunNumber == dailyVersionNumber,
            cancellationToken);

        if (workflowRun == null)
        {
            return null;
        }

        return await MapWorkflowRunToDailyVersionDto(workflowRun);
    }

    private async Task<DailyVersionDto> MapWorkflowRunToDailyVersionDto(WorkflowRunDto workflowRun)
    {
        return new DailyVersionDto
        {
            Version = await GetStableVersionForDailyRun(workflowRun.RunNumber),
            RunNumber = workflowRun.RunNumber,
            Success = workflowRun.Conclusion == "success",
            CreatedAt = workflowRun.CreatedAt,
            UpdatedAt = workflowRun.UpdatedAt,
            RunUrl = workflowRun.Url,
            RunUrlHtml = workflowRun.HtmlUrl,
            DownloadUrls = await GetDownloadUrlsAsync(workflowRun)
        };
    }

    private static DailyVersionSchemaBase GetDailyVersionSchema(int workflowRunNumber)
    {
        var schemas = new List<DailyVersionSchemaBase>() {
            new DailyVersionSchemav1(),
            new DailyVersionSchemav2(),
            new DailyVersionSchemav3()
        };

        foreach (var schema in schemas)
        {
            if (schema.CanHandle(workflowRunNumber))
            {
                return schema;
            }
        }

        throw new InvalidOperationException($"No schema found for workflow run number {workflowRunNumber}, a new one needs to be added!");
    }

    private async Task<DownloadUrlsDto?> GetDownloadUrlsAsync(WorkflowRunDto workflowRun, CancellationToken cancellationToken = default)
    {
        var workflowArtifacts = await _githubApiService.GetWorkflowArtifactsAsync(
            _gtnhConfiguration.DailyBuildsRepoOwner,
            _gtnhConfiguration.DailyBuildsRepoName,
            workflowRun.Id,
            cancellationToken
        );

        var versionSchema = GetDailyVersionSchema(workflowRun.RunNumber);

        var clientUrl = versionSchema.GetClientDownloadUrl(workflowArtifacts);
        var serverUrl = versionSchema.GetServerDownloadUrl(workflowArtifacts);

        if (clientUrl == null || serverUrl == null)
        {
            return null;
        }

        return new DownloadUrlsDto
        {
            Client = clientUrl,
            Server = serverUrl
        };
    }

    private async Task<string> GetStableVersionForDailyRun(int runNumber, CancellationToken cancellationToken = default)
    {
        // TODO: determine stable version based on daily run date
        return "2.9.0";
    }
}
