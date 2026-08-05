using AblApi.Core.AppGithubApi.Dtos;

namespace AblApi.Core.AppGithubApi;

public interface IGithubApiService
{
    public bool SuccessFilter(WorkflowRunDto run);

    public bool FailureFilter(WorkflowRunDto run);

    public Task<WorkflowRunDto?> GetOneWorkflowRunAsync(string owner, string repo, string workflowId, CancellationToken cancellationToken = default);
    public Task<WorkflowRunDto?> GetOneWorkflowRunAsync(string owner, string repo, string workflowId, Func<WorkflowRunDto, bool>? filter = null, CancellationToken cancellationToken = default);

    public Task<List<WorkflowRunDto>> GetWorkflowRunsAsync(string owner, string repo, string workflowId, int count, CancellationToken cancellationToken = default);
    public Task<List<WorkflowRunDto>> GetWorkflowRunsAsync(string owner, string repo, string workflowId, int count, Func<WorkflowRunDto, bool>? filter = null, CancellationToken cancellationToken = default);

    public Task<List<WorkflowRunDto>> GetAllWorkflowRunsAsync(string owner, string repo, string workflowId, CancellationToken cancellationToken = default);
    public Task<List<WorkflowRunDto>> GetAllWorkflowRunsAsync(string owner, string repo, string workflowId, Func<WorkflowRunDto, bool>? filter = null, CancellationToken cancellationToken = default);

    public Task<WorkflowArtifactsDto> GetWorkflowArtifactsAsync(string owner, string repo, long runId, CancellationToken cancellationToken = default);
}
