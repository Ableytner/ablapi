using AblApi.GTNH.Dtos;

namespace AblApi.GTNH;

public interface IGTNewHorizonsService
{
    public Task<StableVersionDto> GetLatestStableVersionAsync(CancellationToken cancellationToken = default);

    public Task<StableVersionDto?> GetSpecificStableVersionAsync(string version, CancellationToken cancellationToken = default);
}
