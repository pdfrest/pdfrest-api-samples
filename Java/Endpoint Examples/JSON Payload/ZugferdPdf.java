import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import java.util.concurrent.TimeUnit;
import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import org.json.JSONObject;

public class ZugferdPdf {
  // By default, we use the US-based API service. This is the primary endpoint for global use.
  private static final String API_URL = "https://api.pdfrest.com";

  // For GDPR compliance and enhanced performance for European users, use the EU-based service
  // instead.
  // For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
  // private static final String API_URL = "https://eu-api.pdfrest.com";

  // Specify your API key here, or in the environment variable PDFREST_API_KEY.
  // You can also put the environment variable in a .env file.
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
  private static final OkHttpClient CLIENT =
      new OkHttpClient.Builder().readTimeout(60, TimeUnit.SECONDS).build();

  public static void main(String[] args) throws IOException {
    // Specify XML and PDF paths here, or as the first and second program arguments.
    File invoiceXml = new File(args.length > 0 ? args[0] : "/path/to/invoice.xml");
    File invoicePdf = new File(args.length > 1 ? args[1] : "/path/to/invoice.pdf");
    String apiKey =
        Dotenv.configure()
            .ignoreIfMalformed()
            .ignoreIfMissing()
            .load()
            .get("PDFREST_API_KEY", DEFAULT_API_KEY);
    String xmlId = upload(invoiceXml, apiKey, "application/xml");
    String pdfId = upload(invoicePdf, apiKey, "application/pdf");
    JSONObject options =
        new JSONObject()
            .put("locale", "de-DE")
            .put("label_language", "de")
            .put("font", "arial")
            .put("bold_font", "arialbold")
            .put("accent_color_rgb", new int[] {0, 92, 171});
    // Upload invoice XML and PDF, then create a ZUGFeRD / Factur-X PDF/A-3 invoice by resource ID.
    // pdfRest preserves the supplied PDF when it agrees with the canonical XML.
    // Fallback generation handles a mismatch or an unconfirmed PDF/XML match.
    // The render options style only that replacement PDF, not a preserved supplied PDF.
    JSONObject payload =
        new JSONObject()
            .put("id", xmlId)
            .put("pdf_id", pdfId)
            .put("regenerate_pdf", true)
            .put("output", "zugferd_invoice")
            .put("render_options", options);
    send(
        new Request.Builder()
            .url(API_URL + "/zugferd-pdf")
            .header("Api-Key", apiKey)
            .post(RequestBody.create(payload.toString(), MediaType.parse("application/json")))
            .build());
  }

  private static String upload(File file, String apiKey, String contentType) throws IOException {
    Request request =
        new Request.Builder()
            .url(API_URL + "/upload")
            .header("Api-Key", apiKey)
            .header("Content-Filename", file.getName())
            .post(RequestBody.create(file, MediaType.parse(contentType)))
            .build();
    try (Response response = CLIENT.newCall(request).execute()) {
      String body = response.body() == null ? "{}" : response.body().string();
      System.out.println("Upload Result code " + response.code());
      if (!response.isSuccessful()) {
        throw new IOException(body);
      }
      return new JSONObject(body).getJSONArray("files").getJSONObject(0).getString("id");
    }
  }

  private static void send(Request request) throws IOException {
    try (Response response = CLIENT.newCall(request).execute()) {
      String body = response.body() == null ? "" : response.body().string();
      System.out.println("Result code " + response.code());
      System.out.println(body);
      if (!response.isSuccessful()) {
        System.exit(1);
      }
    }
  }
}
