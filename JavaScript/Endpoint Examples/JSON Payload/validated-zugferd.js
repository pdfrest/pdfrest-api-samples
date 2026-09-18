// Upload a hybrid PDF, then validate its ZUGFeRD / Factur-X package by resource ID.
var axios = require("axios");
var fs = require("fs");
var path = require("path");

// By default, we use the US-based API service. This is the primary endpoint for global use.
var apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//var apiUrl = "https://eu-api.pdfrest.com";

// Set this path to the completed hybrid ZUGFeRD or Factur-X PDF you want to validate.
var zugferdPdf = "/path/to/zugferd-invoice.pdf";
var upload_config = {
  method: "post",
  maxBodyLength: Infinity,
  url: apiUrl + "/upload",
  headers: {
    "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", // Replace with your API key
    "Content-Filename": path.basename(zugferdPdf),
    "Content-Type": "application/pdf",
  },
  data: fs.createReadStream(zugferdPdf),
};

axios(upload_config)
  .then(function (upload_response) {
    var validation_config = {
      method: "post",
      maxBodyLength: Infinity,
      url: apiUrl + "/validated-zugferd",
      headers: { "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", "Content-Type": "application/json" },
      data: { id: upload_response.data.files[0].id },
    };
    return axios(validation_config);
  })
  .then(function (response) {
    console.log(JSON.stringify(response.data));
  })
  .catch(function (error) {
    console.log(error);
  });
