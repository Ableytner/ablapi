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
        var target = _settings.DeployTargets
            .FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));

        if (target is null
            || string.IsNullOrWhiteSpace(target.Namespace)
            || string.IsNullOrWhiteSpace(target.Deployment))
        {
            _logger.LogWarning("RestartDeploymentAsync: '{Name}' is not a registered allowlist target.", name);
            throw new KeyNotFoundException($"Deployment target '{name}' is not allowed.");
        }

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

        var url = $"/apis/apps/v1/namespaces/{target.Namespace}/deployments/{target.Deployment}";
        using var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
        {
            Content = new StringContent(patch, Encoding.UTF8, "application/strategic-merge-patch+json")
        };

        _logger.LogInformation(
            "RestartDeploymentAsync: patching {Namespace}/{Deployment} (name={Name}) with restartedAt={RestartedAt}",
            target.Namespace, target.Deployment, name, restartedAt);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "RestartDeploymentAsync: k8s API returned {StatusCode} for {Namespace}/{Deployment}: {Body}",
                (int)response.StatusCode, target.Namespace, target.Deployment, body);
            throw new InvalidOperationException($"Kubernetes API returned {(int)response.StatusCode} while restarting '{name}'.");
        }

        _logger.LogInformation(
            "RestartDeploymentAsync: successfully restarted {Namespace}/{Deployment} (name={Name}).",
            target.Namespace, target.Deployment, name);

        await NotifyDeployAsync(name, target, cancellationToken);
    }

    private async Task NotifyDeployAsync(string name, DeployTarget target, CancellationToken cancellationToken)
    {
        var sent = await _nikoBotService.SendSuccessAsync(
            "Deploy",
            $"Deployed `{name}` ({target.Namespace}/{target.Deployment}) from CI",
            "owner",
            cancellationToken: cancellationToken);

        if (!sent)
        {
            _logger.LogWarning("RestartDeploymentAsync: NikoBot notification for '{Name}' was not delivered.", name);
        }
    }
}
