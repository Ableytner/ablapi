using AblApi.Core.AppGithub.Dtos;

namespace AblApi.Core.AppGithub;

public interface IGithubService
{
    public bool SuccessFilter(WorkflowRunDto run);

    public bool FailureFilter(WorkflowRunDto run);

    public Task<bool> TestTokenAsync(CancellationToken cancellationToken = default);

    public Task<WorkflowRunDto?> GetOneWorkflowRunAsync(string owner, string repo, string workflowId, CancellationToken cancellationToken = default);
    public Task<WorkflowRunDto?> GetOneWorkflowRunAsync(string owner, string repo, string workflowId, Func<WorkflowRunDto, bool> filter, CancellationToken cancellationToken = default);

    public Task<List<WorkflowRunDto>> GetWorkflowRunsAsync(string owner, string repo, string workflowId, int count, CancellationToken cancellationToken = default);
    public Task<List<WorkflowRunDto>> GetWorkflowRunsAsync(string owner, string repo, string workflowId, int count, Func<WorkflowRunDto, bool>? filter = null, CancellationToken cancellationToken = default);

    public Task<List<WorkflowRunDto>> GetAllWorkflowRunsAsync(string owner, string repo, string workflowId, CancellationToken cancellationToken = default);
    public Task<List<WorkflowRunDto>> GetAllWorkflowRunsAsync(string owner, string repo, string workflowId, Func<WorkflowRunDto, bool>? filter = null, CancellationToken cancellationToken = default);

    public Task<WorkflowArtifactsDto> GetWorkflowArtifactsAsync(string owner, string repo, long runId, CancellationToken cancellationToken = default);
}
