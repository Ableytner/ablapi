namespace AblApi.Core.AppWillhaben;

public class WillhabenAppSettings
{
    public const string SectionName = "WillhabenConfig";

    public string Keyword { get; set; } = string.Empty;
    public int Category { get; set; }
    public int Rows { get; set; } = 100;

    public int PriceMin { get; set; }
    public int PriceMax { get; set; }

    public bool FilterPaylivery { get; set; }

    public List<string> HandoverTypes { get; set; } = [];

    public List<string> AllowedStates { get; set; } = [];

    public int KmMax { get; set; }

    public List<string> MustInclude { get; set; } = [];
    public List<string> MustExclude { get; set; } = [];

    public bool SortByDistance { get; set; }
    public double ReferenceLat { get; set; }
    public double ReferenceLon { get; set; }
    public double MaxDistanceKm { get; set; }
}
