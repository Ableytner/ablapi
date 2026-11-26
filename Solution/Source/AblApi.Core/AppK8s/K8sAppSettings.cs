namespace AblApi.Core.AppK8s;

public class K8sAppSettings
{
    public const string SectionName = "K8s";

    public string ApiServer { get; set; } = "https://kubernetes.default.svc";

    public string TokenPath { get; set; } = "/var/run/secrets/kubernetes.io/serviceaccount/token";

    public string CaPath { get; set; } = "/var/run/secrets/kubernetes.io/serviceaccount/ca.crt";

    public List<DeployTarget> DeployTargets { get; set; } = [];
}

public class DeployTarget
{
    public string Name { get; set; } = string.Empty;

    public string Namespace { get; set; } = string.Empty;

    public string Deployment { get; set; } = string.Empty;
}
