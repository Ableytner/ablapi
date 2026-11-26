namespace AblApi.Core.AppK8s;

public interface IK8sService
{
    /// <summary>
    /// Annotates the Deployment's pod template with the current timestamp to force a new rollout.
    /// </summary>
    /// <param name="name">
    /// Deployment name. Must match the deployment portion of one of the
    /// <see cref="K8sAppSettings.DeployTargets"/> entries ("namespace/deployment").
    /// </param>
    Task RestartDeploymentAsync(string name, CancellationToken cancellationToken = default);
}
