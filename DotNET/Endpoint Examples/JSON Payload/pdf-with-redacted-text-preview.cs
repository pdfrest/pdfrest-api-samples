
/*
 * What this sample does:
 * - Called from Program.cs to preview text redactions on a PDF via JSON flow.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- pdf-with-redacted-text-preview /path/to/input.pdf
 *
 * Output:
 * - Prints JSON responses; non-2xx results exit non-zero.
 */
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Samples.EndpointExamples.JsonPayload
{
    public static class PdfWithRedactedTextPreview
    {
        public static async Task Execute(string[] args)
        {
            if (args == null || args.Length < 1) { Console.Error.WriteLine("pdf-with-redacted-text-preview requires <inputFile>"); Environment.Exit(1); return; }
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
                using (var redactedTextRequest = new HttpRequestMessage(HttpMethod.Post, "pdf-with-redacted-text-preview"))
                {
                    redactedTextRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
                    redactedTextRequest.Headers.Accept.Add(new("application/json"));
                    redactedTextRequest.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                    var redaction_option_array = new JArray
                    {
                        new JObject { ["type"] = "regex", ["value"] = "(?:\\(\\d{3}\\)\\s?|\\d{3}[-.\\s]?)?\\d{3}[-.\\s]?\\d{4}" },
                        new JObject { ["type"] = "literal", ["value"] = "word" }
                    };
                    JObject parameterJson = new JObject { ["id"] = uploadedID, ["redactions"] = JsonConvert.SerializeObject(redaction_option_array) };
                    redactedTextRequest.Content = new StringContent(parameterJson.ToString(), Encoding.UTF8, "application/json");
                    var redactedTextResponse = await httpClient.SendAsync(redactedTextRequest);
                    var redactedTextResult = await redactedTextResponse.Content.ReadAsStringAsync();
                    Console.WriteLine("Processing response received.");
                    Console.WriteLine(redactedTextResult);

                    // All files uploaded or generated are automatically deleted based on the 
                    // File Retention Period as shown on https://pdfrest.com/pricing. 
                    // For immediate deletion of files, particularly when sensitive data 
                    // is involved, an explicit delete call can be made to the API.
                    //
                    // Deletes all files in the workflow, including outputs. Save all desired files before enabling this step.
                    // IMPORTANT: Do not delete the previewId (the preview PDF) file until after the redaction is applied
                    // with the /pdf-with-redacted-text-applied endpoint.

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

                            var previewId = JObject.Parse(redactedTextResult)["outputId"].ToString();
                            JObject deleteJson = new JObject { ["ids"] = uploadedID + ", " + previewId };
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
