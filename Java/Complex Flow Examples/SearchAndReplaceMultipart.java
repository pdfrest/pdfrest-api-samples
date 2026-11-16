import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import java.util.concurrent.TimeUnit;
import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import org.json.JSONArray;
import org.json.JSONObject;

// Search PDF first, optionally remove unwanted matches, then replace the retained matches.
public class SearchAndReplaceMultipart {
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
    RequestBody searchBody =
        new MultipartBody.Builder()
            .setType(MultipartBody.FORM)
            .addFormDataPart(
                "file",
                inputFile.getName(),
                RequestBody.create(inputFile, MediaType.parse("application/pdf")))
            .addFormDataPart("searches", "[{\"type\":\"literal\",\"value\":\"Draft\"}]")
            .addFormDataPart("output_type", "json")
            .build();
    JSONObject searchResult = send("/search-results", searchBody, apiKey);
    // A program can filter searchResult.matches before submitting the retained matches.
    JSONArray matches = searchResult.getJSONArray("matches");
    if (matches.isEmpty()) {
      System.out.println("No matches found; no PDF was modified.");
      return;
    }
    // Reuse the same unchanged PDF that was searched.
    String sourceId = searchResult.getString("inputId");
    // Each match can have its own replacement and optional layout_mode (default: reflow).
    for (int i = 0; i < matches.length(); i++) {
      matches.getJSONObject(i).put("replacement", "Final");
    }
    RequestBody actionBody =
        new MultipartBody.Builder()
            .setType(MultipartBody.FORM)
            .addFormDataPart("id", sourceId)
            .addFormDataPart("selections", searchResult.toString())
            .build();
    JSONObject actionResult = send("/pdf-with-replaced-text", actionBody, apiKey);
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
