using Microsoft.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.InMemory;
using System.Net;

namespace AblApi.Common;

public class CachedHttpClient : HttpClient
{
    private static readonly IDictionary<HttpStatusCode, TimeSpan> cacheExpirationPerHttpResponseCode = CacheExpirationProvider.CreateSimple(
            TimeSpan.FromSeconds(60 * 5),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5)
        );
    private static readonly InMemoryCacheHandler innerHandler = new InMemoryCacheHandler(cacheExpirationPerHttpResponseCode: cacheExpirationPerHttpResponseCode);

    public CachedHttpClient() : base(innerHandler)
    {
    }
}
