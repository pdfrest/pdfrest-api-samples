// Use AI to find confidentiality obligations by meaning, then highlight them. No exact wording or regex is supplied.
const axios = require("axios");
const FormData = require("form-data");
const fs = require("fs");

// AI Search PDF is available on the US Cloud service only.
const apiUrl = "https://api.pdfrest.com";

// AI Search PDF is unavailable on the EU service and in self-hosted deployments.

const apiKey = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"; // Replace with your API key.

async function main() {
  const searchForm = new FormData();
  searchForm.append("file", fs.createReadStream("/path/to/input.pdf"));
  searchForm.append(
    "searches",
    JSON.stringify([
      {
        type: "prompt",
        value:
          "Find statements that require a party to keep information private or not disclose it to others, even when they do not use the word confidential.",
      },
    ]),
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
    url: apiUrl + "/pdf-with-highlighted-text",
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
