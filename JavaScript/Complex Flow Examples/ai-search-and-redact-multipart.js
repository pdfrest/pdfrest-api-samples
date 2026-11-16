// Use AI to find potential PII, including unknown names and addresses, then redact retained matches. Review AI findings before applying.
const axios = require("axios");
const FormData = require("form-data");
const fs = require("fs");

// By default, we use the US-based API service. This is the primary endpoint for global use.
const apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
// const apiUrl = "https://eu-api.pdfrest.com";

const apiKey = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"; // Replace with your API key.

async function main() {
  const searchForm = new FormData();
  searchForm.append("file", fs.createReadStream("/path/to/input.pdf"));
  searchForm.append(
    "searches",
    JSON.stringify([{ type: "ai_preset", value: "potential_pii" }]),
  );
  searchForm.append("output_type", "json");
  const response = await axios({
    method: "post",
    maxBodyLength: Infinity,
    url: apiUrl + "/ai-search-results",
    headers: { "Api-Key": apiKey, ...searchForm.getHeaders() },
    data: searchForm,
  });
  console.log(JSON.stringify(response.data, null, 2));
  const selections = response.data;
  // AI Search selects content by meaning; ordinary Search needs literal text, a regex, or a pattern preset.
  // Review the returned text and confidence; remove unwanted matches before the action.
  // A program can filter selections.matches here before submitting the retained matches.
  if (selections.matches.length === 0) {
    console.log("No matches found; no PDF was modified.");
    return;
  }
  // Reuse the same unchanged PDF that was searched.
  const actionForm = new FormData();
  actionForm.append("id", selections.inputId);
  actionForm.append("selections", JSON.stringify(selections));
  const actionResponse = await axios({
    method: "post",
    maxBodyLength: Infinity,
    url: apiUrl + "/pdf-with-redacted-text-preview",
    headers: {
      "Api-Key": apiKey,
      ...actionForm.getHeaders(),
    },
    data: actionForm,
  });
  console.log(JSON.stringify(actionResponse.data, null, 2));
  // Preview annotations do not remove content. Inspect the preview before applying.
  // This example immediately applies it to demonstrate the complete workflow.
  const applyForm = new FormData();
  applyForm.append("id", actionResponse.data.outputId);

  const applyResponse = await axios({
    method: "post",
    maxBodyLength: Infinity,
    url: apiUrl + "/pdf-with-redacted-text-applied",
    headers: {
      "Api-Key": apiKey,
      ...applyForm.getHeaders(),
    },
    data: applyForm,
  });
  console.log(JSON.stringify(applyResponse.data, null, 2));
}

main().catch((error) => {
  console.error(error.response?.data || error.message);
  process.exitCode = 1;
});
