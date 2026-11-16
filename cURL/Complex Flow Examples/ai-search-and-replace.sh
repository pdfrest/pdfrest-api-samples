#!/bin/sh

# Use AI to find people’s names without a list of names or regex, then replace retained names with Person.
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

PAYLOAD=$(jq -n --arg id "$INPUT_ID" --argjson searches '[{"type":"ai_preset","value":"person_name"}]' --argjson output_type '"json"' \
  '{id: $id, searches: $searches, output_type: $output_type}')
RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/ai-search-results" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --header 'Content-Type: application/json' \
  --data-binary "$PAYLOAD")
printf '%s\n' "$RESPONSE" | jq .

# AI Search selects content by meaning; ordinary Search needs literal text, a regex, or a pattern preset.
# Review the returned text and confidence; remove unwanted matches before the action.
# A program can filter .matches here before submitting the retained matches.
SELECTIONS="$RESPONSE"
if [ "$(printf '%s' "$SELECTIONS" | jq '.matches | length')" -eq 0 ]; then
  echo "No matches found; no PDF was modified."
  exit 0
fi
# Reuse the same unchanged PDF that was searched.
INPUT_ID=$(printf '%s' "$SELECTIONS" | jq -er '.inputId')
# Each retained name can have its own replacement. shrink_to_fit preserves surrounding text.
SELECTIONS=$(printf '%s' "$SELECTIONS" | jq '.matches |= map(. + {replacement: "Person", layout_mode: "shrink_to_fit"})')
PAYLOAD=$(jq -n --arg id "$INPUT_ID" --argjson selections "$SELECTIONS" \
  '{id: $id, selections: $selections}')
RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/pdf-with-replaced-text" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --header 'Content-Type: application/json' \
  --data-binary "$PAYLOAD")
printf '%s\n' "$RESPONSE" | jq .
