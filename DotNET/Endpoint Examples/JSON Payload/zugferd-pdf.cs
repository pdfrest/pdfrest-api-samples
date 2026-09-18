/*
 * What this sample does:
 * - Uploads invoice XML and PDF, then creates a ZUGFeRD / Factur-X PDF/A-3 invoice using resource IDs.
 * - Preserves the supplied PDF when it agrees with the canonical XML.
 * - Regenerates a styled replacement only for a mismatch or unconfirmed PDF/XML match.
 *
 * Setup (environment):
 * - Copy .env.example to .env and set PDFREST_API_KEY=your_api_key_here.
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- zugferd-pdf /path/to/invoice.xml /path/to/invoice.pdf
 *
 * Output:
 * - Prints the API JSON response and returns a nonzero exit code when an upload or creation request fails.
 */
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Samples.EndpointExamples.JsonPayload;

public static class ZugferdPdf
{
    public static async Task Execute(string[] args)
    {
        var invoiceXml = args.Length > 0 ? args[0] : "/path/to/invoice.xml";
        var invoicePdf = args.Length > 1 ? args[1] : "/path/to/invoice.pdf";
        if (!File.Exists(invoiceXml)) throw new FileNotFoundException("Invoice XML not found.", invoiceXml);
        if (!File.Exists(invoicePdf)) throw new FileNotFoundException("Invoice PDF not found.", invoicePdf);
        var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY") ?? throw new InvalidOperationException("Missing PDFREST_API_KEY");
        var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";
        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.TryAddWithoutValidation("Api-Key", apiKey);
        using var uploadContent = new ByteArrayContent(await File.ReadAllBytesAsync(invoiceXml));
        uploadContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        uploadContent.Headers.Add("Content-Filename", Path.GetFileName(invoiceXml));
        var upload = await client.PostAsync("upload", uploadContent);
        var uploadBody = await upload.Content.ReadAsStringAsync();
        if (!upload.IsSuccessStatusCode) { Console.Error.WriteLine(uploadBody); Environment.ExitCode = 1; return; }
        var xmlId = JObject.Parse(uploadBody)["files"]![0]! ["id"]!.ToString();
        using var pdfContent = new ByteArrayContent(await File.ReadAllBytesAsync(invoicePdf));
        pdfContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        pdfContent.Headers.Add("Content-Filename", Path.GetFileName(invoicePdf));
        var pdfUpload = await client.PostAsync("upload", pdfContent);
        var pdfUploadBody = await pdfUpload.Content.ReadAsStringAsync();
        if (!pdfUpload.IsSuccessStatusCode) { Console.Error.WriteLine(pdfUploadBody); Environment.ExitCode = 1; return; }
        var pdfId = JObject.Parse(pdfUploadBody)["files"]![0]! ["id"]!.ToString();
        // Render options style only a fallback-generated replacement, not a preserved PDF.
        var payload = new JObject { ["id"] = xmlId, ["pdf_id"] = pdfId, ["regenerate_pdf"] = true, ["output"] = "zugferd_invoice", ["render_options"] = new JObject { ["locale"] = "de-DE", ["label_language"] = "de", ["font"] = "arial", ["bold_font"] = "arialbold", ["accent_color_rgb"] = new JArray(0, 92, 171) } };
        var response = await client.PostAsync("zugferd-pdf", new StringContent(payload.ToString(), System.Text.Encoding.UTF8, "application/json"));
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        if (!response.IsSuccessStatusCode) Environment.ExitCode = 1;
    }
}
