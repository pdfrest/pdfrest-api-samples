// Find people and medication names with AI Search PDF (Cloud only). Review results for accuracy and completeness.
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
      { type: "ai_preset", value: "person_name" },
      { type: "prompt", value: "Find medication names." },
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
}

main().catch((error) => {
  console.error(error.response?.data || error.message);
  process.exitCode = 1;
});
