import json

import requests

# Use AI to find potential PII, including unknown names and addresses, then redact retained matches. Review AI findings before applying.
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
            "searches": [{"type": "ai_preset", "value": "potential_pii"}],
            "output_type": "json",
        },
        headers={"Api-Key": api_key, "Accept": "application/json"},
    )
    response.raise_for_status()
    result = response.json()
    print(json.dumps(result, indent=2))
    # AI Search selects content by meaning; ordinary Search needs literal text, a regex, or a pattern preset.
    # Review the returned text and confidence; remove unwanted matches before the action.
    # A program can filter result["matches"] here before submitting the retained matches.
    if not result["matches"]:
        print("No matches found; no PDF was modified.")
    else:
        # Reuse the same unchanged PDF that was searched; do not use an edited copy.
        response = requests.post(
            f"{api_url}/pdf-with-redacted-text-preview",
            json={
                "id": result["inputId"],
                "selections": result,
            },
            headers={"Api-Key": api_key, "Accept": "application/json"},
        )
        response.raise_for_status()
        result = response.json()
        print(json.dumps(result, indent=2))
        # Preview annotations do not remove content. Inspect the preview before applying.
        # This example immediately applies it to demonstrate the complete workflow.
        response = requests.post(
            f"{api_url}/pdf-with-redacted-text-applied",
            json={
                "id": result["outputId"],
            },
            headers={"Api-Key": api_key, "Accept": "application/json"},
        )
        response.raise_for_status()
        print(json.dumps(response.json(), indent=2))

except requests.HTTPError as error:
    print(error.response.text)
    raise SystemExit(1)
