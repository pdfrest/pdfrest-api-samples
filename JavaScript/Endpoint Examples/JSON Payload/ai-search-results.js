// Find people and medication names with AI Search PDF (Cloud only). Review results for accuracy and completeness.
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
      searches: [
        { type: "ai_preset", value: "person_name" },
        { type: "prompt", value: "Find medication names." },
      ],
      output_type: "json",
    },
  });
  console.log(JSON.stringify(response.data, null, 2));
}

main().catch((error) => {
  console.error(error.response?.data || error.message);
  process.exitCode = 1;
});
