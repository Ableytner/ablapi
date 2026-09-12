using AblApi.Common.Extensions;
using AblApi.GTNH.Dtos;
using System.Text.Json;

namespace AblApi.GTNH;

/// <summary>
/// Service for interacting with the gtnewhorizons.com website.
/// </summary>
public class GTNewHorizonsService(GTNHAppSettings appSettings) : IGTNewHorizonsService
{
    private readonly GTNHAppSettings _appSettings = appSettings;

    public async Task<StableVersionDto> GetLatestStableVersionAsync(CancellationToken cancellationToken = default)
    {
        var versionsDict = await GetVersionsJsonAsync(cancellationToken);

        var latestVersion = versionsDict
                                .Where(kvw => Version.TryParse(kvw.Key.Split('-')[0], out _)) // filter out everything not in format X.X.X-alpha|beta
                                .ToList()
                                .SortByVersionDescending(kvw => kvw.Key)
                                .FirstOrDefault();


        if (latestVersion.Equals(default(KeyValuePair<string, VersionsJsonEntryDto>)))
        {
            throw new InvalidOperationException("No stable versions found in the versions.json.");
        }

        return latestVersion.Value.ToDto(latestVersion.Key);
    }

    public async Task<StableVersionDto?> GetSpecificStableVersionAsync(string version, CancellationToken cancellationToken = default)
    {
        var versionsDict = await GetVersionsJsonAsync(cancellationToken);

        if (!versionsDict.TryGetValue(version, out var versionEntry))
        {
            return null;
        }

        return versionEntry.ToDto(version);
    }

    private async Task<VersionsJsonDto> GetVersionsJsonAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(_appSettings.StableVersionApiUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        var versionsDict = JsonSerializer.Deserialize<VersionsJsonDto>(jsonString);

        return versionsDict ?? throw new InvalidOperationException("Failed to deserialize versions.json.");
    }
}
