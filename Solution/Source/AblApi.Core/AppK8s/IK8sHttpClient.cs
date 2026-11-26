namespace AblApi.Core.AppK8s;

public interface IK8sHttpClient
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}
