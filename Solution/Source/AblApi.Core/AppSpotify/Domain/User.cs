using SpotifyAPI.Web;

namespace AblApi.Core.AppSpotify.Domain;

public class User
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }

    public string Uri { get => $"spotify:user:{Id}"; }

    public static User FromDto(PublicUser dto)
    {
        return new User()
        {
            Id = dto.Id,
            Name = dto.DisplayName,
            Url = dto.Uri
        };
    }
    public static User FromDto(PrivateUser dto)
    {
        return new User()
        {
            Id = dto.Id,
            Name = dto.DisplayName,
            Url = dto.Uri
        };
    }
}
