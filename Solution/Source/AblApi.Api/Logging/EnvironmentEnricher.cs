using AblApi.Core.AppSettings;
using Serilog.Core;
using Serilog.Events;

namespace AblApi.Api.Logging;

public class EnvironmentEnricher(EnvironmentIdAppSettings options) : ILogEventEnricher
{
	private readonly EnvironmentIdAppSettings _environment = options;

	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		string environment = _environment.Id;
		environment = string.IsNullOrEmpty(environment) ? "NOTDEFINED" : environment;
		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Environment", environment));
	}
}
