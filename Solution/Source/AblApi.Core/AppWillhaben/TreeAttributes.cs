namespace AblApi.Core.AppWillhaben;

/// <summary>
/// Attribute ID 21 — Zustand (condition).
/// </summary>
public enum Zustand
{
    Neu,
    Gebraucht,
    Defekt,
    Ausstellungsstück,
    Neuwertig,
    Generalüberholt,
}

/// <summary>
/// Attribute ID 2535 — Übergabe (transfer/delivery method).
/// </summary>
public enum Übergabe
{
    Selbstabholung,
    Versand,
}

/// <summary>
/// Attribute ID 3199 — Farbe (color).
/// </summary>
public enum Farbe
{
    Mehrfarbig,
    Schwarz,
    Braun,
    Blau,
    Türkis,
    Grün,
    Gelb,
    Orange,
    Rot,
    Rosa,
    Violett,
    Grau,
    Silber,
    Gold,
    Beige,
    Weiß,
}

public static class TreeAttributes
{
    private static readonly Dictionary<string, int> AttributeByName = new()
    {
        { nameof(Zustand), 21 },
        { nameof(Übergabe), 2535 },
        { nameof(Farbe), 3199 }
    };
    private static readonly Dictionary<int, string> AttributeByValue = new()
    {
        { 21, nameof(Zustand) },
        { 2535, nameof(Übergabe) },
        { 3199, nameof(Farbe) }
    };
    private static readonly Dictionary<string, int> AttributeFieldByName = new()
    {
        // Zustand (ID 21)
        { Zustand.Neu.ToString(), 22 },
        { Zustand.Gebraucht.ToString(), 23 },
        { Zustand.Defekt.ToString(), 24 },
        { Zustand.Ausstellungsstück.ToString(), 2539 },
        { Zustand.Neuwertig.ToString(), 2546 },
        { Zustand.Generalüberholt.ToString(), 5013256 },
        // Übergabe (ID 2535)
        { Übergabe.Selbstabholung.ToString(), 2536 },
        { Übergabe.Versand.ToString(), 2537 },
        // Farbe (ID 3199)
        { Farbe.Mehrfarbig.ToString(), 3200 },
        { Farbe.Schwarz.ToString(), 3201 },
        { Farbe.Braun.ToString(), 3202 },
        { Farbe.Blau.ToString(), 3203 },
        { Farbe.Türkis.ToString(), 3204 },
        { Farbe.Grün.ToString(), 3205 },
        { Farbe.Gelb.ToString(), 3206 },
        { Farbe.Orange.ToString(), 3207 },
        { Farbe.Rot.ToString(), 3208 },
        { Farbe.Rosa.ToString(), 3209 },
        { Farbe.Violett.ToString(), 3210 },
        { Farbe.Grau.ToString(), 3211 },
        { Farbe.Silber.ToString(), 3212 },
        { Farbe.Gold.ToString(), 3213 },
        { Farbe.Beige.ToString(), 3214 },
        { Farbe.Weiß.ToString(), 3215 },
    };
    private static readonly Dictionary<int, string> AttributeFieldByValue = new()
    {
        // Zustand (ID 21)
        { 22, Zustand.Neu.ToString() },
        { 23, Zustand.Gebraucht.ToString() },
        { 24, Zustand.Defekt.ToString() },
        { 2539, Zustand.Ausstellungsstück.ToString() },
        { 2546, Zustand.Neuwertig.ToString() },
        { 5013256, Zustand.Generalüberholt.ToString() },
        // Übergabe (ID 2535)
        { 2536, Übergabe.Selbstabholung.ToString() },
        { 2537, Übergabe.Versand.ToString() },
        // Farbe (ID 3199)
        { 3200, Farbe.Mehrfarbig.ToString() },
        { 3201, Farbe.Schwarz.ToString() },
        { 3202, Farbe.Braun.ToString() },
        { 3203, Farbe.Blau.ToString() },
        { 3204, Farbe.Türkis.ToString() },
        { 3205, Farbe.Grün.ToString() },
        { 3206, Farbe.Gelb.ToString() },
        { 3207, Farbe.Orange.ToString() },
        { 3208, Farbe.Rot.ToString() },
        { 3209, Farbe.Rosa.ToString() },
        { 3210, Farbe.Violett.ToString() },
        { 3211, Farbe.Grau.ToString() },
        { 3212, Farbe.Silber.ToString() },
        { 3213, Farbe.Gold.ToString() },
        { 3214, Farbe.Beige.ToString() },
        { 3215, Farbe.Weiß.ToString() },
    };

    // Example input: "21;22,2535;2536,3199;3201"
    public static Dictionary<string, string> ParseValues(string? raw)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(raw))
        {
            return result;
        }

        var s = raw.Trim('[', ']').Trim();
        if (string.IsNullOrEmpty(s))
        {
            return result;
        }

        foreach (var pair in s.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split(';');
            if (parts.Length != 2)
            {
                continue;
            }

            var attrIdRaw = parts[0].Trim();
            var valueIdRaw = parts[1].Trim();

            if (string.IsNullOrEmpty(attrIdRaw) ||
                string.IsNullOrEmpty(valueIdRaw) ||
                !int.TryParse(attrIdRaw, out int attrId) ||
                !int.TryParse(valueIdRaw, out int valueId))
            {
                continue;
            }

            if (!AttributeByValue.TryGetValue(attrId, out var name) ||
                !AttributeFieldByValue.TryGetValue(valueId, out var value))
            {
                continue;
            }

            result[name] = value;
        }

        return result;
    }

    // Example input: "21;22,2535;2536,3199;3201"
    public static string? ParseValue(string? raw, string name)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var s = raw.Trim('[', ']').Trim();
        if (string.IsNullOrEmpty(s))
        {
            return null;
        }

        foreach (var pair in s.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split(';');
            if (parts.Length != 2)
            {
                continue;
            }

            var attrIdRaw = parts[0].Trim();
            var valueIdRaw = parts[1].Trim();

            if (string.IsNullOrEmpty(attrIdRaw) ||
                string.IsNullOrEmpty(valueIdRaw) ||
                !int.TryParse(attrIdRaw, out int attrId) ||
                !int.TryParse(valueIdRaw, out int valueId))
            {
                continue;
            }

            if (!AttributeByValue.TryGetValue(attrId, out var foundName) ||
                !AttributeFieldByValue.TryGetValue(valueId, out var foundValue))
            {
                continue;
            }

            if (foundName == name)
            {
                return foundValue;
            }
        }

        return null;
    }

    // Example input: "Farbe:Weiß,Übergabe:Versand"
    public static Dictionary<int, int> ParseNames(string? raw)
    {
        var result = new Dictionary<int, int>();

        if (string.IsNullOrWhiteSpace(raw))
        {
            return result;
        }

        foreach (var pair in raw.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split(';');
            if (parts.Length != 2)
            {
                continue;
            }

            var name = parts[0].Trim();
            var value = parts[1].Trim();

            if (string.IsNullOrEmpty(name) ||
                string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (!AttributeByName.TryGetValue(name, out var attrId) ||
                !AttributeFieldByName.TryGetValue(value, out var valueId))
            {
                continue;
            }

            result[attrId] = valueId;
        }

        return result;
    }
}
