using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models;
using AblApi.Repositories.Interfaces.Willhaben;

namespace AblApi.Repositories.Willhaben;

public class WillhabenSeenListingRepository(AblContext context) : GenericRepository<WillhabenSeenListing>(context), IWillhabenSeenListingRepository
{
    public void AddListing(string url, double price)
    {
        Add(new WillhabenSeenListing
        {
            Url = url,
            Price = price,
        });
    }

    public bool Exists(string url, double price)
    {
        return Context.WillhabenSeenListings.Any(e => e.Url == url && e.Price == price);
    }
}
