using Integration.Api.Fixture;

namespace Integration.Api.Extensions;

public static class HttpRequestMessageExt
{
    public static HttpRequestMessage WithAuthProfile(this HttpRequestMessage request, string profile)
    {
        request.Headers.Remove(TestAuthHandler.ProfileHeaderName);
        request.Headers.Add(TestAuthHandler.ProfileHeaderName, profile);
        return request;
    }
}
