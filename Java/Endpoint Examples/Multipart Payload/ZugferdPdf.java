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
    JSONObject options =
        new JSONObject()
            .put("locale", "de-DE")
            .put("label_language", "de")
            .put("font", "arial")
            .put("bold_font", "arialbold")
            .put("accent_color_rgb", new int[] {0, 92, 171});
    // Create a ZUGFeRD / Factur-X PDF/A-3 invoice from XML and an existing invoice PDF.
    // pdfRest preserves the supplied PDF when it agrees with the canonical XML.
    // Fallback generation handles a mismatch or an unconfirmed PDF/XML match.
    // The render options style only that replacement PDF, not a preserved supplied PDF.
    MultipartBody body =
        new MultipartBody.Builder()
            .setType(MultipartBody.FORM)
            .addFormDataPart(
                "file",
                invoiceXml.getName(),
                RequestBody.create(invoiceXml, MediaType.parse("application/xml")))
            .addFormDataPart(
                "pdf_file",
                invoicePdf.getName(),
                RequestBody.create(invoicePdf, MediaType.parse("application/pdf")))
            .addFormDataPart("regenerate_pdf", "true")
            .addFormDataPart("render_options", options.toString())
            .addFormDataPart("output", "zugferd_invoice")
            .build();
    send(
        new Request.Builder()
            .url(API_URL + "/zugferd-pdf")
            .header("Api-Key", apiKey)
            .post(body)
            .build());
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
