using AblApi.Core.AppGithub.Dtos;

namespace AblApi.GTNH.DailyVersionSchemas;

public abstract class DailyVersionSchemaBase
{
    protected abstract int MinRunNumber { get; }
    protected abstract int MaxRunNumber { get; }

    protected abstract string ClientArchiveName { get; }
    protected abstract string ClientJava8ArchiveName { get; }
    protected abstract string ServerArchiveName { get; }
    protected abstract string ServerJava8ArchiveName { get; }

    public bool CanHandle(int workflowRunNumber)
    {
        return workflowRunNumber >= MinRunNumber && workflowRunNumber <= MaxRunNumber;
    }

    public string? GetClientDownloadUrl(WorkflowArtifactsDto workflowArtifacts)
    {
        foreach (var artifact in workflowArtifacts.Artifacts)
        {
            if (artifact.Name.EndsWith(ClientArchiveName))
            {
                return artifact.ArchiveDownloadUrl;
            }
        }

        return null;
    }

    public string? GetClientJava8DownloadUrl(WorkflowArtifactsDto workflowArtifacts)
    {
        foreach (var artifact in workflowArtifacts.Artifacts)
        {
            if (artifact.Name.EndsWith(ClientJava8ArchiveName))
            {
                return artifact.ArchiveDownloadUrl;
            }
        }

        return null;
    }

    public string? GetServerDownloadUrl(WorkflowArtifactsDto workflowArtifacts)
    {
        foreach (var artifact in workflowArtifacts.Artifacts)
        {
            if (artifact.Name.EndsWith(ServerArchiveName))
            {
                return artifact.ArchiveDownloadUrl;
            }
        }

        return null;
    }

    public string? GetServerJava8DownloadUrl(WorkflowArtifactsDto workflowArtifacts)
    {
        foreach (var artifact in workflowArtifacts.Artifacts)
        {
            if (artifact.Name.EndsWith(ServerJava8ArchiveName))
            {
                return artifact.ArchiveDownloadUrl;
            }
        }

        return null;
    }
}
