using System.Net.Http.Headers;
using System.Text;

namespace Lib;

public class Http
{
    public static HttpClient JsonClient(Action<HttpRequestHeaders> setHeaders)
    {
        HttpClient client = new();

        client.DefaultRequestHeaders.Add("Accept", "application/json");
        setHeaders(client.DefaultRequestHeaders);

        return client;
    }

    public static async Task<HttpResponseMessage> JsonPost(HttpClient client, string url, string body)
    {
        return await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
    }
}

public static class CompactExtension
{
    /// <summary>
    /// Remove all white spaces from string.
    /// Mostly used with JSON string.
    /// </summary>
    public static string Compact(this string source)
    {
        var builder = new StringBuilder(source.Length);
        for (int i = 0; i < source.Length; ++i)
        {
            char c = source[i];
            if (!char.IsWhiteSpace(c)) builder.Append(c);
        }
        return source.Length == builder.Length ? source : builder.ToString();
    }
}