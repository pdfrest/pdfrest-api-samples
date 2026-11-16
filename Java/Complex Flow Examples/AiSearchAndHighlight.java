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

// Use AI to find confidentiality obligations by meaning, then highlight them. No exact wording or
// regex is supplied.
public class AiSearchAndHighlight {
  // AI Search PDF is available on the US Cloud service only.
  private static final String API_URL = "https://api.pdfrest.com";

  // AI Search PDF is unavailable on the EU service and in self-hosted deployments.

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
    JSONObject searchPayload =
        new JSONObject()
            .put("id", inputId)
            .put(
                "searches",
                new JSONArray(
                    "[{\"type\":\"prompt\",\"value\":\"Find statements that require a party to keep information private or not disclose it to others, even when they do not use the word confidential.\"}]"))
            .put("output_type", "json");
    JSONObject searchResult =
        send(
            "/ai-search-results",
            RequestBody.create(searchPayload.toString(), MediaType.parse("application/json")),
            apiKey);
    // AI Search selects content by meaning; ordinary Search needs literal text, a regex, or a
    // pattern preset.
    // Review the returned text and confidence; remove unwanted matches before the action.
    // A program can filter searchResult.matches before submitting the retained matches.
    JSONArray matches = searchResult.getJSONArray("matches");
    if (matches.isEmpty()) {
      System.out.println("No matches found; no PDF was modified.");
      return;
    }
    // Reuse the same unchanged PDF that was searched.
    String sourceId = searchResult.getString("inputId");
    JSONObject actionPayload = new JSONObject().put("id", sourceId).put("selections", searchResult);
    JSONObject actionResult =
        send(
            "/pdf-with-highlighted-text",
            RequestBody.create(actionPayload.toString(), MediaType.parse("application/json")),
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
