using AblApi.Core.AppWillhaben.Domain;
using AblApi.Core.AppWillhaben.Dtos;

namespace AblApi.Core.AppWillhaben;

public interface IWillhabenService
{
    Task<List<WillhabenListing>> SearchAsync(WillhabenConfigDto config, CancellationToken cancellationToken = default);
}
