using AblApi.Core.AppNikoBot;
using Serilog.Core;
using Serilog.Events;

namespace AblApi.Api.Logging;

public class NikoBotSink(Func<INikoBotService> createService) : ILogEventSink
{
    private INikoBotService? _service;

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level < LogEventLevel.Error)
        {
            return;
        }

        _service ??= createService();

        _service.SendLogMessage(
            LogUtil.GetLogLevelString(logEvent.Level),
            logEvent.Properties.TryGetValue("Sender", out LogEventPropertyValue? sender) ? sender.ToString() : "AblApi",
            logEvent.RenderMessage()
        );
    }
}
