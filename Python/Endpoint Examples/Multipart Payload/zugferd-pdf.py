import json
import os
import requests
from requests_toolbelt import MultipartEncoder

# Create a ZUGFeRD / Factur-X PDF/A-3 invoice from XML and an existing invoice PDF.
# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
#api_url = "https://eu-api.pdfrest.com"

# Set these paths to your CII XML invoice and the corresponding visual PDF.
api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
invoice_xml = "/path/to/invoice.xml"
invoice_pdf = "/path/to/invoice.pdf"

# pdfRest preserves the supplied PDF when it agrees with the canonical XML.
# Fallback: generate a replacement PDF for a mismatch or unconfirmed PDF/XML match.
# These styles apply only to that fallback-generated PDF, not to a preserved PDF.
mp_encoder_zugferd = MultipartEncoder(
    fields={
        'file': ('invoice.xml', open(invoice_xml, 'rb'), 'application/xml'),
        'pdf_file': ('invoice.pdf', open(invoice_pdf, 'rb'), 'application/pdf'),
        'regenerate_pdf': 'true',
        'render_options': json.dumps({'locale': 'de-DE', 'label_language': 'de', 'font': 'arial', 'bold_font': 'arialbold', 'accent_color_rgb': [0, 92, 171]}),
        'output': 'zugferd_invoice',
    }
)

headers = {
    'Accept': 'application/json',
    'Content-Type': mp_encoder_zugferd.content_type,
    'Api-Key': api_key
}

print("Sending POST request to zugferd-pdf endpoint...")
response = requests.post(api_url + '/zugferd-pdf', data=mp_encoder_zugferd, headers=headers)
print("Response status code: " + str(response.status_code))
if response.ok:
    print(json.dumps(response.json(), indent=2))
else:
    print(response.text)
