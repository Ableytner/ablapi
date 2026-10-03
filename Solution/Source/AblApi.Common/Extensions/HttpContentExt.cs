namespace AblApi.Common.Extensions;

public static class HttpContentExt
{
    public static string ReadAsString(this HttpContent content)
    {
        // from: https://stackoverflow.com/a/77132945
        var responseStream = content.ReadAsStream();       
        using var memoryStream = new MemoryStream();
        responseStream.CopyTo(memoryStream);
        return System.Text.Encoding.Default.GetString(memoryStream.ToArray());
    }
}
