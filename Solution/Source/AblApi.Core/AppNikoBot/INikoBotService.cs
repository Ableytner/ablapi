using AblApi.Core.AppNikoBot.Domain;
using AblApi.Core.AppNikoBot.Enum;

namespace AblApi.Core.AppNikoBot;

public interface INikoBotService
{
    Task<bool> SendEmbedAsync(NikoBotEmbedMessage message, CancellationToken cancellationToken = default);

    bool SendLogMessage(string logLevel, string sender, string message);

    Task<bool> SendSuccessAsync(string title, string message, string channelId, NikoBotColor color = NikoBotColor.Green, NikoBotEmbedField[]? fields = null, CancellationToken cancellationToken = default);

    Task<bool> SendErrorAsync(string title, string message, string channelId, NikoBotColor color = NikoBotColor.Red, NikoBotEmbedField[]? fields = null, CancellationToken cancellationToken = default);

    Task<bool> SendWarningAsync(string title, string message, string channelId, NikoBotColor color = NikoBotColor.Orange, NikoBotEmbedField[]? fields = null, CancellationToken cancellationToken = default);

    Task<bool> SendInfoAsync(string title, string message, string channelId, NikoBotColor color = NikoBotColor.Blue, NikoBotEmbedField[]? fields = null, CancellationToken cancellationToken = default);
}
