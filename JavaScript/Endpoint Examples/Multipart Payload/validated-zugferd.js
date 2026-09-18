// Validate a hybrid ZUGFeRD / Factur-X PDF without modifying it.
var axios = require('axios');
var FormData = require('form-data');
var fs = require('fs');

// By default, we use the US-based API service. This is the primary endpoint for global use.
var apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//var apiUrl = "https://eu-api.pdfrest.com";

// Set this path to the completed hybrid ZUGFeRD or Factur-X PDF you want to validate.
var zugferdPdf = "/path/to/zugferd-invoice.pdf";
var data = new FormData();
data.append('file', fs.createReadStream(zugferdPdf));

var config = {
  method: 'post',
  maxBodyLength: Infinity,
  url: apiUrl + '/validated-zugferd',
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
