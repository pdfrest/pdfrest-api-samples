#!/bin/sh

# Search PDF first, optionally remove unwanted matches, then replace the retained matches.
set -eu

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key.

# Upload once; subsequent JSON requests identify this PDF by resource ID.
UPLOAD_RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: $API_KEY" \
  --header 'Content-Filename: input.pdf' \
  --header 'Content-Type: application/octet-stream' \
  --data-binary '@/path/to/input.pdf')
INPUT_ID=$(printf '%s' "$UPLOAD_RESPONSE" | jq -er '.files[0].id')

PAYLOAD=$(jq -n --arg id "$INPUT_ID" --argjson searches '[{"type":"literal","value":"Draft"}]' --argjson output_type '"json"' \
  '{id: $id, searches: $searches, output_type: $output_type}')
RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/search-results" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --header 'Content-Type: application/json' \
  --data-binary "$PAYLOAD")
printf '%s\n' "$RESPONSE" | jq .

# A program can filter .matches here before submitting the retained matches.
SELECTIONS="$RESPONSE"
if [ "$(printf '%s' "$SELECTIONS" | jq '.matches | length')" -eq 0 ]; then
  echo "No matches found; no PDF was modified."
  exit 0
fi
# Reuse the same unchanged PDF that was searched.
INPUT_ID=$(printf '%s' "$SELECTIONS" | jq -er '.inputId')
# Each retained match can have its own replacement and optional layout_mode (default: reflow).
SELECTIONS=$(printf '%s' "$SELECTIONS" | jq '.matches |= map(. + {replacement: "Final"})')
PAYLOAD=$(jq -n --arg id "$INPUT_ID" --argjson selections "$SELECTIONS" \
  '{id: $id, selections: $selections}')
RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/pdf-with-replaced-text" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --header 'Content-Type: application/json' \
  --data-binary "$PAYLOAD")
printf '%s\n' "$RESPONSE" | jq .
