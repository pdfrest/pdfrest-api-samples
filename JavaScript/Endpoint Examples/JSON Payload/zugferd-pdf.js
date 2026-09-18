// Upload invoice XML and PDF, then create a ZUGFeRD / Factur-X PDF/A-3 invoice by resource ID.
var axios = require("axios");
var fs = require("fs");
var path = require("path");

// By default, we use the US-based API service. This is the primary endpoint for global use.
var apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//var apiUrl = "https://eu-api.pdfrest.com";

// Set these paths to your CII XML invoice and the corresponding visual PDF.
var invoiceXml = "/path/to/invoice.xml";
var invoicePdf = "/path/to/invoice.pdf";

var upload_config = {
  method: "post",
  maxBodyLength: Infinity,
  url: apiUrl + "/upload",
  headers: {
    "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", // Replace with your API key
    "Content-Filename": path.basename(invoiceXml),
    "Content-Type": "application/xml",
  },
  data: fs.createReadStream(invoiceXml),
};

axios(upload_config)
  .then(function (upload_response) {
    var xmlId = upload_response.data.files[0].id;
    var pdf_upload_config = {
      method: "post",
      maxBodyLength: Infinity,
      url: apiUrl + "/upload",
      headers: {
        "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", // Replace with your API key
        "Content-Filename": path.basename(invoicePdf),
        "Content-Type": "application/pdf",
      },
      data: fs.createReadStream(invoicePdf),
    };
    return axios(pdf_upload_config).then(function (pdf_upload_response) {
      return { xmlId: xmlId, pdfId: pdf_upload_response.data.files[0].id };
    });
  })
  .then(function (uploaded) {
    // pdfRest preserves the supplied PDF when it agrees with the canonical XML.
    // Fallback generation handles a mismatch or an unconfirmed PDF/XML match.
    // The render options style only that replacement PDF, not a preserved supplied PDF.
    var zugferd_config = {
      method: "post",
      maxBodyLength: Infinity,
      url: apiUrl + "/zugferd-pdf",
      headers: { "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", "Content-Type": "application/json" },
      data: {
        id: uploaded.xmlId,
        pdf_id: uploaded.pdfId,
        regenerate_pdf: true,
        output: "zugferd_invoice",
        render_options: { locale: "de-DE", label_language: "de", font: "arial", bold_font: "arialbold", accent_color_rgb: [0, 92, 171] },
      }
    };
    return axios(zugferd_config);
  })
  .then(function (response) {
    console.log(JSON.stringify(response.data));
  })
  .catch(function (error) {
    console.log(error);
  });
