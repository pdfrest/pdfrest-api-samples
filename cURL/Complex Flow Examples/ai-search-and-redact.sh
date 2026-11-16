#!/bin/sh

# Use AI to find potential PII, including unknown names and addresses, then redact retained matches. Review AI findings before applying.
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

PAYLOAD=$(jq -n --arg id "$INPUT_ID" --argjson searches '[{"type":"ai_preset","value":"potential_pii"}]' --argjson output_type '"json"' \
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
PAYLOAD=$(jq -n --arg id "$INPUT_ID" --argjson selections "$SELECTIONS" \
  '{id: $id, selections: $selections}')
RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/pdf-with-redacted-text-preview" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --header 'Content-Type: application/json' \
  --data-binary "$PAYLOAD")
printf '%s\n' "$RESPONSE" | jq .
# Preview annotations do not remove content. Inspect the preview before applying.
# This example immediately applies it to demonstrate the complete workflow.
PREVIEW_ID=$(printf '%s' "$RESPONSE" | jq -er '.outputId')
PAYLOAD=$(jq -n --arg id "$PREVIEW_ID"  \
  '{id: $id}')
RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/pdf-with-redacted-text-applied" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --header 'Content-Type: application/json' \
  --data-binary "$PAYLOAD")
printf '%s\n' "$RESPONSE" | jq .
