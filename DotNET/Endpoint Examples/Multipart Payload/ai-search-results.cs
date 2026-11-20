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
 *   dotnet run -- ai-search-results-multipart /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; HTTP and validation failures exit non-zero.
 */

using Samples;

namespace Samples.EndpointExamples.MultipartPayload;

public static class AiSearchResults
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "ai-search-results-multipart");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "ai-search-results");
        request.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        request.Headers.Accept.Add(new("application/json"));
        var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(File.OpenRead(inputPath));
        fileContent.Headers.ContentType = new("application/pdf");
        form.Add(fileContent, "file", Path.GetFileName(inputPath));
        form.Add(new StringContent("[{\"type\":\"ai_preset\",\"value\":\"person_name\"},{\"type\":\"prompt\",\"value\":\"Find medication names.\"}]"), "searches");
        form.Add(new StringContent("json"), "output_type");
        request.Content = form;
        await SampleInput.PrintResponse(client, request);
    }
}
