namespace AblApi.Core.AppK8s;

public interface IK8sService
{
    // Annotates the Deployments' pod template with the current timestamp to force a new rollout and image pull
    Task RestartDeploymentAsync(string name, CancellationToken cancellationToken = default);
}
