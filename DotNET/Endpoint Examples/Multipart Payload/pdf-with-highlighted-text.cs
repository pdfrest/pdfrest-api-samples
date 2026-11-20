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
 *   dotnet run -- pdf-with-highlighted-text-multipart /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; HTTP and validation failures exit non-zero.
 */

using Samples;

namespace Samples.EndpointExamples.MultipartPayload;

public static class PdfWithHighlightedText
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "pdf-with-highlighted-text-multipart");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "pdf-with-highlighted-text");
        request.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        request.Headers.Accept.Add(new("application/json"));
        var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(File.OpenRead(inputPath));
        fileContent.Headers.ContentType = new("application/pdf");
        form.Add(fileContent, "file", Path.GetFileName(inputPath));
        form.Add(new StringContent("[{\"type\":\"literal\",\"value\":\"Confidential\"}]"), "searches");
        form.Add(new StringContent("255,255,0"), "rgb_color");
        form.Add(new StringContent("0.4"), "opacity");
        request.Content = form;
        await SampleInput.PrintResponse(client, request);
    }
}
