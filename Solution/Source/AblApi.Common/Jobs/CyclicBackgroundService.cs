using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AblApi.Common.Jobs;

public abstract class CyclicBackgroundService(ILogger logger) : BackgroundService
{
	protected readonly ILogger Logger = logger;
	protected abstract string Name { get; }
	protected abstract TimeSpan CycleTime { get; }

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			await Initialize();
		}
		catch (Exception ex)
		{
			Logger.LogError("{Name}.{Function} - Init: Caught Exception {ExceptionMessage}, {Exception}", Name, nameof(ExecuteAsync), ex.Message, ex);
			throw;
		}

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await Cyclic();
			}
			catch (Exception ex)
			{
				Logger.LogError("{Name}.{Function} - Cyclic caught Exception {ExceptionMessage}, {Exception}", Name, nameof(ExecuteAsync), ex.Message, ex);
			}

			await Task.Delay(CycleTime, stoppingToken);

			Logger.LogDebug("{Name}.{Function} - running {UtcNow} on thread #{ThreadId}", Name, nameof(ExecuteAsync), DateTimeOffset.UtcNow, Environment.CurrentManagedThreadId);
		}

		Logger.LogWarning("{Name}.{Function} - stopped {UtcNow}", Name, nameof(ExecuteAsync), DateTimeOffset.UtcNow);
	}

	protected abstract Task Initialize();

	protected abstract Task Cyclic();
}
