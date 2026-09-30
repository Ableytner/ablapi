using AblApi.DataAccess.Models;

namespace AblApi.Repositories.Interfaces.Willhaben;

public interface IWillhabenSeenListingRepository : IGenericRepository<WillhabenSeenListing>
{
    public void AddListing(string url, double price);

    public bool Exists(string url, double price);
}
