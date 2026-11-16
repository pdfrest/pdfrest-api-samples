#!/bin/sh

# Use AI to find potential PII, including unknown names and addresses, then redact retained matches. Review AI findings before applying.
set -eu

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key.

RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/ai-search-results" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --form 'file=@/path/to/input.pdf;type=application/pdf' \
  --form-string 'searches=[{"type":"ai_preset","value":"potential_pii"}]' \
  --form-string 'output_type=json')
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
RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/pdf-with-redacted-text-preview" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --form-string "id=$INPUT_ID" \
  --form-string "selections=$SELECTIONS")
printf '%s\n' "$RESPONSE" | jq .
# Preview annotations do not remove content. Inspect the preview before applying.
# This example immediately applies it to demonstrate the complete workflow.
PREVIEW_ID=$(printf '%s' "$RESPONSE" | jq -er '.outputId')
RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/pdf-with-redacted-text-applied" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --form-string "id=$PREVIEW_ID")
printf '%s\n' "$RESPONSE" | jq .
