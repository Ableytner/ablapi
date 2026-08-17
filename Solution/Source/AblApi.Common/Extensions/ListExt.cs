namespace AblApi.Common.Extensions;

public static class ListExt
{
    public static List<string> SortByVersionDescending(this List<string> list)
    {
        return list.OrderByDescending(GetSuffixToCompare)
                   .OrderByDescending(x => new Version(x.Split('-')[0])).ToList();
    }

    public static List<T> SortByVersionDescending<T>(this List<T> list, Func<T, string> versionSelector)
    {
        return list.OrderByDescending(x => GetSuffixToCompare(versionSelector(x)))
                   .OrderByDescending(x => new Version(versionSelector(x).Split('-')[0])).ToList();
    }

    private static string GetSuffixToCompare(string version)
    {
        var parts = version.Split("-", 2);

        if (parts.Length == 1) 
            return "zzzzzzzzzz";

        return parts[1];
    }
}
