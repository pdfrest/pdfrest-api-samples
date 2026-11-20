#!/bin/sh

# Use AI to find confidentiality obligations by meaning, then highlight them. No exact wording or regex is supplied.
set -eu

# AI Search PDF is available on the US Cloud service only.
API_URL="https://api.pdfrest.com"

# AI Search PDF is unavailable on the EU service and in self-hosted deployments.

API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key.

RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/ai-search-results" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --form 'file=@/path/to/input.pdf;type=application/pdf' \
  --form-string 'searches=[{"type":"prompt","value":"Find statements that require a party to keep information private or not disclose it to others, even when they do not use the word confidential."}]' \
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
RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/pdf-with-highlighted-text" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --form-string "id=$INPUT_ID" \
  --form-string "selections=$SELECTIONS")
printf '%s\n' "$RESPONSE" | jq .
