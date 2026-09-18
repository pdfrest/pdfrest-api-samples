import json
import os
import requests

# Upload a hybrid PDF, then validate its ZUGFeRD / Factur-X package by resource ID.
# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
#api_url = "https://eu-api.pdfrest.com"

# Set this path to the completed hybrid ZUGFeRD or Factur-X PDF you want to validate.
api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
zugferd_pdf = "/path/to/zugferd-invoice.pdf"

upload = requests.post(api_url + '/upload', data=open(zugferd_pdf, 'rb'), headers={'Api-Key': api_key, 'Content-Type': 'application/pdf', 'Content-Filename': os.path.basename(zugferd_pdf)})
if not upload.ok:
    print(upload.text)
    exit()

response = requests.post(api_url + '/validated-zugferd', json={'id': upload.json()['files'][0]['id']}, headers={'Accept': 'application/json', 'Api-Key': api_key})
print(json.dumps(response.json(), indent=2))
