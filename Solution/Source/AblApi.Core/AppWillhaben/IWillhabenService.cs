using AblApi.Core.AppWillhaben.Domain;

namespace AblApi.Core.AppWillhaben;

public interface IWillhabenService
{
    Task<List<WillhabenListing>> SearchAsync(CancellationToken cancellationToken = default);
}
