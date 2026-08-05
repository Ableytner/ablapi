using AblApi.Core.AppGithubApi.Dtos;
using AblApi.GTNH.Dtos;

namespace AblApi.GTNH;

public interface IGTNHManager
{
    public Task<DailyVersionDto> GetLatestDailyVersionAsync(bool? success = null, CancellationToken cancellationToken = default);

    public Task<DailyVersionDto?> GetSpecificDailyVersionAsync(int dailyVersionId, CancellationToken cancellationToken = default);
}
