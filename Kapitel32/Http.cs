using System.Net;

namespace Library.Lending;

public sealed class CatalogClient(HttpClient http)
{
    // Expected failures from outside become an answer, not a crash.
    public async Task<string?> TitleOfAsync(string isbn)
    {
        try
        {
            using HttpResponseMessage response =
                await http.GetAsync("https://catalog.example/isbn/" + isbn);
            return response.StatusCode == HttpStatusCode.OK
                ? await response.Content.ReadAsStringAsync()
                : null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }
}

// A stand-in for the network: it answers without leaving the process.
public sealed class StubHandler(
    Func<HttpRequestMessage, HttpResponseMessage> answer) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(answer(request));
}
