using AblApi.Api.Logging;
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
            opts.MinimumLevel = GetLogLevel(elkConfig.LogLevel);
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

    private static LogEventLevel GetLogLevel(string logLevel)
    {
        return logLevel switch
        {
            "Verbose" => LogEventLevel.Verbose,
            "Debug" => LogEventLevel.Debug,
            "Information" => LogEventLevel.Information,
            "Warning" => LogEventLevel.Warning,
            "Error" => LogEventLevel.Error,
            "Fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}
