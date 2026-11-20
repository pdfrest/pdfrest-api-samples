import json

import requests
from requests_toolbelt import MultipartEncoder

# Find people and medication names with AI Search PDF (Cloud only). Review results for accuracy and completeness.
# AI Search PDF is available on the US Cloud service only.
api_url = "https://api.pdfrest.com"

# AI Search PDF is unavailable on the EU service and in self-hosted deployments.

api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"  # Replace with your API key.

try:
    with open("/path/to/input.pdf", "rb") as input_file:
        form = MultipartEncoder(
            fields={
                "file": ("input.pdf", input_file, "application/pdf"),
                "searches": json.dumps(
                    [
                        {"type": "ai_preset", "value": "person_name"},
                        {"type": "prompt", "value": "Find medication names."},
                    ]
                ),
                "output_type": "json",
            }
        )
        response = requests.post(
            f"{api_url}/ai-search-results",
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
