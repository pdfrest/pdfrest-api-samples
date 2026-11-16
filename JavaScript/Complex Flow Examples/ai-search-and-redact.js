// Use AI to find potential PII, including unknown names and addresses, then redact retained matches. Review AI findings before applying.
const axios = require("axios");
const fs = require("fs");

// AI Search PDF is available on the US Cloud service only.
const apiUrl = "https://api.pdfrest.com";

// AI Search PDF is unavailable on the EU service and in self-hosted deployments.

const apiKey = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"; // Replace with your API key.

async function main() {
  // Upload once; subsequent JSON requests identify this PDF by resource ID.
  const uploadResponse = await axios({
    method: "post",
    maxBodyLength: Infinity,
    url: apiUrl + "/upload",
    headers: {
      "Api-Key": apiKey,
      "Content-Filename": "input.pdf",
      "Content-Type": "application/octet-stream",
    },
    data: fs.createReadStream("/path/to/input.pdf"),
  });
  const inputId = uploadResponse.data.files[0].id;
  const response = await axios({
    method: "post",
    maxBodyLength: Infinity,
    url: apiUrl + "/ai-search-results",
    headers: { "Api-Key": apiKey, "Content-Type": "application/json" },
    data: {
      id: inputId,
      searches: [{ type: "ai_preset", value: "potential_pii" }],
      output_type: "json",
    },
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
  const actionResponse = await axios({
    method: "post",
    maxBodyLength: Infinity,
    url: apiUrl + "/pdf-with-redacted-text-preview",
    headers: { "Api-Key": apiKey, "Content-Type": "application/json" },
    data: { id: selections.inputId, selections: selections },
  });
  console.log(JSON.stringify(actionResponse.data, null, 2));
  // Preview annotations do not remove content. Inspect the preview before applying.
  // This example immediately applies it to demonstrate the complete workflow.
  const applyResponse = await axios({
    method: "post",
    maxBodyLength: Infinity,
    url: apiUrl + "/pdf-with-redacted-text-applied",
    headers: { "Api-Key": apiKey, "Content-Type": "application/json" },
    data: { id: actionResponse.data.outputId },
  });
  console.log(JSON.stringify(applyResponse.data, null, 2));
}

main().catch((error) => {
  console.error(error.response?.data || error.message);
  process.exitCode = 1;
});
