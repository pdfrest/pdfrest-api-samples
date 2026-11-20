#!/bin/sh

# Search PDF first, optionally remove unwanted matches, then highlight the retained matches.
set -eu

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key.

RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/search-results" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --form 'file=@/path/to/input.pdf;type=application/pdf' \
  --form-string 'searches=[{"type":"literal","value":"Confidential"}]' \
  --form-string 'output_type=json')
printf '%s\n' "$RESPONSE" | jq .

# A program can filter .matches here before submitting the retained matches.
SELECTIONS="$RESPONSE"
if [ "$(printf '%s' "$SELECTIONS" | jq '.matches | length')" -eq 0 ]; then
  echo "No matches found; no PDF was modified."
  exit 0
fi
# Reuse the same unchanged PDF that was searched.
INPUT_ID=$(printf '%s' "$SELECTIONS" | jq -er '.inputId')
RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/pdf-with-highlighted-text" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --form-string "id=$INPUT_ID" \
  --form-string "selections=$SELECTIONS")
printf '%s\n' "$RESPONSE" | jq .
