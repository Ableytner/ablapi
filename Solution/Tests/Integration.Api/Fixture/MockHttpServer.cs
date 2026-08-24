using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Integration.Api.Fixture;

/// <summary>
/// A minimal local HTTP server used to mock external API responses in integration tests
/// without requiring changes to the production code being tested.
/// </summary>
public sealed class MockHttpServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _listenTask;

    public string Url { get; }

    public MockHttpServer(string responseBody, string contentType = "application/json")
    {
        (_listener, Url) = StartListenerOnFreePort();

        _listenTask = Task.Run(() => ListenLoop(responseBody, contentType, _cts.Token));
    }

    private async Task ListenLoop(string responseBody, string contentType, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync().WaitAsync(cancellationToken);
                var buffer = Encoding.UTF8.GetBytes(responseBody);

                context.Response.ContentType = contentType;
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, cancellationToken);
                context.Response.OutputStream.Close();
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested || !_listener.IsListening)
            {
                break;
            }
        }
    }

    private static (HttpListener Listener, string Url) StartListenerOnFreePort()
    {
        const int maxAttempts = 20;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var port = GetCandidatePort();
            var url = $"http://127.0.0.1:{port}/";

            var listener = new HttpListener();
            listener.Prefixes.Add(url);

            try
            {
                listener.Start();
                return (listener, url);
            }
            catch (HttpListenerException) when (attempt < maxAttempts)
            {
                // Race condition: port was claimed by someone else between the probe and the bind attempt above.
                // Dispose and retry with a freshly probed port.
                listener.Close();
                Thread.Sleep(Random.Shared.Next(10, 100));
            }
        }

        throw new InvalidOperationException($"Failed to start {nameof(MockHttpServer)} after {maxAttempts} attempts.");
    }

    private static int GetCandidatePort()
    {
        using var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        return ((IPEndPoint)probe.LocalEndpoint).Port;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
        _listener.Close();

        try
        {
            _listenTask.Wait(TimeSpan.FromSeconds(1));
        }
        catch (Exception)
        {
            // ignore exceptions from the background listener loop during shutdown
        }

        _cts.Dispose();
    }
}
