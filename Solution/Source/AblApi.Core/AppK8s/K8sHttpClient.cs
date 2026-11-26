using System.Net.Security;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace AblApi.Core.AppK8s;

public class K8sHttpClient : HttpClient, IK8sHttpClient
{
    private readonly string _tokenPath;
    private readonly ILogger<K8sHttpClient> _logger;
    private bool _tokenMissingWarned;

    public K8sHttpClient(K8sAppSettings settings, ILogger<K8sHttpClient> logger)
        : base(BuildHandler(settings))
    {
        _tokenPath = settings.TokenPath;
        _logger = logger;
        BaseAddress = new Uri(settings.ApiServer);

        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        logger.LogInformation(
            "K8sHttpClient initialized: ApiServer={ApiServer}, tokenPath={TokenPath}, ca={CaPath}",
            settings.ApiServer, settings.TokenPath, settings.CaPath);
    }

    public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = ReadToken();
        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }

    private string? ReadToken()
    {
        try
        {
            if (!File.Exists(_tokenPath))
            {
                if (!_tokenMissingWarned)
                {
                    _logger.LogWarning("K8s SA token file not found at {Path}; requests will be unauthenticated.", _tokenPath);
                    _tokenMissingWarned = true;
                }
                return null;
            }

            return File.ReadAllText(_tokenPath).Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read K8s SA token from {Path}; request will be unauthenticated.", _tokenPath);
            return null;
        }
    }

    private static SocketsHttpHandler BuildHandler(K8sAppSettings settings)
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        };

        var sslOptions = BuildSslOptions(settings.CaPath);
        if (sslOptions != null)
        {
            handler.SslOptions = sslOptions;
        }

        return handler;
    }

    private static SslClientAuthenticationOptions? BuildSslOptions(string caPath)
    {
        if (!File.Exists(caPath))
        {
            return null;
        }

        X509Certificate? ca;
        try
        {
            ca = X509CertificateLoader.LoadCertificate(File.ReadAllBytes(caPath));
        }
        catch (Exception)
        {
            return null;
        }

        var chain = new X509Chain();
        chain.ChainPolicy.ExtraStore.Add(ca);
        chain.ChainPolicy.RevocationMode = X509RevocationMode.Offline;

        return new SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = (sender, cert, _, sslErrors) =>
            {
                if (sslErrors == SslPolicyErrors.None)
                {
                    return true;
                }
                if (cert is not X509Certificate2 cert2)
                {
                    return false;
                }

                lock (chain)
                {
                    return chain.Build(cert2);
                }
            }
        };
    }
}
