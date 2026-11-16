import json

import requests
from requests_toolbelt import MultipartEncoder

# Highlight matching text directly, without a separate search call.
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
                "rgb_color": "255,255,0",
                "opacity": json.dumps(0.4),
            }
        )
        response = requests.post(
            f"{api_url}/pdf-with-highlighted-text",
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

except requests.HTTPError as error:
    print(error.response.text)
    raise SystemExit(1)
