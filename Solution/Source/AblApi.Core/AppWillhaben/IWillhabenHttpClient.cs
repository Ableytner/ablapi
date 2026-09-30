using AblApi.Core.AppWillhaben.Dtos;

namespace AblApi.Core.AppWillhaben;

public interface IWillhabenHttpClient
{
    string BuildSearchUrl(WillhabenConfigDto config);

    Task<List<WillhabenListingDto>> GetListingsAsync(string url, CancellationToken cancellationToken = default);
}
