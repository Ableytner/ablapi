using AblApi.Core.AppNikoBot.Domain;
using AblApi.Core.AppNikoBot.Dtos;
using AblApi.Core.AppNikoBot.Enum;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AblApi.Core.AppNikoBot;

/// <summary>
/// Service for sending Discord embed messages via the NikoBot inbox HTTP endpoint.
/// </summary>
public class NikoBotService(ILogger<NikoBotService> logger, NikoBotAppSettings settings) : INikoBotService
{
    private readonly ILogger<NikoBotService> _logger = logger;
    private readonly NikoBotAppSettings _settings = settings;

    public async Task<bool> SendEmbedAsync(NikoBotEmbedMessage message, CancellationToken cancellationToken = default)
    {
        var payload = new InboxPayload
        {
            Title = message.Title,
            Message = message.Message,
            Color = message.Color.GetHexValue(),
            ChannelId = message.ChannelId,
            Fields = message.Fields?.Select(f => new InboxField
            {
                Name = f.Name,
                Value = f.Value,
                Inline = f.Inline
            }).ToArray()
        };

        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl + "/api/inbox")
            {
                Content = JsonContent.Create(payload, options: jsonOptions)
            };
            request.Headers.Add("X-Api-Secret", _settings.ApiSecret);

            using var httpClient = new HttpClient();
            var response = await httpClient.SendAsync(request, cancellationToken);
            var success = response.IsSuccessStatusCode;

            if (!success)
            {
                _logger.LogWarning("NikoBot inbox request failed with status code {StatusCode}: {ErrorMessage}", response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send embed message to NikoBot inbox");
            return false;
        }
    }

    public bool SendLogMessage(string logLevel, string sender, string message)
    {
        // this would create an infinite loop if NikoBot communication fails
        if (message.Contains("Failed to send embed message to NikoBot inbox"))
        {
            return true;
        }

        var payload = new InboxPayload
        {
            Title = "",
            Message = $"Received {logLevel} from {sender}:\n```\n{message}\n```",
            Color = NikoBotColor.Red.GetHexValue(),
            ChannelId = "owner"
        };

        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl + "/api/inbox")
            {
                Content = JsonContent.Create(payload, options: jsonOptions)
            };
            request.Headers.Add("X-Api-Secret", _settings.ApiSecret);

            using var httpClient = new HttpClient();
            var response = httpClient.Send(request);
            var success = response.IsSuccessStatusCode;

            if (!success)
            {
                _logger.LogWarning("NikoBot inbox request failed with status code {StatusCode}: {ErrorMessage}", response.StatusCode, response.Content.ToString());
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send embed message to NikoBot inbox");
            return false;
        }
    }

    public Task<bool> SendSuccessAsync(string title, string message, string channelId, NikoBotColor color = NikoBotColor.Green, NikoBotEmbedField[]? fields = null, CancellationToken cancellationToken = default)
    {
        return SendEmbedAsync(new NikoBotEmbedMessage
        {
            Title = title,
            Message = message,
            Color = color,
            ChannelId = channelId,
            Fields = fields
        }, cancellationToken);
    }

    public Task<bool> SendErrorAsync(string title, string message, string channelId, NikoBotColor color = NikoBotColor.Red, NikoBotEmbedField[]? fields = null, CancellationToken cancellationToken = default)
    {
        return SendEmbedAsync(new NikoBotEmbedMessage
        {
            Title = title,
            Message = message,
            Color = color,
            ChannelId = channelId,
            Fields = fields
        }, cancellationToken);
    }

    public Task<bool> SendWarningAsync(string title, string message, string channelId, NikoBotColor color = NikoBotColor.Orange, NikoBotEmbedField[]? fields = null, CancellationToken cancellationToken = default)
    {
        return SendEmbedAsync(new NikoBotEmbedMessage
        {
            Title = title,
            Message = message,
            Color = color,
            ChannelId = channelId,
            Fields = fields
        }, cancellationToken);
    }

    public Task<bool> SendInfoAsync(string title, string message, string channelId, NikoBotColor color = NikoBotColor.Blue, NikoBotEmbedField[]? fields = null, CancellationToken cancellationToken = default)
    {
        return SendEmbedAsync(new NikoBotEmbedMessage
        {
            Title = title,
            Message = message,
            Color = color,
            ChannelId = channelId,
            Fields = fields
        }, cancellationToken);
    }
}
