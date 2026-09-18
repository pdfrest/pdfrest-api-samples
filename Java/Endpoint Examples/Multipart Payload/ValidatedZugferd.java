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

public class ValidatedZugferd {
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
    // Specify the completed hybrid ZUGFeRD or Factur-X PDF path here, or as the first program
    // argument.
    File zugferdPdf = new File(args.length > 0 ? args[0] : "/path/to/zugferd-invoice.pdf");
    String apiKey =
        Dotenv.configure()
            .ignoreIfMalformed()
            .ignoreIfMissing()
            .load()
            .get("PDFREST_API_KEY", DEFAULT_API_KEY);
    // Validate a hybrid ZUGFeRD / Factur-X PDF without modifying it.
    MultipartBody body =
        new MultipartBody.Builder()
            .setType(MultipartBody.FORM)
            .addFormDataPart(
                "file",
                zugferdPdf.getName(),
                RequestBody.create(zugferdPdf, MediaType.parse("application/pdf")))
            .build();
    send(
        new Request.Builder()
            .url(API_URL + "/validated-zugferd")
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
