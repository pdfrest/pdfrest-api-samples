#!/bin/sh

# Find people and medication names with AI Search PDF (Cloud only). Review results for accuracy and completeness.
set -eu

# AI Search PDF is available on the US Cloud service only.
API_URL="https://api.pdfrest.com"

# AI Search PDF is unavailable on the EU service and in self-hosted deployments.

API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key.

RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/ai-search-results" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --form 'file=@/path/to/input.pdf;type=application/pdf' \
  --form-string 'searches=[{"type":"ai_preset","value":"person_name"},{"type":"prompt","value":"Find medication names."}]' \
  --form-string 'output_type=json')
printf '%s\n' "$RESPONSE" | jq .
