namespace AblApi.Core.AppK8s;

public class K8sAppSettings
{
    public const string SectionName = "K8s";

    public string ApiServer { get; set; } = "https://kubernetes.default.svc";

    public string TokenPath { get; set; } = "/var/run/secrets/kubernetes.io/serviceaccount/token";

    public string CaPath { get; set; } = "/var/run/secrets/kubernetes.io/serviceaccount/ca.crt";

    // of the form "namespace/deployment", CI sends only "deployment"
    public List<string> DeployTargets { get; set; } = [];
}
