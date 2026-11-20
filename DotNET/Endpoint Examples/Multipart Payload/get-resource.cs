/*
 * What this sample does:
 * - Downloads a resource file by id.
 * - Routed from Program.cs as: `dotnet run -- get-resource-multipart <id> [out]`.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- get-resource-multipart <id> out.bin
 *
 * Output:
 * - Writes to the specified output path (default resource.bin). Non-2xx exits non-zero.
 */

namespace Samples.EndpointExamples.MultipartPayload
{
    public static class GetResource
    {
        public static async Task Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.Error.WriteLine("get-resource-multipart requires <id> [out]");
                Environment.Exit(1);
                return;
            }

            var id = args[0];
            var outPath = args.Length > 1 ? args[1] : "resource.bin";

            var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";
            var resourceBase = baseUrl.TrimEnd('/') + "/resource/";

            using (var httpClient = new HttpClient { BaseAddress = new Uri(resourceBase) })
            {
                try
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, id + "?format=file"))
                    {
                        using (var response = await httpClient.SendAsync(request))
                        {
                            response.EnsureSuccessStatusCode();
                            await using var stream = await response.Content.ReadAsStreamAsync();
                            await using var fs = new FileStream(outPath, FileMode.Create);
                            await stream.CopyToAsync(fs);
                        }
                    }
                    Console.WriteLine($"Saved to {outPath}");
                }
                catch (HttpRequestException e)
                {
                    Console.Error.WriteLine($"HTTP error: {e.Message}");
                    Environment.Exit(1);
                }
            }
        }
    }
}
