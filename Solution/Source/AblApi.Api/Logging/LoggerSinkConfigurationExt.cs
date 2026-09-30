using AblApi.Api.Logging;
using AblApi.Core.AppNikoBot;
using AblApi.Core.AppSettings;
using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace AblApi.Api.Logging;

public static class LoggerSinkConfigurationExt
{
    public static LoggerConfiguration Elasticsearch(this LoggerSinkConfiguration loggerConfiguration, ElkAppSettings elkConfig)
    {
        var elasticsearchHttpUri = new Uri(elkConfig.ElkUrl);

        return loggerConfiguration.Elasticsearch([elasticsearchHttpUri], opts =>
        {
            opts.DataStream = new DataStreamName(elkConfig.DataStreamType, elkConfig.DataStreamDataSet, elkConfig.DataStreamNamespace);
            opts.BootstrapMethod = BootstrapMethod.Failure;
            opts.MinimumLevel = LogUtil.GetLogLevel(elkConfig.LogLevel);
            opts.ConfigureChannel = channelOpts =>
            {
                channelOpts.BufferOptions = new BufferOptions
                {
                };
            };
        }, transport =>
        {
            transport.Authentication(new ApiKey(elkConfig.ApiKey));
        });
    }

    public static LoggerConfiguration NikoBot(this LoggerSinkConfiguration loggerConfiguration, IServiceProvider services)
    {
        return loggerConfiguration.Async(
            opts => opts.Sink(new NikoBotSink(() => services.GetRequiredService<INikoBotService>()), LogEventLevel.Error),
            10000,
            false
        );
    }
}
