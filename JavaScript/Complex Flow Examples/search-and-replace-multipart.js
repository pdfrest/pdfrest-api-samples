// Search PDF first, optionally remove unwanted matches, then replace the retained matches.
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
    JSON.stringify([{ type: "literal", value: "Draft" }]),
  );
  searchForm.append("output_type", "json");
  const response = await axios({
    method: "post",
    maxBodyLength: Infinity,
    url: apiUrl + "/search-results",
    headers: { "Api-Key": apiKey, ...searchForm.getHeaders() },
    data: searchForm,
  });
  console.log(JSON.stringify(response.data, null, 2));
  const selections = response.data;
  // A program can filter selections.matches here before submitting the retained matches.
  if (selections.matches.length === 0) {
    console.log("No matches found; no PDF was modified.");
    return;
  }
  // Reuse the same unchanged PDF that was searched.
  // Each retained match can have its own replacement and optional layout_mode (default: reflow).
  for (const match of selections.matches) {
    match.replacement = "Final";
  }
  const actionForm = new FormData();
  actionForm.append("id", selections.inputId);
  actionForm.append("selections", JSON.stringify(selections));
  const actionResponse = await axios({
    method: "post",
    maxBodyLength: Infinity,
    url: apiUrl + "/pdf-with-replaced-text",
    headers: {
      "Api-Key": apiKey,
      ...actionForm.getHeaders(),
    },
    data: actionForm,
  });
  console.log(JSON.stringify(actionResponse.data, null, 2));
}

main().catch((error) => {
  console.error(error.response?.data || error.message);
  process.exitCode = 1;
});
