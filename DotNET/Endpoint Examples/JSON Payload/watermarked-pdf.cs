
/*
 * What this sample does:
 * - Called from Program.cs to add a text watermark to a PDF via JSON flow.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- watermarked-pdf /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; non-2xx results exit non-zero.
 */
using Newtonsoft.Json.Linq;
using System.Text;

namespace Samples.EndpointExamples.JsonPayload
{
    public static class WatermarkedPdf
    {
        public static async Task Execute(string[] args)
        {
            if (args == null || args.Length < 1) { Console.Error.WriteLine("watermarked-pdf requires <inputFile>"); Environment.Exit(1); return; }
            var inputPath = args[0]; if (!File.Exists(inputPath)) { Console.Error.WriteLine($"File not found: {inputPath}"); Environment.Exit(1); return; }
            var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY"); if (string.IsNullOrWhiteSpace(apiKey)) { Console.Error.WriteLine("Missing required environment variable: PDFREST_API_KEY"); Environment.Exit(1); return; }
            var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";
            using (var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) })
            using (var uploadRequest = new HttpRequestMessage(HttpMethod.Post, "upload"))
            {
                uploadRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
                uploadRequest.Headers.Accept.Add(new("application/json"));
                var uploadByteArray = File.ReadAllBytes(inputPath);
                var uploadByteAryContent = new ByteArrayContent(uploadByteArray);
                uploadByteAryContent.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");
                uploadByteAryContent.Headers.TryAddWithoutValidation("Content-Filename", Path.GetFileName(inputPath));
                uploadRequest.Content = uploadByteAryContent;
                var uploadResponse = await httpClient.SendAsync(uploadRequest);
                var uploadResult = await uploadResponse.Content.ReadAsStringAsync();
                Console.WriteLine("Upload response received.");
                Console.WriteLine(uploadResult);
                JObject uploadResultJson = JObject.Parse(uploadResult);
                var uploadedID = uploadResultJson["files"][0]["id"];
                using (var watermarkRequest = new HttpRequestMessage(HttpMethod.Post, "watermarked-pdf"))
                {
                    watermarkRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
                    watermarkRequest.Headers.Accept.Add(new("application/json"));
                    watermarkRequest.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                    JObject parameterJson = new JObject { ["id"] = uploadedID, ["watermark_text"] = "Watermarked" };
                    watermarkRequest.Content = new StringContent(parameterJson.ToString(), Encoding.UTF8, "application/json");
                    var watermarkResponse = await httpClient.SendAsync(watermarkRequest);
                    var watermarkResult = await watermarkResponse.Content.ReadAsStringAsync();
                    Console.WriteLine("Processing response received.");
                    Console.WriteLine(watermarkResult);
                    JObject watermarkResultJson = JObject.Parse(watermarkResult);
                    var outputID = watermarkResultJson["outputId"];

                    // All files uploaded or generated are automatically deleted based on the 
                    // File Retention Period as shown on https://pdfrest.com/pricing. 
                    // For immediate deletion of files, particularly when sensitive data 
                    // is involved, an explicit delete call can be made to the API.
                    //
                    // Deletes all files in the workflow, including outputs. Save all desired files before enabling this step.

                    // Optional immediate deletion of sensitive files
                    // Default: false; override with PDFREST_DELETE_SENSITIVE_FILES=true
                    var deleteSensitiveFiles = string.Equals(
                        Environment.GetEnvironmentVariable("PDFREST_DELETE_SENSITIVE_FILES"),
                        "true",
                        StringComparison.OrdinalIgnoreCase);

                    if (deleteSensitiveFiles)
                    {
                        using (var deleteRequest = new HttpRequestMessage(HttpMethod.Post, "delete"))
                        {
                            deleteRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
                            deleteRequest.Headers.Accept.Add(new("application/json"));
                            deleteRequest.Headers.TryAddWithoutValidation("Content-Type", "application/json");

                            JObject deleteJson = new JObject
                            {
                                ["ids"] = $"{uploadedID}, {outputID}"
                            };
                            deleteRequest.Content = new StringContent(deleteJson.ToString(), Encoding.UTF8, "application/json");
                            var deleteResponse = await httpClient.SendAsync(deleteRequest);
                            var deleteResult = await deleteResponse.Content.ReadAsStringAsync();
                            Console.WriteLine(deleteResult);
                        }
                    }
                }
            }
        }
    }
}
