using AblApi.Core.AppGithub;
using AblApi.Core.AppGithub.Dtos;
using AblApi.GTNH.DailyVersionSchemas;
using AblApi.GTNH.Dtos;
using AblApi.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace AblApi.GTNH;

/// <summary>
/// Central service for any GTNH related operations.
/// </summary>
public class GTNHService(ILogger<GTNHService> logger, GTNHAppSettings gtnhConfiguration, IGithubService githubApiService, IGTNewHorizonsService gtNewHorizonsService, IAblRepository ablRepository) : IGTNHService
{
    private readonly ILogger<GTNHService> _logger = logger;
    private readonly GTNHAppSettings _gtnhConfiguration = gtnhConfiguration;
    private readonly IGithubService _githubService = githubApiService;
    private readonly IGTNewHorizonsService _gtNewHorizonsService = gtNewHorizonsService;
    private readonly IAblRepository _ablRepository = ablRepository;

    public async Task<bool> TestToken()
    {
        return await _githubService.TestTokenAsync();
    }

    public async Task<int> GetLatestDailyVersionRunNumberAsync(CancellationToken cancellationToken = default)
    {
        var latestRun = await _githubService.GetOneWorkflowRunAsync(
            _gtnhConfiguration.DailyBuildsRepoOwner,
            _gtnhConfiguration.DailyBuildsRepoName,
            _gtnhConfiguration.DailyBuildsWorkflowId,
            cancellationToken);
        if (latestRun == null)
        {
            throw new InvalidOperationException("No workflow runs found for the daily builds workflow. Did the name change?");
        }
        return latestRun.RunNumber;
    }

    public async Task<DailyVersionDto> GetLatestDailyVersionAsync(bool? success = null, CancellationToken cancellationToken = default)
    {
        WorkflowRunDto? latestRun;
        Func<WorkflowRunDto, bool>? filter;
        if (success == null)
        {
            latestRun = await _githubService.GetOneWorkflowRunAsync(
                _gtnhConfiguration.DailyBuildsRepoOwner,
                _gtnhConfiguration.DailyBuildsRepoName,
                _gtnhConfiguration.DailyBuildsWorkflowId,
                cancellationToken);
        }
        else
        {
            latestRun = await _githubService.GetOneWorkflowRunAsync(
                _gtnhConfiguration.DailyBuildsRepoOwner,
                _gtnhConfiguration.DailyBuildsRepoName,
                _gtnhConfiguration.DailyBuildsWorkflowId,
                success.Value ? _githubService.SuccessFilter : _githubService.FailureFilter,
                cancellationToken);
        }

        if (latestRun == null)
        {
            throw new InvalidOperationException("No workflow runs found for the daily builds workflow. Did the name change?");
        }

        return await MapWorkflowRunToDailyVersionDto(latestRun);
    }

    public async Task<DailyVersionDto?> GetSpecificDailyVersionAsync(int dailyVersionNumber, CancellationToken cancellationToken = default)
    {
        var dbRun = await _ablRepository.GTNHDailyVersionRepository.GetByRunNumberAsync(dailyVersionNumber);
        if (dbRun != null)
        {
            return DailyVersionDto.FromDbo(dbRun);
        }

        var workflowRun = await _githubService.GetOneWorkflowRunAsync(
            _gtnhConfiguration.DailyBuildsRepoOwner,
            _gtnhConfiguration.DailyBuildsRepoName,
            _gtnhConfiguration.DailyBuildsWorkflowId,
            run => run.RunNumber == dailyVersionNumber,
            cancellationToken);

        if (workflowRun == null)
        {
            return null;
        }

        var dto = await MapWorkflowRunToDailyVersionDto(workflowRun);

        _ablRepository.GTNHDailyVersionRepository.Add(dto.ToDbo());
        await _ablRepository.SaveChangesAsync();

        return dto;
    }

    public async Task<StableVersionDto> GetLatestStableVersionAsync(CancellationToken cancellationToken = default)
    {
        return await _gtNewHorizonsService.GetLatestStableVersionAsync(cancellationToken);
    }

    public async Task<StableVersionDto?> GetSpecificStableVersionAsync(string version, CancellationToken cancellationToken = default)
    {
        return await _gtNewHorizonsService.GetSpecificStableVersionAsync(version, cancellationToken);
    }

    private async Task<DailyVersionDto> MapWorkflowRunToDailyVersionDto(WorkflowRunDto workflowRun)
    {
        return new DailyVersionDto
        {
            Version = await GetStableVersionForDailyRun(workflowRun),
            RunNumber = workflowRun.RunNumber,
            Success = workflowRun.Conclusion == "success",
            CreatedAt = workflowRun.CreatedAt,
            UpdatedAt = workflowRun.UpdatedAt,
            RunUrl = workflowRun.Url,
            RunUrlHtml = workflowRun.HtmlUrl,
            DownloadUrls = await GetDownloadUrlsAsync(workflowRun)
        };
    }

    private async Task<string> GetStableVersionForDailyRun(WorkflowRunDto run)
    {
        return "2.8.4";

        var stableVersions = await _ablRepository.GTNHStableVersionRepository.GetAllAsListAsync();

        stableVersions.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));

        if (run.CreatedAt > stableVersions[0].CreatedAt)
        {
            return stableVersions[0].Version;
        }

        foreach (var stableVersion in stableVersions)
        {
            if (run.CreatedAt <= stableVersion.CreatedAt)
            {
                return stableVersion.Version;
            }
        }

        throw new Exception($"Daily run {run.RunNumber} is older than all stable versions, this should never happen!");
    }

    private async Task<DownloadUrlsDto?> GetDownloadUrlsAsync(WorkflowRunDto workflowRun, CancellationToken cancellationToken = default)
    {
        var workflowArtifacts = await _githubService.GetWorkflowArtifactsAsync(
            _gtnhConfiguration.DailyBuildsRepoOwner,
            _gtnhConfiguration.DailyBuildsRepoName,
            workflowRun.Id,
            cancellationToken
        );

        var versionSchema = GetDailyVersionSchema(workflowRun.RunNumber);

        var clientUrl = versionSchema.GetClientDownloadUrl(workflowArtifacts);
        var clientJava8Url = versionSchema.GetClientJava8DownloadUrl(workflowArtifacts);
        var serverUrl = versionSchema.GetServerDownloadUrl(workflowArtifacts);
        var serverJava8Url = versionSchema.GetServerJava8DownloadUrl(workflowArtifacts);

        if (clientUrl is null || clientJava8Url is null || serverUrl is null || serverJava8Url is null)
            return null;

        return new DownloadUrlsDto
        {
            Client = clientUrl,
            ClientJava8 = clientJava8Url,
            Server = serverUrl,
            ServerJava8 = serverJava8Url
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
}
