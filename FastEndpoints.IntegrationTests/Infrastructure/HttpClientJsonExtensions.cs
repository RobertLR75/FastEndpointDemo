using System.Net.Http.Json;
using System.Text.Json;

namespace FastEndpoints.IntegrationTests.Infrastructure;

internal static class HttpClientJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string url, T body, CancellationToken ct)
        => client.PostAsJsonAsync(url, body, JsonOptions, ct);

    public static Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string url, T body, CancellationToken ct)
        => client.PutAsJsonAsync(url, body, JsonOptions, ct);

    public static Task<T?> ReadJsonAsync<T>(this HttpContent content, CancellationToken ct)
        => content.ReadFromJsonAsync<T>(JsonOptions, ct);
}
