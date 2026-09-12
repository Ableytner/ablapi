namespace AblApi.GTNH.DailyVersionSchemas;

public class DailyVersionSchemav3 : DailyVersionSchemaBase
{
    protected override int MinRunNumber { get; } = 637;
    protected override int MaxRunNumber { get; } = 9999;

    protected override string ClientArchiveName { get; } = "mmcprism-java17-26.zip";
    protected override string ClientJava8ArchiveName { get; } = "mmcprism-java8.zip";
    protected override string ServerArchiveName { get; } = "server-java17-26.zip";
    protected override string ServerJava8ArchiveName { get; } = "server-java8.zip";
}
