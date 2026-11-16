import json

import requests

# Search PDF first, optionally remove unwanted matches, then replace the retained matches.
# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# api_url = "https://eu-api.pdfrest.com"

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
        f"{api_url}/search-results",
        json={
            "id": input_id,
            "searches": [{"type": "literal", "value": "Draft"}],
            "output_type": "json",
        },
        headers={"Api-Key": api_key, "Accept": "application/json"},
    )
    response.raise_for_status()
    result = response.json()
    print(json.dumps(result, indent=2))
    # A program can filter result["matches"] here before submitting the retained matches.
    if not result["matches"]:
        print("No matches found; no PDF was modified.")
    else:
        # Reuse the same unchanged PDF that was searched; do not use an edited copy.
        # Add an exact replacement to each retained match; each can have different text.
        # Optional layout_mode can also be set per match; omission uses reflow.
        for match in result["matches"]:
            match["replacement"] = "Final"
        response = requests.post(
            f"{api_url}/pdf-with-replaced-text",
            json={
                "id": result["inputId"],
                "selections": result,
            },
            headers={"Api-Key": api_key, "Accept": "application/json"},
        )
        response.raise_for_status()
        result = response.json()
        print(json.dumps(result, indent=2))

except requests.HTTPError as error:
    print(error.response.text)
    raise SystemExit(1)
