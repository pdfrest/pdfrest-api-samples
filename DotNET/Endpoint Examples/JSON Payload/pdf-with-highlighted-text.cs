/*
 * What this sample does:
 * - Highlight matching text directly, without a separate search call.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- pdf-with-highlighted-text /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; HTTP and validation failures exit non-zero.
 */

using Newtonsoft.Json.Linq;
using Samples;

namespace Samples.EndpointExamples.JsonPayload;

public static class PdfWithHighlightedText
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "pdf-with-highlighted-text");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        // Upload once; subsequent JSON requests identify this PDF by resource ID.
        var inputId = await SampleInput.UploadPdf(client, inputPath, apiKey);
        using var request = SampleInput.JsonRequest("pdf-with-highlighted-text", new JObject
        {
            ["id"] = inputId,
            ["searches"] = JArray.Parse("""[{"type":"literal","value":"Confidential"}]"""),
            ["rgb_color"] = "255,255,0",
            ["opacity"] = 0.4,
        }, apiKey);
        await SampleInput.PrintResponse(client, request);
    }
}
