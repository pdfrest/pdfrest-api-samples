/*
 * What this sample does:
 * - Find people and medication names with AI Search PDF (Cloud only). Review results for accuracy and completeness.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - AI Search PDF is available on the US Cloud service only.
 * - Leave PDFREST_URL unset or use https://api.pdfrest.com.
 * - AI Search PDF is unavailable in EU and self-hosted deployments.
 *
 * Usage:
 *   dotnet run -- ai-search-results /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; HTTP and validation failures exit non-zero.
 */

using Newtonsoft.Json.Linq;
using Samples;

namespace Samples.EndpointExamples.JsonPayload;

public static class AiSearchResults
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "ai-search-results");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        // Upload once; subsequent JSON requests identify this PDF by resource ID.
        var inputId = await SampleInput.UploadPdf(client, inputPath, apiKey);
        using var request = SampleInput.JsonRequest("ai-search-results", new JObject
        {
            ["id"] = inputId,
            ["searches"] = JArray.Parse("""[{"type":"ai_preset","value":"person_name"},{"type":"prompt","value":"Find medication names."}]"""),
            ["output_type"] = "json",
        }, apiKey);
        await SampleInput.PrintResponse(client, request);
    }
}
