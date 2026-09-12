using AblApi.Core;
using Serilog.Core;
using Serilog.Events;

namespace AblApi.Api.Logging;

public class EnvironmentEnricher : ILogEventEnricher
{
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Environment", EnvironmentHelper.GetEnvironment()));
	}
}
