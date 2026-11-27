namespace AblApi.Core.AppK8s;

public class K8sAppSettings
{
    public const string SectionName = "K8s";

    // of the form "namespace/deployment", CI sends only "deployment"
    public List<string> DeployTargets { get; set; } = [];
}
