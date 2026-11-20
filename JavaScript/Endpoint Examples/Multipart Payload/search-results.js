// Find exact text, an invoice-number pattern, and email addresses without changing the PDF.
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
    JSON.stringify([
      { type: "literal", value: "Confidential" },
      { type: "regex", value: "INV-[0-9]{4}" },
      { type: "preset", value: "email" },
    ]),
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
}

main().catch((error) => {
  console.error(error.response?.data || error.message);
  process.exitCode = 1;
});
