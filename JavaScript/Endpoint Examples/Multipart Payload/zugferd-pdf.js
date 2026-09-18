// Create a ZUGFeRD / Factur-X PDF/A-3 invoice from XML and an existing invoice PDF.
var axios = require('axios');
var FormData = require('form-data');
var fs = require('fs');

// By default, we use the US-based API service. This is the primary endpoint for global use.
var apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//var apiUrl = "https://eu-api.pdfrest.com";

// Set these paths to your CII XML invoice and the corresponding visual PDF.
var invoiceXml = "/path/to/invoice.xml";
var invoicePdf = "/path/to/invoice.pdf";

var data = new FormData();
data.append('file', fs.createReadStream(invoiceXml));
data.append('pdf_file', fs.createReadStream(invoicePdf));
// pdfRest preserves the supplied PDF when it agrees with the canonical XML.
// Fallback: generate a replacement PDF when the supplied PDF is mismatched or cannot be fully confirmed.
data.append('regenerate_pdf', 'true');
// These styles apply only to that fallback-generated PDF; they do not alter a preserved PDF.
data.append('render_options', JSON.stringify({
  locale: "de-DE", label_language: "de", font: "arial", bold_font: "arialbold",
  accent_color_rgb: [0, 92, 171],
}));
data.append('output', 'zugferd_invoice');

var config = {
  method: 'post',
  maxBodyLength: Infinity,
  url: apiUrl + '/zugferd-pdf',
  headers: {
    'Api-Key': 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', // Replace with your API key
    ...data.getHeaders()
  },
  data: data
};

axios(config)
.then(function (response) {
  console.log(JSON.stringify(response.data));
})
.catch(function (error) {
  console.log(error);
});
