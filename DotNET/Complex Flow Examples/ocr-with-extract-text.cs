/*
 * What this sample does:
 * - Performs OCR on a PDF, then extracts text using the result id.
 * - Routed from Program.cs as: `dotnet run -- ocr-with-extract-text <pdf>`.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- ocr-with-extract-text /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses for OCR and extracted-text calls.
 */

using Newtonsoft.Json.Linq;
using System.Text;

namespace Samples.ComplexFlowExamples
{
    public static class OcrWithExtractText
    {
        public static async Task Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.Error.WriteLine("ocr-with-extract-text requires <pdf>");
                Environment.Exit(1);
                return;
            }
            var pdfPath = args[0];
            if (!File.Exists(pdfPath))
            {
                Console.Error.WriteLine($"File not found: {pdfPath}");
                Environment.Exit(1);
                return;
            }
            var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.Error.WriteLine("Missing required environment variable: PDFREST_API_KEY");
                Environment.Exit(1);
                return;
            }
            var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";

            using (var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) })
            {
                using var ocrRequest = new HttpRequestMessage(HttpMethod.Post, "pdf-with-ocr-text");
                ocrRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
                ocrRequest.Headers.Accept.Add(new("application/json"));
                var ocrMultipartContent = new MultipartFormDataContent();
                var pdfByteArray = File.ReadAllBytes(pdfPath);
                var pdfByteArrayContent = new ByteArrayContent(pdfByteArray);
                ocrMultipartContent.Add(pdfByteArrayContent, "file", Path.GetFileName(pdfPath));
                pdfByteArrayContent.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");
                ocrRequest.Content = ocrMultipartContent;
                var ocrResponse = await httpClient.SendAsync(ocrRequest);
                var ocrResult = await ocrResponse.Content.ReadAsStringAsync();
                Console.WriteLine("OCR response received.");
                Console.WriteLine(ocrResult);
                var ocrJson = JObject.Parse(ocrResult);
                var ocrPDFID = (string?)(ocrJson["outputId"] ?? ocrJson["id"] ?? ocrJson["files"]?[0]?["id"]);
                if (string.IsNullOrWhiteSpace(ocrPDFID))
                {
                    Console.Error.WriteLine("Failed to obtain OCR output id. Full response:");
                    Console.Error.WriteLine(ocrResult);
                    Environment.Exit(1);
                    return;
                }

                using var extractTextRequest = new HttpRequestMessage(HttpMethod.Post, "extracted-text");
                extractTextRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
                extractTextRequest.Headers.Accept.Add(new("application/json"));
                var extractTextMultipartContent = new MultipartFormDataContent();
                var byteArrayOption = new ByteArrayContent(Encoding.UTF8.GetBytes(ocrPDFID));
                extractTextMultipartContent.Add(byteArrayOption, "id");
                extractTextRequest.Content = extractTextMultipartContent;
                var extractTextResponse = await httpClient.SendAsync(extractTextRequest);
                var extractTextResult = await extractTextResponse.Content.ReadAsStringAsync();
                Console.WriteLine("Extract text response received.");
                Console.WriteLine(extractTextResult);
            }
        }
    }
}
