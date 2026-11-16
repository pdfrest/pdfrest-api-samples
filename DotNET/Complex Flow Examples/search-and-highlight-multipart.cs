/*
 * What this sample does:
 * - Search PDF first, optionally remove unwanted matches, then highlight the retained matches.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- search-and-highlight-multipart /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; HTTP and validation failures exit non-zero.
 */

using Newtonsoft.Json.Linq;
using Samples;

namespace Samples.ComplexFlowExamples;

public static class SearchAndHighlightMultipart
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "search-and-highlight-multipart");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        using var searchRequest = new HttpRequestMessage(HttpMethod.Post, "search-results");
        searchRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        searchRequest.Headers.Accept.Add(new("application/json"));
        var searchForm = new MultipartFormDataContent();
        var fileContent = new StreamContent(File.OpenRead(inputPath));
        fileContent.Headers.ContentType = new("application/pdf");
        searchForm.Add(fileContent, "file", Path.GetFileName(inputPath));
        searchForm.Add(new StringContent("[{\"type\":\"literal\",\"value\":\"Confidential\"}]"), "searches");
        searchForm.Add(new StringContent("json"), "output_type");
        searchRequest.Content = searchForm;
        using var searchResponse = await client.SendAsync(searchRequest);
        var searchText = await searchResponse.Content.ReadAsStringAsync();
        Console.WriteLine(searchText);
        searchResponse.EnsureSuccessStatusCode();
        var selections = JObject.Parse(searchText);
        // A program can filter selections["matches"] before submitting the retained matches.
        var matches = (JArray)selections["matches"]!;
        if (matches.Count == 0)
        {
            Console.WriteLine("No matches found; no PDF was modified.");
            return;
        }
        // Reuse the same unchanged PDF that was searched.
        var sourceId = selections["inputId"]!.Value<string>()!;
        using var actionRequest = new HttpRequestMessage(HttpMethod.Post, "pdf-with-highlighted-text");
        actionRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        actionRequest.Headers.Accept.Add(new("application/json"));
        var actionForm = new MultipartFormDataContent();
        actionForm.Add(new StringContent(sourceId), "id");
        actionForm.Add(new StringContent(selections.ToString(Newtonsoft.Json.Formatting.None)), "selections");
        actionRequest.Content = actionForm;
        await SampleInput.PrintResponse(client, actionRequest);
    }
}
