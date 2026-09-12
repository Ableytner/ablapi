namespace AblApi.GTNH.DailyVersionSchemas;

public class DailyVersionSchemav2 : DailyVersionSchemaBase
{
    protected override int MinRunNumber { get; } = 501;
    protected override int MaxRunNumber { get; } = 636;

    protected override string ClientArchiveName { get; } = "mmcprism-java17-25.zip";
    protected override string ClientJava8ArchiveName { get; } = "mmcprism-java8.zip";
    protected override string ServerArchiveName { get; } = "server-java17-25.zip";
    protected override string ServerJava8ArchiveName { get; } = "server-java8.zip";
}
