import json

import requests
from requests_toolbelt import MultipartEncoder

# Search PDF first, optionally remove unwanted matches, then redact the retained matches.
# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# api_url = "https://eu-api.pdfrest.com"

api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"  # Replace with your API key.

try:
    with open("/path/to/input.pdf", "rb") as input_file:
        form = MultipartEncoder(
            fields={
                "file": ("input.pdf", input_file, "application/pdf"),
                "searches": json.dumps([{"type": "literal", "value": "Confidential"}]),
                "output_type": "json",
            }
        )
        response = requests.post(
            f"{api_url}/search-results",
            data=form,
            headers={
                "Api-Key": api_key,
                "Accept": "application/json",
                "Content-Type": form.content_type,
            },
        )
    response.raise_for_status()
    result = response.json()
    print(json.dumps(result, indent=2))
    # A program can filter result["matches"] here before submitting the retained matches.
    if not result["matches"]:
        print("No matches found; no PDF was modified.")
    else:
        # Reuse the same unchanged PDF that was searched; do not use an edited copy.
        form = MultipartEncoder(
            fields={
                "id": result["inputId"],
                "selections": json.dumps(result),
            }
        )
        response = requests.post(
            f"{api_url}/pdf-with-redacted-text-preview",
            data=form,
            headers={
                "Api-Key": api_key,
                "Accept": "application/json",
                "Content-Type": form.content_type,
            },
        )
        response.raise_for_status()
        result = response.json()
        print(json.dumps(result, indent=2))
        # Preview annotations do not remove content. Inspect the preview before applying.
        # This example immediately applies it to demonstrate the complete workflow.
        form = MultipartEncoder(
            fields={
                "id": result["outputId"],
            }
        )
        response = requests.post(
            f"{api_url}/pdf-with-redacted-text-applied",
            data=form,
            headers={
                "Api-Key": api_key,
                "Accept": "application/json",
                "Content-Type": form.content_type,
            },
        )
        response.raise_for_status()
        print(json.dumps(response.json(), indent=2))

except requests.HTTPError as error:
    print(error.response.text)
    raise SystemExit(1)
