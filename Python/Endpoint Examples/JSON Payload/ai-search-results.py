import json

import requests

# Find people and medication names with AI Search PDF (Cloud only). Review results for accuracy and completeness.
# AI Search PDF is available on the US Cloud service only.
api_url = "https://api.pdfrest.com"

# AI Search PDF is unavailable on the EU service and in self-hosted deployments.

api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"  # Replace with your API key.

try:
    # Upload once; subsequent JSON requests identify this PDF by resource ID.
    with open("/path/to/input.pdf", "rb") as input_file:
        upload_response = requests.post(
            f"{api_url}/upload",
            data=input_file,
            headers={
                "Api-Key": api_key,
                "Content-Filename": "input.pdf",
                "Content-Type": "application/octet-stream",
            },
        )
    upload_response.raise_for_status()
    input_id = upload_response.json()["files"][0]["id"]

    response = requests.post(
        f"{api_url}/ai-search-results",
        json={
            "id": input_id,
            "searches": [
                {"type": "ai_preset", "value": "person_name"},
                {"type": "prompt", "value": "Find medication names."},
            ],
            "output_type": "json",
        },
        headers={"Api-Key": api_key, "Accept": "application/json"},
    )
    response.raise_for_status()
    result = response.json()
    print(json.dumps(result, indent=2))

except requests.HTTPError as error:
    print(error.response.text)
    raise SystemExit(1)
