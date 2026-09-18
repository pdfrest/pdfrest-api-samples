/*
 * What this sample does:
 * - Creates a ZUGFeRD / Factur-X PDF/A-3 invoice from XML and an existing PDF through multipart/form-data.
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
 *   dotnet run -- zugferd-pdf-multipart /path/to/invoice.xml /path/to/invoice.pdf
 *
 * Output:
 * - Prints the API JSON response and returns a nonzero exit code when the request fails.
 */
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Samples.EndpointExamples.MultipartPayload;

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
        using var form = new MultipartFormDataContent();
        var xml = new ByteArrayContent(await File.ReadAllBytesAsync(invoiceXml));
        xml.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        form.Add(xml, "file", Path.GetFileName(invoiceXml));
        var pdf = new ByteArrayContent(await File.ReadAllBytesAsync(invoicePdf));
        pdf.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(pdf, "pdf_file", Path.GetFileName(invoicePdf));
        // Render options style only a fallback-generated replacement, not a preserved PDF.
        form.Add(new StringContent("true"), "regenerate_pdf");
        form.Add(new StringContent(new JObject { ["locale"] = "de-DE", ["label_language"] = "de", ["font"] = "arial", ["bold_font"] = "arialbold", ["accent_color_rgb"] = new JArray(0, 92, 171) }.ToString()), "render_options");
        form.Add(new StringContent("zugferd_invoice"), "output");
        var response = await client.PostAsync("zugferd-pdf", form);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        if (!response.IsSuccessStatusCode) Environment.ExitCode = 1;
    }
}
