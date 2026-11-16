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
 *   dotnet run -- ai-search-and-redact /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; HTTP and validation failures exit non-zero.
 */

using Newtonsoft.Json.Linq;
using Samples;

namespace Samples.ComplexFlowExamples;

public static class AiSearchAndRedact
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "ai-search-and-redact");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        // Upload once; subsequent JSON requests identify this PDF by resource ID.
        var inputId = await SampleInput.UploadPdf(client, inputPath, apiKey);
        using var searchRequest = SampleInput.JsonRequest("ai-search-results", new JObject
        {
            ["id"] = inputId,
            ["searches"] = JArray.Parse("""[{"type":"ai_preset","value":"potential_pii"}]"""),
            ["output_type"] = "json",
        }, apiKey);
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
        using var actionRequest = SampleInput.JsonRequest("pdf-with-redacted-text-preview", new JObject
        {
            ["id"] = sourceId,
            ["selections"] = selections,
        }, apiKey);
        using var previewResponse = await client.SendAsync(actionRequest);
        var previewText = await previewResponse.Content.ReadAsStringAsync();
        Console.WriteLine(previewText);
        previewResponse.EnsureSuccessStatusCode();
        var previewId = JObject.Parse(previewText)["outputId"]!.Value<string>()!;
        // Preview annotations do not remove content. Inspect the preview before applying.
        // This example immediately applies it to demonstrate the complete workflow.
        using var applyRequest = SampleInput.JsonRequest("pdf-with-redacted-text-applied", new JObject
        {
            ["id"] = previewId,
        }, apiKey);
        await SampleInput.PrintResponse(client, applyRequest);
    }
}
