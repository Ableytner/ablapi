using Microsoft.Extensions.Logging;

namespace AblApi.Core.AppK8s;

public static class DeployTargetsValidator
{
    public static bool TryParse(string entry, out string ns, out string deployment)
    {
        ns = string.Empty;
        deployment = string.Empty;

        var slash = entry.IndexOf('/');
        if (slash <= 0 || slash == entry.Length - 1)
        {
            return false;
        }

        ns = entry[..slash];
        deployment = entry[(slash + 1)..];
        return true;
    }

    public static void ValidateAndLog(ILogger logger, IEnumerable<string> targets)
    {
        var validNames = new List<string>();

        foreach (var entry in targets)
        {
            if (TryParse(entry, out var ns, out var deployment))
            {
                validNames.Add(deployment);
            }
            else
            {
                logger.LogWarning(
                    "DeployTargets: entry '{Entry}' is malformed; expected 'namespace/deployment' (e.g. 'ablapi/ablapi-web').",
                    entry);
            }
        }

        logger.LogInformation(
            "DeployTargets: {Count} valid deploy name(s): {Names}",
            validNames.Count,
            validNames.Count > 0 ? string.Join(", ", validNames) : "(none)");
    }
}
