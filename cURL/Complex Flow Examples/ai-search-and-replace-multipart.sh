#!/bin/sh

# Use AI to find people’s names without a list of names or regex, then replace retained names with Person.
set -eu

# AI Search PDF is available on the US Cloud service only.
API_URL="https://api.pdfrest.com"

# AI Search PDF is unavailable on the EU service and in self-hosted deployments.

API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key.

RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/ai-search-results" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --form 'file=@/path/to/input.pdf;type=application/pdf' \
  --form-string 'searches=[{"type":"ai_preset","value":"person_name"}]' \
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
# Each retained name can have its own replacement. shrink_to_fit preserves surrounding text.
SELECTIONS=$(printf '%s' "$SELECTIONS" | jq '.matches |= map(. + {replacement: "Person", layout_mode: "shrink_to_fit"})')
RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/pdf-with-replaced-text" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --form-string "id=$INPUT_ID" \
  --form-string "selections=$SELECTIONS")
printf '%s\n' "$RESPONSE" | jq .
