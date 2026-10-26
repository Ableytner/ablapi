using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models.Spotify;
using AblApi.Repositories.Interfaces.Spotify;
using Microsoft.EntityFrameworkCore;

namespace AblApi.Repositories.Spotify;

public class SpotifyPlaylistCacheRepository(AblContext context) : GenericRepository<SpotifyPlaylistCache>(context), ISpotifyPlaylistCacheRepository
{
    public async Task<SpotifyPlaylistCache?> GetByPlaylistIdAsync(string user, string playlistId)
    {
        return await Context.SpotifyPlaylistCaches
            .Include(c => c.Tracks)
            .FirstOrDefaultAsync(c => c.SpotifyUser.UserIdentifier == user && c.PlaylistId == playlistId);
    }

    public async Task UpsertAsync(string user, string playlistId, string? snapshotId, IEnumerable<SpotifyPlaylistCacheTrack> tracks)
    {
        var spotifyUser = await Context.SpotifyUsers
            .FirstOrDefaultAsync(u => u.UserIdentifier == user);

        if (spotifyUser == null)
        {
            return;
        }

        var entity = await Context.SpotifyPlaylistCaches
            .FirstOrDefaultAsync(c => c.SpotifyUserId == spotifyUser.Id && c.PlaylistId == playlistId);

        if (entity == null)
        {
            entity = new SpotifyPlaylistCache
            {
                SpotifyUserId = spotifyUser.Id,
                PlaylistId = playlistId,
                SnapshotId = snapshotId,
            };

            foreach (var track in tracks)
            {
                entity.Tracks.Add(track);
            }

            Context.SpotifyPlaylistCaches.Add(entity);
        }
        else
        {
            entity.SnapshotId = snapshotId;

            Context.SpotifyPlaylistCacheTracks.RemoveRange(entity.Tracks);

            foreach (var track in tracks)
            {
                entity.Tracks.Add(track);
            }
        }

        await Context.SaveChangesAsync();
    }

    public async Task RemoveByPlaylistIdAsync(string user, string playlistId)
    {
        var spotifyUser = await Context.SpotifyUsers
            .FirstOrDefaultAsync(u => u.UserIdentifier == user);

        if (spotifyUser == null)
        {
            return;
        }

        var entity = await Context.SpotifyPlaylistCaches
            .FirstOrDefaultAsync(c => c.SpotifyUserId == spotifyUser.Id && c.PlaylistId == playlistId);

        if (entity != null)
        {
            Context.SpotifyPlaylistCaches.Remove(entity);
        }

        await Context.SaveChangesAsync();
    }

    public Task<SpotifyPlaylistCache?> GetLikedSongsAsync(string user)
    {
        return GetByPlaylistIdAsync(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId);
    }

    public Task UpsertLikedSongsAsync(string user, IEnumerable<SpotifyPlaylistCacheTrack> tracks)
    {
        return UpsertAsync(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId, null, tracks);
    }

    public Task RemoveLikedSongsAsync(string user)
    {
        return RemoveByPlaylistIdAsync(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId);
    }
}
