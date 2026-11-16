![pdfRest](https://cms.pdfrest.com/content/images/2022/11/pdfRest_logo_tag_750_275_light_bg.png)

**[pdfRest.com](https://pdfrest.com/) | [Get Started](https://pdfrest.com/getstarted/) | [API Lab](https://pdfrest.com/apilab/) | [Explore the Toolkit](https://pdfrest.com/apitools/) | [Solutions](https://pdfrest.com/learning/solutions/) | [Pricing](https://pdfrest.com/pricing/) | [Documentation](https://docs.pdfrest.com/) | [Support](https://pdfrest.com/support/)**

Official code samples for [pdfRest](https://pdfrest.com/), the enterprise grade REST API Toolkit for professional document workflows. Built by the digital document experts at [Datalogics](https://www.datalogics.com) and powered by trusted Adobe PDF Library technology, our platform makes complex PDF automation simple, scalable, and secure.

This repository contains multi language code samples designed to help you rapidly integrate 40+ programmatic PDF tools into your specific tech stack.

<br>

## pdfRest API Toolkit

| [Compress PDF](https://pdfrest.com/apitools/compress-pdf/) | [Convert to PDF](https://pdfrest.com/apitools/convert-to-pdf/) | [Convert to PDF/A](https://pdfrest.com/apitools/convert-to-pdfa/) | [Convert to PDF/X](https://pdfrest.com/apitools/convert-to-pdfx/) |
| :--- | :--- | :--- | :--- |
| **[Encrypt PDF](https://pdfrest.com/apitools/encrypt-pdf/)** | **[Restrict PDF](https://pdfrest.com/apitools/restrict-pdf/)** | **[Merge PDFs](https://pdfrest.com/apitools/merge-pdfs/)** | **[Split PDF](https://pdfrest.com/apitools/split-pdf/)** |
| **[Decrypt PDF](https://pdfrest.com/apitools/encrypt-pdf/)** | **[Unrestrict PDF](https://pdfrest.com/apitools/restrict-pdf/)** | **[Add to PDF](https://pdfrest.com/apitools/add-to-pdf/)** | **[PDF to Images](https://pdfrest.com/apitools/pdf-to-images/)** |
| **[Watermark PDF](https://pdfrest.com/apitools/watermark-pdf/)** | **[Flatten Transparencies](https://pdfrest.com/apitools/flatten-transparencies/)** | **[Flatten Annotations](https://pdfrest.com/apitools/flatten-annotations/)** | **[Flatten Layers](https://pdfrest.com/apitools/flatten-layers/)** |
| **[Query PDF](https://pdfrest.com/apitools/query-pdf/)** | **[Linearize PDF](https://pdfrest.com/apitools/linearize-pdf/)** | **[Upload Files](https://pdfrest.com/apitools/upload-files/)** | **[Zip Files](https://pdfrest.com/apitools/zip-files/)** |
| **[Flatten Forms](https://pdfrest.com/apitools/flatten-forms/)** | **[Import Form Data](https://pdfrest.com/apitools/import-form-data/)** | **[Export Form Data](https://pdfrest.com/apitools/export-form-data/)** | **[Extract Text](https://pdfrest.com/apitools/extract-text/)** |
| **[PDF to Word](https://pdfrest.com/apitools/pdf-to-word/)** | **[PDF to Excel](https://pdfrest.com/apitools/pdf-to-excel/)** | **[PDF to PowerPoint](https://pdfrest.com/apitools/pdf-to-powerpoint/)** | **[Extract Images](https://pdfrest.com/apitools/extract-images/)** |
| **[OCR to PDF](https://pdfrest.com/apitools/ocr-pdf/)** | **[API Polling](https://pdfrest.com/apitools/api-polling/)** | **[Rasterize PDF](https://pdfrest.com/apitools/rasterize-pdf/)** | **[Convert PDF Colors](https://pdfrest.com/apitools/convert-pdf-colors/)** |
| **[Redact PDF](https://pdfrest.com/apitools/redact-pdf/)** | **[PDF to Markdown](https://pdfrest.com/apitools/pdf-to-markdown/)** | **[Sign PDF](https://pdfrest.com/apitools/sign-pdf/)** | **[Summarize PDF](https://pdfrest.com/apitools/summarize-pdf/)** | 
| **[Translate PDF](https://pdfrest.com/apitools/translate-pdf/)** | **[TDM Reserve PDF](https://pdfrest.com/apitools/tdm-reserve-pdf/)** | | |

<br>

## Integration Options

### Package Managers & Containers
* **Python SDK:** Install the [pdfrest package](https://pypi.org/project/pdfrest/) via pip. View the [Python Documentation](https://python.pdfrest.com/).
* **Docker:** Pull images from [pdfRest on Docker Hub](https://hub.docker.com/r/pdfrest/pdf-api-toolkit).
* **Postman:** Fast-track testing with our [Postman Collection](https://www.postman.com/pdfrest).

### AI Automation
Connect with AI solutions to automate complex PDF operations. For example, [pdfAssistant.ai](https://pdfassistant.ai/) uses this API to provide an intelligent, chat-based PDF processing interface.

<br>

## Quick Start Guide

1.  **Get an API Key:** [Register for a free account](https://pdfrest.com/getstarted/) to receive your key (includes 100 free monthly calls).
2.  **Clone this Repo:** Use the code samples in this repository to see how to submit requests programmatically.
3.  **Configure Samples:**
    * Find the comment: `Place your api key here`
    * Replace `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` with your actual key.

### Downloading Files
Each directory contains a `get-resource` sample. When you perform a POST call, the API returns a UUID for your file. Replace `xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` in the sample with your specific UUID to download the output.

### Privacy & File Deletion
To delete sensitive files immediately after processing, edit the sample file and set its local deletion flag to `true`. This is disabled by default. 

* JavaScript: Change `const DELETE_SENSITIVE_FILES = false` to `true`
* Python: Change `DELETE_SENSITIVE_FILES = False` to `True`
* PHP: Change `$DELETE_SENSITIVE_FILES = false` to `true`
* .NET (C#): Change `var deleteSensitiveFiles = false` to `true`
* Java: Change `final boolean DELETE_SENSITIVE_FILES = false` to `true`
* cURL: Uncomment `# DELETE_SENSITIVE_FILES=true`

<br>

## API Documentation

### Search, highlight, redact, and replace

The [search and modify examples](search-and-modify-examples.md) cover Search PDF, AI Search PDF, Highlight PDF, and Replace PDF Text, plus workflows that pass Search PDF results to Highlight PDF, Redact PDF, or Replace PDF Text. All examples are available in C#, Java, JavaScript, PHP, Python, and cURL, with both JSON resource-ID and multipart payloads.

After you have successfully sent an API Call using these examples, take a look at the [API Reference Guide](https://docs.pdfrest.com/cloud-api-reference/) for a full description of each endpoint and parameters you can adjust to customize your solution.

<br>

## Support
Need help? Contact us through our [Support form](https://pdfrest.com/support) and our team will assist you.
