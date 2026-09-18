import json
import os
import requests
from requests_toolbelt import MultipartEncoder

# Validate a hybrid ZUGFeRD / Factur-X PDF without modifying it.
# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
#api_url = "https://eu-api.pdfrest.com"

# Set this path to the completed hybrid ZUGFeRD or Factur-X PDF you want to validate.
api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
zugferd_pdf = "/path/to/zugferd-invoice.pdf"

mp_encoder_zugferd = MultipartEncoder(
    fields={'file': ('zugferd-invoice.pdf', open(zugferd_pdf, 'rb'), 'application/pdf')}
)
headers = {
    'Accept': 'application/json',
    'Content-Type': mp_encoder_zugferd.content_type,
    'Api-Key': api_key
}

print("Sending POST request to validated-zugferd endpoint...")
response = requests.post(api_url + '/validated-zugferd', data=mp_encoder_zugferd, headers=headers)
print("Response status code: " + str(response.status_code))
if response.ok:
    print(json.dumps(response.json(), indent=2))
else:
    print(response.text)
