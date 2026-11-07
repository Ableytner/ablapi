using AblApi.Core.AppSpotify.Domain;
using AblApi.Repositories.Interfaces;
using AblApi.Repositories.Interfaces.Spotify;
using SpotifyAPI.Web;

namespace AblApi.Core.AppSpotify;

public class SpotifyService(SpotifyClientFactory clientFactory, IAblRepository ablRepository, SpotifyAppSettings appSettings) : ISpotifyService
{
    private const int BatchSize = 100;

    private readonly SpotifyClientFactory _clientFactory = clientFactory;
    private readonly IAblRepository _ablRepository = ablRepository;
    private readonly SpotifyAppSettings _appSettings = appSettings;

    public async Task<User> GetUserAsync(string user, CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.GetClient(user);

        var userDto = await client.UserProfile.Current(cancellationToken);

        return User.FromDto(userDto);
    }

    public async Task<Playlist> CreatePlaylist(string user, string name, string description, bool isPublic, CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.GetClient(user);

        var request = new PlaylistCreateRequest(name)
        {
            Description = description,
            Public = isPublic
        };

        var dto = await client.Playlists.Create(request, cancellationToken);

        return Playlist.FromDto(dto);
    }

    public async Task<IEnumerable<Playlist>> GetAllPlaylists(string user, CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.GetClient(user);

        var paging = await client.Playlists.CurrentUsers(cancellationToken);
        var playlistsDto = await client.PaginateAll(paging, cancellationToken: cancellationToken);

        var spotifyUser = await GetUserAsync(user, cancellationToken);

        var playlists = playlistsDto.Select(Playlist.FromDto)
                                    .Where(p => p.OwnerId == spotifyUser.Id);

        return playlists;
    }

    public async Task<Playlist> GetPlaylist(string user, string id, CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.GetClient(user);

        var playlistDto = await client.Playlists.Get(id, cancellationToken);

        return Playlist.FromDto(playlistDto);
    }

    public async Task<int> GetLikedSongsCount(string user, CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.GetClient(user);

        var playlistDto = await client.Library.GetTracks(cancellationToken);

        return playlistDto.Total.Value;
    }

    public async Task DeletePlaylist(string user, string id, CancellationToken cancellationToken)
    {
        var client = _clientFactory.GetClient(user);

        // Spotify's docs point to DELETE /me/library, which the library cannot call correctly for playlists
        #pragma warning disable CS0618
        await client.Follow.UnfollowPlaylist(id, cancellationToken);
        #pragma warning restore CS0618
    }

    public async Task<IEnumerable<PlaylistTrack>> GetPlaylistTracks(string user, string id, CancellationToken cancellationToken)
    {
        if (id == ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId)
        {
            return await GetLikedSongsTracks(user, cancellationToken);
        }

        var client = _clientFactory.GetClient(user);

        var paging = await client.Playlists.GetPlaylistItems(id, cancellationToken);

        var dtos = await client.PaginateAll(paging, cancellationToken: cancellationToken);

        return dtos.Select(PlaylistTrack.FromDto);
    }

    public async Task<IEnumerable<PlaylistTrack>> GetLikedSongsTracks(string user, CancellationToken cancellationToken)
    {
        var client = _clientFactory.GetClient(user);

        var paging = await client.Library.GetTracks(cancellationToken);

        var dtos = await client.PaginateAll(paging, cancellationToken: cancellationToken);

        return dtos.Select(PlaylistTrack.FromDto);
    }

    public async Task AddTracks(string user, string id, IEnumerable<string> uris, CancellationToken cancellationToken)
    {
        var client = _clientFactory.GetClient(user);

        foreach (var chunk in uris.ToList().Chunk(BatchSize))
        {
            await client.Playlists.AddPlaylistItems(id, new PlaylistAddItemsRequest(chunk.ToList()), cancellationToken);
        }
    }

    public async Task RemoveTracks(string user, string id, IEnumerable<string> uris, CancellationToken cancellationToken)
    {
        var client = _clientFactory.GetClient(user);

        foreach (var chunk in uris.ToList().Chunk(BatchSize))
        {
            var request = new PlaylistRemoveItemsRequestV2()
            {
                Items = chunk.Select(u => new PlaylistRemoveItemsRequestV2.Item() { Uri = u }).ToList()
            };

            await client.Playlists.RemovePlaylistItems(id, request, cancellationToken);
        }
    }

    public async Task<Playlist> CreateAllPlaylistAsync(string user, CancellationToken cancellationToken = default)
    {
        var playlist = await CreatePlaylist(
            user,
            _appSettings.AllPlaylistName,
            "Playlist with all songs from all your playlists, created by github.com/Ableytner/ablapi",
            false,
            cancellationToken);

        await _ablRepository.SpotifyUserRepository.SetAllPlaylistIdAsync(user, playlist.Id);

        return playlist;
    }
}
