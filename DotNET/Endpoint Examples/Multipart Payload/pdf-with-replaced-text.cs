/*
 * What this sample does:
 * - Find and replace all matches of each rule directly, without a separate search call.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- pdf-with-replaced-text-multipart /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; HTTP and validation failures exit non-zero.
 */

using Samples;

namespace Samples.EndpointExamples.MultipartPayload;

public static class PdfWithReplacedText
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "pdf-with-replaced-text-multipart");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        // Each rule uses one exact replacement for every match; omitted layout_mode uses reflow.
        using var request = new HttpRequestMessage(HttpMethod.Post, "pdf-with-replaced-text");
        request.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        request.Headers.Accept.Add(new("application/json"));
        var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(File.OpenRead(inputPath));
        fileContent.Headers.ContentType = new("application/pdf");
        form.Add(fileContent, "file", Path.GetFileName(inputPath));
        form.Add(new StringContent("[{\"search\":{\"type\":\"literal\",\"value\":\"Draft\"},\"replacement\":\"Final\"},{\"search\":{\"type\":\"literal\",\"value\":\"Pending\"},\"replacement\":\"Ready\"}]"), "replacements");
        request.Content = form;
        await SampleInput.PrintResponse(client, request);
    }
}
