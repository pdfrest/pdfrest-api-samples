#!/bin/sh

# Find and replace all matches of each rule directly, without a separate search call.
set -eu

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key.

# Each rule uses one exact replacement for every match; omitted layout_mode uses reflow.

RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/pdf-with-replaced-text" \
  --header "Api-Key: $API_KEY" \
  --header 'Accept: application/json' \
  --form 'file=@/path/to/input.pdf;type=application/pdf' \
  --form-string 'replacements=[{"search":{"type":"literal","value":"Draft"},"replacement":"Final"},{"search":{"type":"literal","value":"Pending"},"replacement":"Ready"}]')
printf '%s\n' "$RESPONSE" | jq .
