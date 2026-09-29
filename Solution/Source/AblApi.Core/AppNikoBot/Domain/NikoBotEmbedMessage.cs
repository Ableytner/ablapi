using AblApi.Core.AppNikoBot.Enum;

namespace AblApi.Core.AppNikoBot.Domain;

public class NikoBotEmbedMessage
{
    public required string Title { get; set; }

    public required string Message { get; set; }

    public required NikoBotColor Color { get; set; }

    public required string ChannelId { get; set; }

    public NikoBotEmbedField[]? Fields { get; set; }
}

public class NikoBotEmbedField
{
    public required string Name { get; set; }

    public required string Value { get; set; }

    public bool Inline { get; set; }
}
