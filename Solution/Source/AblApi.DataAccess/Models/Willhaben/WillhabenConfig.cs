namespace AblApi.DataAccess.Models.Willhaben;

public class WillhabenConfig
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Keyword { get; set; } = string.Empty;

    public int Category { get; set; }

    public int Rows { get; set; } = 100;

    public int PriceMin { get; set; }

    public int PriceMax { get; set; }

    public int FilterPaylivery { get; set; }

    public string HandoverTypes { get; set; } = string.Empty;

    public string AllowedStates { get; set; } = string.Empty;

    public int KmMax { get; set; }

    public string MustInclude { get; set; } = string.Empty;

    public string MustExclude { get; set; } = string.Empty;

    public bool SortByDistance { get; set; }

    public double ReferenceLat { get; set; }

    public double ReferenceLon { get; set; }

    public double MaxDistanceKm { get; set; }

    public bool IsActive { get; set; } = true;
}
