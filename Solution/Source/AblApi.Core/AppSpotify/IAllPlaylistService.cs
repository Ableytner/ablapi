namespace AblApi.Core.AppSpotify;

public interface IAllPlaylistService
{
    Task UpdateAsync(string user, CancellationToken cancellationToken = default);
}
