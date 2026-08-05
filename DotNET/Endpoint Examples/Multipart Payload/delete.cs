/*
 * What this sample does:
 * - Deletes multiple resources by ids.
 * - Routed from Program.cs as: `dotnet run -- delete-multipart <id1> [id2] [...]`.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- delete-multipart id1 id2 id3
 *
 * Output:
 * - Prints the JSON response. Validation errors (args/env) exit non-zero.
 */

namespace Samples.EndpointExamples.MultipartPayload
{
    public static class Delete
    {
        public static async Task Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.Error.WriteLine("delete-multipart requires <id1> [id2] [...]");
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

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, baseUrl.TrimEnd('/') + "/delete");
            request.Headers.TryAddWithoutValidation("Api-Key", apiKey);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(string.Join(',', args)), "ids");
            request.Content = content;
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(body);
        }
    }
}
