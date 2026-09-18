#!/bin/sh

# Upload a hybrid PDF, then validate its ZUGFeRD / Factur-X package by resource ID.
# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

# Replace the API key and PDF path below with your own values.
API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key
ZUGFERD_PDF="/path/to/zugferd-invoice.pdf"

PDF_ID=$(curl --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: $API_KEY" \
  --header "Content-Filename: $(basename "$ZUGFERD_PDF")" \
  --header "Content-Type: application/pdf" \
  --data-binary "@$ZUGFERD_PDF" | jq -r '.files[0].id')

test -n "$PDF_ID" && test "$PDF_ID" != "null" || { echo "PDF upload failed" >&2; exit 1; }

curl --location "$API_URL/validated-zugferd" \
  --header "Accept: application/json" \
  --header "Api-Key: $API_KEY" \
  --header "Content-Type: application/json" \
  --data "{\"id\":\"$PDF_ID\"}"
