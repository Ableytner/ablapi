using System.Text.Json;
using AblApi.Core.AppNikoBot;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace AblApi.Core.AppK8s;

public class K8sService(ILogger<K8sService> logger, K8sAppSettings settings, IKubernetes kubernetes, INikoBotService nikoBotService) : IK8sService
{
    private readonly ILogger<K8sService> _logger = logger;
    private readonly K8sAppSettings _settings = settings;
    private readonly IKubernetes _kubernetes = kubernetes;
    private readonly INikoBotService _nikoBotService = nikoBotService;

    public async Task RestartDeploymentAsync(string name, CancellationToken cancellationToken = default)
    {
        var target = FindTarget(name);

        if (target is null)
        {
            _logger.LogWarning("RestartDeploymentAsync: '{Name}' is not a registered allowlist target.", name);
            throw new KeyNotFoundException($"Deployment target '{name}' is not allowed.");
        }

        var (ns, deployment) = target.Value;

        var restartedAt = DateTime.UtcNow.ToString("o");
        var patch = JsonSerializer.Serialize(new
        {
            spec = new
            {
                template = new
                {
                    metadata = new
                    {
                        annotations = new Dictionary<string, string>
                        {
                            ["kubectl.kubernetes.io/restartedAt"] = restartedAt
                        }
                    }
                }
            }
        });

        _logger.LogInformation(
            "RestartDeploymentAsync: patching {Namespace}/{Deployment} with restartedAt={RestartedAt}",
            ns, deployment, restartedAt);

        await _kubernetes.AppsV1.PatchNamespacedDeploymentAsync(
            new V1Patch(patch, V1Patch.PatchType.StrategicMergePatch),
            deployment,
            ns,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "RestartDeploymentAsync: successfully restarted {Namespace}/{Deployment}.",
            ns, deployment);

        await NotifyDeployAsync(name, ns, deployment, cancellationToken);
    }

    private (string Namespace, string Deployment)? FindTarget(string name)
    {
        foreach (var entry in _settings.DeployTargets)
        {
            if (!DeployTargetsValidator.TryParse(entry, out var ns, out var deployment))
            {
                _logger.LogWarning(
                    "RestartDeploymentAsync: allowlist entry '{Entry}' is malformed; expected 'namespace/deployment'.",
                    entry);
                continue;
            }

            if (string.Equals(deployment, name, StringComparison.OrdinalIgnoreCase))
            {
                return (ns, deployment);
            }
        }

        return null;
    }

    private async Task NotifyDeployAsync(string name, string ns, string deployment, CancellationToken cancellationToken)
    {
        var sent = await _nikoBotService.SendSuccessAsync(
            "Deploy",
            $"Deployed `{name}` ({ns}/{deployment}) from CI",
            "owner",
            cancellationToken: cancellationToken);

        if (!sent)
        {
            _logger.LogWarning("RestartDeploymentAsync: NikoBot notification for '{Name}' was not delivered.", name);
        }
    }
}
