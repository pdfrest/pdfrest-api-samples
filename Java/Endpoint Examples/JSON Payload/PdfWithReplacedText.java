import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import java.util.concurrent.TimeUnit;
import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import org.json.JSONArray;
import org.json.JSONObject;

// Find and replace all matches of each rule directly, without a separate search call.
public class PdfWithReplacedText {
  // By default, we use the US-based API service. This is the primary endpoint for global use.
  private static final String API_URL = "https://api.pdfrest.com";

  // For GDPR compliance and enhanced performance for European users, use the EU-based service
  // instead.
  // For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
  // private static final String API_URL = "https://eu-api.pdfrest.com";

  // Specify your API key here, or in PDFREST_API_KEY in the environment or a .env file.
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
  private static final OkHttpClient CLIENT =
      new OkHttpClient.Builder().readTimeout(60, TimeUnit.SECONDS).build();

  public static void main(String[] args) throws IOException {
    File inputFile = new File(args.length > 0 ? args[0] : "/path/to/input.pdf");
    String apiKey =
        Dotenv.configure()
            .ignoreIfMalformed()
            .ignoreIfMissing()
            .load()
            .get("PDFREST_API_KEY", DEFAULT_API_KEY);
    // Upload once; subsequent JSON requests identify this PDF by resource ID.
    JSONObject uploaded =
        send(
            "/upload",
            RequestBody.create(inputFile, MediaType.parse("application/octet-stream")),
            apiKey,
            inputFile.getName());
    String inputId = uploaded.getJSONArray("files").getJSONObject(0).getString("id");
    // Each rule uses one exact replacement for every match; omitted layout_mode uses reflow.
    JSONObject searchPayload =
        new JSONObject()
            .put("id", inputId)
            .put(
                "replacements",
                new JSONArray(
                    "[{\"search\":{\"type\":\"literal\",\"value\":\"Draft\"},\"replacement\":\"Final\"},{\"search\":{\"type\":\"literal\",\"value\":\"Pending\"},\"replacement\":\"Ready\"}]"));
    JSONObject searchResult =
        send(
            "/pdf-with-replaced-text",
            RequestBody.create(searchPayload.toString(), MediaType.parse("application/json")),
            apiKey);
  }

  private static JSONObject send(String path, RequestBody body, String apiKey) throws IOException {
    return send(path, body, apiKey, null);
  }

  private static JSONObject send(String path, RequestBody body, String apiKey, String filename)
      throws IOException {
    Request.Builder builder =
        new Request.Builder()
            .url(API_URL + path)
            .header("Api-Key", apiKey)
            .header("Accept", "application/json")
            .post(body);
    if (filename != null) {
      builder.header("Content-Filename", filename);
    }
    try (Response response = CLIENT.newCall(builder.build()).execute()) {
      String text = response.body() == null ? "{}" : response.body().string();
      System.out.println("Response status code: " + response.code());
      System.out.println(text);
      if (!response.isSuccessful()) {
        throw new IOException("Request failed: " + response.code());
      }
      return new JSONObject(text);
    }
  }
}
