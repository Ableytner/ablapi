using AblApi.Core.AppSpotify.Domain;
using AblApi.DataAccess.Models.Spotify;
using AblApi.Repositories.Interfaces;
using AblApi.Repositories.Interfaces.Spotify;
using Microsoft.Extensions.Logging;

namespace AblApi.Core.AppSpotify;

public class AllPlaylistService(ISpotifyService spotify, IAblRepository ablRepository, ILogger<AllPlaylistService> logger) : IAllPlaylistService
{
    private const int PollIntervalSeconds = 5;

    private readonly ISpotifyService _spotify = spotify;
    private readonly IAblRepository _ablRepository = ablRepository;
    private readonly ILogger<AllPlaylistService> _logger = logger;

    public async Task UpdateAsync(string user, CancellationToken cancellationToken = default)
    {
        var cacheRepo = _ablRepository.SpotifyPlaylistCacheRepository;

        // the user must have an all_playlist to update
        var spotifyUser = await _ablRepository.SpotifyUserRepository.GetByUserIdentifierAsync(user);
        if (spotifyUser == null || string.IsNullOrEmpty(spotifyUser.AllPlaylistId))
        {
            return;
        }

        var playlists = (await _spotify.GetAllPlaylists(user, cancellationToken)).ToList();

        var likedSongsCount = await _spotify.GetLikedSongsCount(user, cancellationToken);
        var likedSongs = new Playlist
        {
            Id = ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId,
            Name = "Liked Songs",
            Description = string.Empty,
            OwnerId = string.Empty,
            SnapshotId = null,
            TrackCount = likedSongsCount,
            Tracks = []
        };
        playlists.Add(likedSongs);

        var allPlaylist = await _spotify.GetPlaylist(user, spotifyUser.AllPlaylistId, cancellationToken);

        // check which playlists changed
        var changedPlaylists = new List<Playlist>();
        foreach (var playlist in playlists)
        {
            if (playlist.Id == allPlaylist.Id)
            {
                continue;
            }

            if (!await LoadCachedTracksAsync(cacheRepo, user, playlist))
            {
                changedPlaylists.Add(playlist);
            }
        }

        // early exit if nothing changed
        if (changedPlaylists.Count == 0)
        {
            _logger.LogInformation("all_playlist for user {User} is up to date.", user);
            return;
        }

        // build the new track set from all source playlists
        var newTracksSet = new TrackSet();
        foreach (var playlist in playlists)
        {
            if (playlist.Id == allPlaylist.Id)
            {
                continue;
            }

            if (changedPlaylists.Contains(playlist))
            {
                playlist.Tracks = (await _spotify.GetPlaylistTracks(user, playlist.Id, cancellationToken)).ToList();
                await UpsertCacheAsync(cacheRepo, user, playlist, playlist.Tracks);
            }

            foreach (var track in playlist.Tracks)
            {
                newTracksSet.Add(track);
            }
        }

        // load the current all_playlist tracks
        if (!await LoadCachedTracksAsync(cacheRepo, user, allPlaylist))
        {
            // a fresh fetch returns newest-first; reverse to oldest-first
            var tracks = (await _spotify.GetPlaylistTracks(user, allPlaylist.Id, cancellationToken)).ToList();
            tracks.Reverse();
            allPlaylist.Tracks = tracks;
        }

        // compute the diff and apply it
        var currentTrackIds = allPlaylist.Tracks.Select(t => t.Id).ToList();
        var newTrackIds = newTracksSet.Ids();

        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(currentTrackIds, newTrackIds);

        if (toRemove.Count > 0)
        {
            await _spotify.RemoveTracks(user, allPlaylist.Id, toRemove.Select(t => $"spotify:track:{t}"), cancellationToken);
        }

        if (toAdd.Count > 0)
        {
            // the newest track needs to be first
            toAdd.Reverse();
            await _spotify.AddTracks(user, allPlaylist.Id, toAdd.Select(t => $"spotify:track:{t}"), cancellationToken);
        }

        if (toAdd.Count == 0 && toRemove.Count == 0)
        {
            _logger.LogInformation("all_playlist for user {User} did not change.", user);
            return;
        }

        // wait for Spotify to finish processing, then refresh the all_playlist meta
        // TODO: possible bug: what if the track count didn't change but the order did?
        var newTracks = newTracksSet.Tracks();
        var remoteTrackCount = 0;
        while (remoteTrackCount != newTracks.Count)
        {
            allPlaylist = await _spotify.GetPlaylist(user, allPlaylist.Id, cancellationToken);
            remoteTrackCount = allPlaylist.TrackCount;

            if (remoteTrackCount != newTracks.Count)
            {
                await Task.Delay(TimeSpan.FromSeconds(PollIntervalSeconds), cancellationToken);
            }
        }

        // update all_playlist cache
        await UpsertCacheAsync(cacheRepo, user, allPlaylist, newTracks);

        _logger.LogInformation(
            "all_playlist for user {User} updated: removed {Removed}, added {Added}.",
            user, toRemove.Count, toAdd.Count);
    }

    private static async Task<bool> LoadCachedTracksAsync(
        ISpotifyPlaylistCacheRepository cacheRepo, string user, Playlist playlist)
    {
        var cached = await cacheRepo.GetByPlaylistIdAsync(user, playlist.Id);
        if (cached is null)
        {
            return false;
        }

        // compare by snapshot id when both playlists have one
        if (playlist.SnapshotId is not null && cached.SnapshotId is not null)
        {
            if (cached.SnapshotId != playlist.SnapshotId)
            {
                return false;
            }
        }
        // fall back to track count
        else if (cached.Tracks.Count != playlist.TrackCount)
        {
            return false;
        }

        playlist.Tracks = cached.Tracks.Select(PlaylistTrack.FromDbo).ToList();
        return true;
    }

    private static async Task UpsertCacheAsync(
        ISpotifyPlaylistCacheRepository cacheRepo, string user, Playlist playlist, IEnumerable<PlaylistTrack> tracks)
    {
        var cacheTracks = tracks
            .Select(t => new SpotifyPlaylistCacheTrack
            {
                TrackId = t.Id,
                AddedAt = t.AddedAt,
            })
            .ToList();

        await cacheRepo.UpsertAsync(user, playlist.Id, playlist.SnapshotId, cacheTracks);
    }
}
