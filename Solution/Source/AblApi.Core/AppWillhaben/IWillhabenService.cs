using AblApi.Core.AppWillhaben.Domain;
using AblApi.Core.AppWillhaben.Dtos;

namespace AblApi.Core.AppWillhaben;

public interface IWillhabenService
{
    Task<List<WillhabenListing>> SearchAsync(WillhabenConfigDto config, CancellationToken cancellationToken = default);

    Task<WillhabenConfigDto> CreateConfigAsync(WillhabenConfigDto dto, CancellationToken cancellationToken = default);

    Task<WillhabenConfigDto> UpdateConfigAsync(WillhabenConfigDto dto, CancellationToken cancellationToken = default);

    Task DeleteConfigAsync(string name, CancellationToken cancellationToken = default);

    Task<List<WillhabenConfigDto>> ListConfigsAsync(CancellationToken cancellationToken = default);

    Task AddSeenListingAsync(WillhabenListing listing, CancellationToken cancellationToken = default);

    Task<bool> HasSeenListingAsync(WillhabenListing listing, CancellationToken cancellationToken = default);
}
