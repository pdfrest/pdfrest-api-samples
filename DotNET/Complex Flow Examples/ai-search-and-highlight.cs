/*
 * What this sample does:
 * - Use AI to find confidentiality obligations by meaning, then highlight them. No exact wording or regex is supplied.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- ai-search-and-highlight /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; HTTP and validation failures exit non-zero.
 */

using Newtonsoft.Json.Linq;
using Samples;

namespace Samples.ComplexFlowExamples;

public static class AiSearchAndHighlight
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "ai-search-and-highlight");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        // Upload once; subsequent JSON requests identify this PDF by resource ID.
        var inputId = await SampleInput.UploadPdf(client, inputPath, apiKey);
        using var searchRequest = SampleInput.JsonRequest("ai-search-results", new JObject
        {
            ["id"] = inputId,
            ["searches"] = JArray.Parse("""[{"type":"prompt","value":"Find statements that require a party to keep information private or not disclose it to others, even when they do not use the word confidential."}]"""),
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
        using var actionRequest = SampleInput.JsonRequest("pdf-with-highlighted-text", new JObject
        {
            ["id"] = sourceId,
            ["selections"] = selections,
        }, apiKey);
        await SampleInput.PrintResponse(client, actionRequest);
    }
}
