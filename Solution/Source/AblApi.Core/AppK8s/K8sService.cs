using System.Text;
using System.Text.Json;
using AblApi.Core.AppNikoBot;
using Microsoft.Extensions.Logging;

namespace AblApi.Core.AppK8s;

public class K8sService(ILogger<K8sService> logger, K8sAppSettings settings, IK8sHttpClient httpClient, INikoBotService nikoBotService) : IK8sService
{
    private readonly ILogger<K8sService> _logger = logger;
    private readonly K8sAppSettings _settings = settings;
    private readonly IK8sHttpClient _httpClient = httpClient;
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

        var url = $"/apis/apps/v1/namespaces/{ns}/deployments/{deployment}";
        using var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
        {
            Content = new StringContent(patch, Encoding.UTF8, "application/strategic-merge-patch+json")
        };

        _logger.LogInformation(
            "RestartDeploymentAsync: patching {Namespace}/{Deployment} with restartedAt={RestartedAt}",
            ns, deployment, restartedAt);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "RestartDeploymentAsync: k8s API returned {StatusCode} for {Namespace}/{Deployment}: {Body}",
                (int)response.StatusCode, ns, deployment, body);
            throw new InvalidOperationException($"Kubernetes API returned {(int)response.StatusCode} while restarting '{name}'.");
        }

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
