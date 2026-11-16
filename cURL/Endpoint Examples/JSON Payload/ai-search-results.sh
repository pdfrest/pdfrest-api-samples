#!/bin/sh

# Find people and medication names with AI Search PDF (Cloud only). Review results for accuracy and completeness.
set -eu

# AI Search PDF is available on the US Cloud service only.
API_URL="https://api.pdfrest.com"

# AI Search PDF is unavailable on the EU service and in self-hosted deployments.

API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key.

# Upload once; subsequent JSON requests identify this PDF by resource ID.
UPLOAD_RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: $API_KEY" \
  --header 'Content-Filename: input.pdf' \
  --header 'Content-Type: application/octet-stream' \
  --data-binary '@/path/to/input.pdf')
INPUT_ID=$(printf '%s' "$UPLOAD_RESPONSE" | jq -er '.files[0].id')

PAYLOAD=$(jq -n --arg id "$INPUT_ID" --argjson searches '[{"type":"ai_preset","value":"person_name"},{"type":"prompt","value":"Find medication names."}]' --argjson output_type '"json"' \
  '{id: $id, searches: $searches, output_type: $output_type}')
RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/ai-search-results" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --header 'Content-Type: application/json' \
  --data-binary "$PAYLOAD")
printf '%s\n' "$RESPONSE" | jq .
