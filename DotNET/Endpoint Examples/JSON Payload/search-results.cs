/*
 * What this sample does:
 * - Find exact text, an invoice-number pattern, and email addresses without changing the PDF.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- search-results /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; HTTP and validation failures exit non-zero.
 */

using Newtonsoft.Json.Linq;
using Samples;

namespace Samples.EndpointExamples.JsonPayload;

public static class SearchResults
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "search-results");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        // Upload once; subsequent JSON requests identify this PDF by resource ID.
        var inputId = await SampleInput.UploadPdf(client, inputPath, apiKey);
        using var request = SampleInput.JsonRequest("search-results", new JObject
        {
            ["id"] = inputId,
            ["searches"] = JArray.Parse("""[{"type":"literal","value":"Confidential"},{"type":"regex","value":"INV-[0-9]{4}"},{"type":"preset","value":"email"}]"""),
            ["output_type"] = "json",
        }, apiKey);
        await SampleInput.PrintResponse(client, request);
    }
}
