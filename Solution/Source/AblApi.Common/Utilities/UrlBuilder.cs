namespace AblApi.Common.Utilities;

public class UrlBuilder
{
    private string _url = string.Empty;
    private readonly List<(string key, string value)> _params = new();

    public UrlBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    public UrlBuilder WithParam(string key, string value)
    {
        _params.Add((key, value));
        return this;
    }
    public UrlBuilder WithParam<T>(string key, IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            _params.Add((key, value?.ToString() ?? string.Empty));
        }

        return this;
    }

    public UrlBuilder WithOptionalParam(bool shouldAdd, string key, string value)
    {
        return shouldAdd ? WithParam(key, value) : this;
    }
    public UrlBuilder WithOptionalParam<T>(bool shouldAdd, string key, IEnumerable<T> values)
    {
        return shouldAdd ? WithParam(key, values) : this;
    }

    public string Build()
    {
        if (string.IsNullOrEmpty(_url))
        {
            throw new InvalidOperationException("URL must be set before building. Call WithUrl first.");
        }

        if (_params.Count == 0)
        {
            return _url;
        }

        var queryString = string.Join("&", _params.Select(p => $"{Uri.EscapeDataString(p.key)}={Uri.EscapeDataString(p.value)}"));
        return $"{_url}?{queryString}";
    }
}
