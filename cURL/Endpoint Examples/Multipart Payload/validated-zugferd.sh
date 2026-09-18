#!/bin/sh

# Validate a hybrid ZUGFeRD / Factur-X PDF without modifying it.
# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

# Replace the API key and PDF path below with your own values.
API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key
ZUGFERD_PDF="/path/to/zugferd-invoice.pdf"

curl --location "$API_URL/validated-zugferd" \
  --header "Accept: application/json" \
  --header "Api-Key: $API_KEY" \
  --form "file=@$ZUGFERD_PDF;type=application/pdf"
