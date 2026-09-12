namespace AblApi.GTNH.DailyVersionSchemas;

public class DailyVersionSchemav1 : DailyVersionSchemaBase
{
    protected override int MinRunNumber { get; } = 1;
    protected override int MaxRunNumber { get; } = 500;

    protected override string ClientArchiveName { get; } = "mmcprism-new-java";
    protected override string ClientJava8ArchiveName { get; } = "mmcprism-java8";
    protected override string ServerArchiveName { get; } = "server-new-java";
    protected override string ServerJava8ArchiveName { get; } = "server";
}
