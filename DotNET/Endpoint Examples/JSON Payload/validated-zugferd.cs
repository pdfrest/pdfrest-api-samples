/*
 * What this sample does:
 * - Uploads a hybrid PDF, then validates its ZUGFeRD / Factur-X package using its resource ID.
 *
 * Setup (environment):
 * - Copy .env.example to .env and set PDFREST_API_KEY=your_api_key_here.
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- validated-zugferd /path/to/zugferd-invoice.pdf
 *
 * Output:
 * - Prints the validation JSON response and returns a nonzero exit code when an upload or validation request fails.
 */
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Samples.EndpointExamples.JsonPayload;

public static class ValidatedZugferd
{
    public static async Task Execute(string[] args)
    {
        var zugferdPdf = args.Length > 0 ? args[0] : "/path/to/zugferd-invoice.pdf";
        if (!File.Exists(zugferdPdf)) throw new FileNotFoundException("ZUGFeRD PDF not found.", zugferdPdf);
        var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY") ?? throw new InvalidOperationException("Missing PDFREST_API_KEY");
        var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";
        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.TryAddWithoutValidation("Api-Key", apiKey);
        using var uploadContent = new ByteArrayContent(await File.ReadAllBytesAsync(zugferdPdf));
        uploadContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        uploadContent.Headers.Add("Content-Filename", Path.GetFileName(zugferdPdf));
        var upload = await client.PostAsync("upload", uploadContent);
        var uploadBody = await upload.Content.ReadAsStringAsync();
        if (!upload.IsSuccessStatusCode) { Console.Error.WriteLine(uploadBody); Environment.ExitCode = 1; return; }
        var pdfId = JObject.Parse(uploadBody)["files"]![0]! ["id"]!.ToString();
        var response = await client.PostAsync("validated-zugferd", new StringContent(new JObject { ["id"] = pdfId }.ToString(), System.Text.Encoding.UTF8, "application/json"));
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        if (!response.IsSuccessStatusCode) Environment.ExitCode = 1;
    }
}
