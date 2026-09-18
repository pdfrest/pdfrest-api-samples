import json
import os
import requests

# Upload invoice XML and PDF, then create a ZUGFeRD / Factur-X PDF/A-3 invoice by resource ID.
# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
#api_url = "https://eu-api.pdfrest.com"

# Set these paths to your CII XML invoice and the corresponding visual PDF.
api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
invoice_xml = "/path/to/invoice.xml"
invoice_pdf = "/path/to/invoice.pdf"

upload = requests.post(api_url + '/upload', data=open(invoice_xml, 'rb'), headers={'Api-Key': api_key, 'Content-Type': 'application/xml', 'Content-Filename': os.path.basename(invoice_xml)})
if not upload.ok:
    print(upload.text)
    exit()

pdf_upload = requests.post(api_url + '/upload', data=open(invoice_pdf, 'rb'), headers={'Api-Key': api_key, 'Content-Type': 'application/pdf', 'Content-Filename': os.path.basename(invoice_pdf)})
if not pdf_upload.ok:
    print(pdf_upload.text)
    exit()

# pdfRest preserves the supplied PDF when it agrees with the canonical XML.
# Fallback generation handles a mismatch or an unconfirmed PDF/XML match.
# The render options style only that replacement PDF, not a preserved supplied PDF.
response = requests.post(api_url + '/zugferd-pdf', json={
    "id": upload.json()["files"][0]["id"], "pdf_id": pdf_upload.json()["files"][0]["id"], "regenerate_pdf": True, "output": "zugferd_invoice",
    "render_options": {"locale": "de-DE", "label_language": "de", "font": "arial", "bold_font": "arialbold", "accent_color_rgb": [0, 92, 171]},
}, headers={"Accept": "application/json", "Api-Key": api_key})
print(json.dumps(response.json(), indent=2))
