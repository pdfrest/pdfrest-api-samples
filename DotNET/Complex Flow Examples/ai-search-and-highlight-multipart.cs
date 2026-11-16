/*
 * What this sample does:
 * - Use AI to find confidentiality obligations by meaning, then highlight them. No exact wording or regex is supplied.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - AI Search PDF is available on the US Cloud service only.
 * - Leave PDFREST_URL unset or use https://api.pdfrest.com.
 * - AI Search PDF is unavailable in EU and self-hosted deployments.
 *
 * Usage:
 *   dotnet run -- ai-search-and-highlight-multipart /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; HTTP and validation failures exit non-zero.
 */

using Newtonsoft.Json.Linq;
using Samples;

namespace Samples.ComplexFlowExamples;

public static class AiSearchAndHighlightMultipart
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "ai-search-and-highlight-multipart");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        using var searchRequest = new HttpRequestMessage(HttpMethod.Post, "ai-search-results");
        searchRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        searchRequest.Headers.Accept.Add(new("application/json"));
        var searchForm = new MultipartFormDataContent();
        var fileContent = new StreamContent(File.OpenRead(inputPath));
        fileContent.Headers.ContentType = new("application/pdf");
        searchForm.Add(fileContent, "file", Path.GetFileName(inputPath));
        searchForm.Add(new StringContent("[{\"type\":\"prompt\",\"value\":\"Find statements that require a party to keep information private or not disclose it to others, even when they do not use the word confidential.\"}]"), "searches");
        searchForm.Add(new StringContent("json"), "output_type");
        searchRequest.Content = searchForm;
        using var searchResponse = await client.SendAsync(searchRequest);
        var searchText = await searchResponse.Content.ReadAsStringAsync();
        Console.WriteLine(searchText);
        searchResponse.EnsureSuccessStatusCode();
        var selections = JObject.Parse(searchText);
        // AI Search recognizes content without known text or regex patterns.
        // Review the returned text and confidence before making changes.
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
