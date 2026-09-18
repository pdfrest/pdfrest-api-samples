#!/bin/sh

# Create a ZUGFeRD / Factur-X PDF/A-3 invoice from XML and an existing invoice PDF.
# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

# Replace the API key and invoice paths below with your own values.
API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key
INVOICE_XML="/path/to/invoice.xml"
INVOICE_PDF="/path/to/invoice.pdf"

# pdfRest preserves the supplied PDF when it agrees with the canonical XML.
# `regenerate_pdf` enables a fallback replacement PDF for a mismatch or unconfirmed match.
# `render_options` style that fallback PDF only; they do not alter a preserved supplied PDF.
curl --location "$API_URL/zugferd-pdf" \
  --header "Accept: application/json" \
  --header "Api-Key: $API_KEY" \
  --form "file=@$INVOICE_XML;type=application/xml" \
  --form "pdf_file=@$INVOICE_PDF;type=application/pdf" \
  --form "regenerate_pdf=true" \
  --form 'render_options={"locale":"de-DE","label_language":"de","font":"arial","bold_font":"arialbold","accent_color_rgb":[0,92,171]}' \
  --form "output=zugferd_invoice"
