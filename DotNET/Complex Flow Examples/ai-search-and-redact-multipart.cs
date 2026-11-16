/*
 * What this sample does:
 * - Use AI to find potential PII, including unknown names and addresses, then redact retained matches. Review AI findings before applying.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- ai-search-and-redact-multipart /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; HTTP and validation failures exit non-zero.
 */

using Newtonsoft.Json.Linq;
using Samples;

namespace Samples.ComplexFlowExamples;

public static class AiSearchAndRedactMultipart
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "ai-search-and-redact-multipart");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        using var searchRequest = new HttpRequestMessage(HttpMethod.Post, "ai-search-results");
        searchRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        searchRequest.Headers.Accept.Add(new("application/json"));
        var searchForm = new MultipartFormDataContent();
        var fileContent = new StreamContent(File.OpenRead(inputPath));
        fileContent.Headers.ContentType = new("application/pdf");
        searchForm.Add(fileContent, "file", Path.GetFileName(inputPath));
        searchForm.Add(new StringContent("[{\"type\":\"ai_preset\",\"value\":\"potential_pii\"}]"), "searches");
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
        using var actionRequest = new HttpRequestMessage(HttpMethod.Post, "pdf-with-redacted-text-preview");
        actionRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        actionRequest.Headers.Accept.Add(new("application/json"));
        var actionForm = new MultipartFormDataContent();
        actionForm.Add(new StringContent(sourceId), "id");
        actionForm.Add(new StringContent(selections.ToString(Newtonsoft.Json.Formatting.None)), "selections");
        actionRequest.Content = actionForm;
        using var previewResponse = await client.SendAsync(actionRequest);
        var previewText = await previewResponse.Content.ReadAsStringAsync();
        Console.WriteLine(previewText);
        previewResponse.EnsureSuccessStatusCode();
        var previewId = JObject.Parse(previewText)["outputId"]!.Value<string>()!;
        // Preview annotations do not remove content. Inspect the preview before applying.
        // This example immediately applies it to demonstrate the complete workflow.
        using var applyRequest = new HttpRequestMessage(HttpMethod.Post, "pdf-with-redacted-text-applied");
        applyRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        applyRequest.Headers.Accept.Add(new("application/json"));
        var applyForm = new MultipartFormDataContent();
        applyForm.Add(new StringContent(previewId), "id");

        applyRequest.Content = applyForm;
        await SampleInput.PrintResponse(client, applyRequest);
    }
}
