using AblApi.Core.AppGithub.Dtos;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AblApi.Core.AppGithub;

public class GithubService(ILogger<GithubService> logger, IGithubHttpClient httpClient) : IGithubService
{
    private readonly int _workflowsPerPage = 100;

    private readonly ILogger<GithubService> _logger = logger;
    private readonly IGithubHttpClient _httpClient = httpClient;

    public bool SuccessFilter(WorkflowRunDto run) => run.Conclusion == "success";

    public bool FailureFilter(WorkflowRunDto run) => run.Conclusion == "failure";

    public async Task<bool> TestTokenAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.TestToken();

        if (!result)
        {
            _logger.LogError("Failed to connect to Github API. Please check your Github token and network connectivity.");
        }

        return result;
    }

    public async Task<WorkflowRunDto?> GetOneWorkflowRunAsync(string owner, string repo, string workflowId, CancellationToken cancellationToken = default)
    {
        var workflowRuns = await GetPagedWorkflowRunsAsync(owner, repo, workflowId, 1, 1, cancellationToken);

        return workflowRuns.WorkflowRuns.First();
    }
    public async Task<WorkflowRunDto?> GetOneWorkflowRunAsync(string owner, string repo, string workflowId, Func<WorkflowRunDto, bool> filter, CancellationToken cancellationToken = default)
    {
        var workflowRuns = await GetPagedWorkflowRunsAsync(owner, repo, workflowId, 1, _workflowsPerPage, cancellationToken);

        int seenItems = workflowRuns.WorkflowRuns.Length;
        int totalItems = workflowRuns.TotalCount;
        int page = 2;

        if (filter != null)
        {
            workflowRuns.WorkflowRuns = workflowRuns.WorkflowRuns.Where(filter).ToArray();
        }

        if (workflowRuns.WorkflowRuns.Any())
        {
            return workflowRuns.WorkflowRuns.First();
        }

        while (seenItems < totalItems)
        {
            workflowRuns = await GetPagedWorkflowRunsAsync(owner, repo, workflowId, page, _workflowsPerPage, cancellationToken);

            seenItems += workflowRuns.WorkflowRuns.Length;

            if (filter != null)
            {
                workflowRuns.WorkflowRuns = workflowRuns.WorkflowRuns.Where(filter).ToArray();
            }

            if (workflowRuns.WorkflowRuns.Any())
            {
                return workflowRuns.WorkflowRuns.First();
            }

            page++;
        }

        return null;
    }

    public async Task<List<WorkflowRunDto>> GetWorkflowRunsAsync(string owner, string repo, string workflowId, int count, CancellationToken cancellationToken = default)
    {
        return await GetWorkflowRunsAsync(owner, repo, workflowId, count, null, cancellationToken);
    }
    public async Task<List<WorkflowRunDto>> GetWorkflowRunsAsync(string owner, string repo, string workflowId, int count, Func<WorkflowRunDto, bool>? filter = null, CancellationToken cancellationToken = default)
    {
        var workflowRuns = GetPagedWorkflowRunsAsync(owner, repo, workflowId, 1, _workflowsPerPage, cancellationToken).Result;

        int seenItems = workflowRuns.WorkflowRuns.Length;
        int page = 2;

        if (filter != null)
        {
            workflowRuns.WorkflowRuns = workflowRuns.WorkflowRuns.Where(filter).ToArray();
        }
        var requestedRuns = new List<WorkflowRunDto>(workflowRuns.WorkflowRuns.Take(Math.Min(workflowRuns.WorkflowRuns.Length, count)));

        while (seenItems < workflowRuns.TotalCount && requestedRuns.Count < count)
        {
            workflowRuns = await GetPagedWorkflowRunsAsync(owner, repo, workflowId, page, _workflowsPerPage, cancellationToken);

            seenItems += workflowRuns.WorkflowRuns.Length;

            if (filter != null)
            {
                workflowRuns.WorkflowRuns = workflowRuns.WorkflowRuns.Where(filter).ToArray();
            }
            requestedRuns.AddRange(workflowRuns.WorkflowRuns.Take(count - requestedRuns.Count));

            page++;
        }

        return requestedRuns;
    }

    public async Task<List<WorkflowRunDto>> GetAllWorkflowRunsAsync(string owner, string repo, string workflowId, CancellationToken cancellationToken = default)
    {
        return await GetAllWorkflowRunsAsync(owner, repo, workflowId, null, cancellationToken);
    }
    public async Task<List<WorkflowRunDto>> GetAllWorkflowRunsAsync(string owner, string repo, string workflowId, Func<WorkflowRunDto, bool>? filter = null, CancellationToken cancellationToken = default)
    {
        var workflowRuns = await GetPagedWorkflowRunsAsync(owner, repo, workflowId, 1, _workflowsPerPage, cancellationToken);

        int seenItems = workflowRuns.WorkflowRuns.Length;
        int totalItems = workflowRuns.TotalCount;
        int page = 2;

        if (filter != null)
        {
            workflowRuns.WorkflowRuns = workflowRuns.WorkflowRuns.Where(filter).ToArray();
        }
        var requestedRuns = new List<WorkflowRunDto>(workflowRuns.WorkflowRuns);

        while (seenItems < totalItems)
        {
            workflowRuns = await GetPagedWorkflowRunsAsync(owner, repo, workflowId, page, _workflowsPerPage, cancellationToken);

            seenItems += workflowRuns.WorkflowRuns.Length;

            if (filter != null)
            {
                workflowRuns.WorkflowRuns = workflowRuns.WorkflowRuns.Where(filter).ToArray();
            }

            requestedRuns.AddRange(workflowRuns.WorkflowRuns);

            page++;
        }

        return requestedRuns;
    }

    public async Task<WorkflowArtifactsDto> GetWorkflowArtifactsAsync(string owner, string repo, long runId, CancellationToken cancellationToken = default)
    {
        var url = $"repos/{owner}/{repo}/actions/runs/{runId}/artifacts";
        _logger.LogInformation("Fetching workflow artifacts from {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var workflowArtifacts = JsonSerializer.Deserialize<WorkflowArtifactsDto>(content);
        return workflowArtifacts ?? throw new InvalidOperationException("Failed to deserialize workflow artifacts.");
    }

    private async Task<WorkflowRunsDto> GetPagedWorkflowRunsAsync(string owner, string repo, string workflowId, int page, int perPage, CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page number must be greater than or equal to 1.");
        }
        if (perPage < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(perPage), "perPage must be greater than or equal to 1.");
        }
        if (perPage > _workflowsPerPage)
        {
            throw new ArgumentOutOfRangeException(nameof(perPage), $"perPage cannot be greater than {_workflowsPerPage}.");
        }

        var url = $"repos/{owner}/{repo}/actions/workflows/{workflowId}/runs?page={page}&per_page={perPage}";
        _logger.LogInformation("Fetching paged workflow runs from {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var workflowRuns = JsonSerializer.Deserialize<WorkflowRunsDto>(content);
        return workflowRuns ?? throw new InvalidOperationException("Failed to deserialize workflow runs.");
    }
}
