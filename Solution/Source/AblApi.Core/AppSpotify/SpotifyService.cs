using AblApi.Core.AppSpotify.Domain;
using AblApi.Core.AppSpotify.Exceptions;
using SpotifyAPI.Web;

namespace AblApi.Core.AppSpotify;

public class SpotifyService(SpotifyClientFactory clientFactory) : ISpotifyService
{
    private readonly SpotifyClientFactory _clientFactory = clientFactory;

    public async Task<User> GetUserAsync(CancellationToken cancellationToken = default)
    {
        if (_clientFactory.Instance == null)
        {
            throw new UserNotLinkedException();
        }
        var client = _clientFactory.Instance;

        var userDto = await client.UserProfile.Current(cancellationToken);

        return User.FromDto(userDto);
    }

    public async Task<Playlist> CreatePlaylist(string name, string description, bool isPublic = false, CancellationToken cancellationToken = default)
    {
        if (_clientFactory.Instance == null)
        {
            throw new UserNotLinkedException();
        }
        var client = _clientFactory.Instance;

        var request = new PlaylistCreateRequest(name)
        {
            Description = description,
            Public = isPublic
        };

        var dto = await client.Playlists.Create(request, cancellationToken);

        return Playlist.FromDto(dto);
    }

    public async Task<IEnumerable<Playlist>> GetAllPlaylists(CancellationToken cancellationToken = default)
    {
        if (_clientFactory.Instance == null)
        {
            throw new UserNotLinkedException();
        }
        var client = _clientFactory.Instance;

        var paging = await client.Playlists.CurrentUsers(cancellationToken);
        var playlistsDto = await client.PaginateAll(paging, cancellationToken: cancellationToken);

        var user = await GetUserAsync(cancellationToken);

        var playlists = playlistsDto.Select(Playlist.FromDto)
                                    .Where(p => p.OwnerId == user.Id);

        return playlists;
    }

    public async Task<Playlist> GetPlaylist(string id, CancellationToken cancellationToken = default)
    {
        if (_clientFactory.Instance == null)
        {
            throw new UserNotLinkedException();
        }
        var client = _clientFactory.Instance;

        var playlistDto = await client.Playlists.Get(id, cancellationToken);

        return Playlist.FromDto(playlistDto);
    }

    public async Task<int> GetLikedSongsCount(CancellationToken cancellationToken = default)
    {
        if (_clientFactory.Instance == null)
        {
            throw new UserNotLinkedException();
        }
        var client = _clientFactory.Instance;

        var playlistDto = await client.Library.GetTracks(cancellationToken);

        return playlistDto.Total.Value;
    }

    public async Task DeletePlaylist(string uri, CancellationToken cancellationToken)
    {
        if (_clientFactory.Instance == null)
        {
            throw new UserNotLinkedException();
        }
        var client = _clientFactory.Instance;

        await client.Library.RemoveItems(new LibraryRemoveItemsRequest([uri]), cancellationToken);
    }

    public async Task<IEnumerable<PlaylistTrack>> GetPlaylistTracks(string id, CancellationToken cancellationToken)
    {
        if (_clientFactory.Instance == null)
        {
            throw new UserNotLinkedException();
        }
        var client = _clientFactory.Instance;

        var paging = await client.Playlists.GetPlaylistItems(id, cancellationToken);

        var dtos = await client.PaginateAll(paging, cancellationToken: cancellationToken);

        return dtos.Select(PlaylistTrack.FromDto);
    }

    public async Task<IEnumerable<PlaylistTrack>> GetLikedSongsTracks(CancellationToken cancellationToken)
    {
        if (_clientFactory.Instance == null)
        {
            throw new UserNotLinkedException();
        }
        var client = _clientFactory.Instance;

        var paging = await client.Library.GetTracks(cancellationToken);

        var dtos = await client.PaginateAll(paging, cancellationToken: cancellationToken);

        return dtos.Select(PlaylistTrack.FromDto);
    }

    public async Task AddTracks(string id, IEnumerable<string> uris, CancellationToken cancellationToken)
    {
        if (_clientFactory.Instance == null)
        {
            throw new UserNotLinkedException();
        }
        var client = _clientFactory.Instance;

        await client.Playlists.AddPlaylistItems(id, new PlaylistAddItemsRequest(uris.ToList()), cancellationToken);
    }

    public async Task RemoveTracks(string id, IEnumerable<string> uris, CancellationToken cancellationToken)
    {
        if (_clientFactory.Instance == null)
        {
            throw new UserNotLinkedException();
        }
        var client = _clientFactory.Instance;

        var request = new PlaylistRemoveItemsRequest()
        {
            Tracks = uris.Select(u => new PlaylistRemoveItemsRequest.Item() { Uri = u }).ToList()
        };

        await client.Playlists.RemovePlaylistItems(id, request, cancellationToken);
    }
}
