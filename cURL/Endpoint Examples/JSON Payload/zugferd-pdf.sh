#!/bin/sh

# Upload invoice XML and PDF, then create a ZUGFeRD / Factur-X PDF/A-3 invoice by resource ID.
# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

# Replace the API key and invoice paths below with your own values.
API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key
INVOICE_XML="/path/to/invoice.xml"
INVOICE_PDF="/path/to/invoice.pdf"

XML_ID=$(curl --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: $API_KEY" \
  --header "Content-Filename: $(basename "$INVOICE_XML")" \
  --header "Content-Type: application/xml" \
  --data-binary "@$INVOICE_XML" | jq -r '.files[0].id')

test -n "$XML_ID" && test "$XML_ID" != "null" || { echo "XML upload failed" >&2; exit 1; }

PDF_ID=$(curl --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: $API_KEY" \
  --header "Content-Filename: $(basename "$INVOICE_PDF")" \
  --header "Content-Type: application/pdf" \
  --data-binary "@$INVOICE_PDF" | jq -r '.files[0].id')

test -n "$PDF_ID" && test "$PDF_ID" != "null" || { echo "PDF upload failed" >&2; exit 1; }

# pdfRest preserves the supplied PDF when it agrees with the canonical XML.
# `regenerate_pdf` enables a fallback replacement PDF for a mismatch or unconfirmed match.
# `render_options` style that fallback PDF only; they do not alter a preserved supplied PDF.
curl --location "$API_URL/zugferd-pdf" \
  --header "Accept: application/json" \
  --header "Api-Key: $API_KEY" \
  --header "Content-Type: application/json" \
  --data "{\"id\":\"$XML_ID\",\"pdf_id\":\"$PDF_ID\",\"regenerate_pdf\":true,\"output\":\"zugferd_invoice\",\"render_options\":{\"locale\":\"de-DE\",\"label_language\":\"de\",\"font\":\"arial\",\"bold_font\":\"arialbold\",\"accent_color_rgb\":[0,92,171]}}"
