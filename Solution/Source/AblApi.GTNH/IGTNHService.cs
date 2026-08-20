using AblApi.GTNH.Dtos;

namespace AblApi.GTNH;

public interface IGTNHService
{
    public Task<bool> TestToken();

    public Task<int> GetLatestDailyVersionRunNumberAsync(CancellationToken cancellationToken = default);

    public Task<DailyVersionDto> GetLatestDailyVersionAsync(bool? success = null, CancellationToken cancellationToken = default);

    public Task<DailyVersionDto?> GetSpecificDailyVersionAsync(int dailyVersionId, CancellationToken cancellationToken = default);
}
