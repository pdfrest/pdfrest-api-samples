# DotNET pdfRest API Samples

This folder contains .NET 8 C# samples for the pdfRest API. It is a single console project that dispatches to many scenario‑focused samples:

- Endpoint Examples/JSON Payload: upload a file, then call an endpoint with JSON.
- Endpoint Examples/Multipart Payload: send files + parameters via multipart/form‑data.
- Complex Flow Examples: multi‑step workflows across multiple endpoints.

All samples are callable from a single dispatcher (`Program.cs`) using `dotnet run -- <command> [args]`.

## Requirements

- .NET 8 SDK installed (`dotnet --version` should print 8.x)

## Setup

1. From this folder, copy the environment template and set your key:

   ```bash
   cp .env.example .env
   # then edit .env and set your API key
   # PDFREST_API_KEY=your_api_key_here
   ```

2. Optional: set a regional API base (EU/GDPR). Add to `.env` if you want calls to remain in the EU:

   ```
   PDFREST_URL=https://eu-api.pdfrest.com
   ```

   For details see https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work

Notes
- This project loads `.env` via `DotNetEnv.Env.Load()` and reads settings from environment variables at runtime.
- Never commit your real API key. Only `.env.example` is versioned.

## Build

Run from this folder:

```bash
dotnet build
```

## Run: General Pattern

The dispatcher prints a help summary. To see it:

```bash
dotnet run --
```

Run any sample via:

```bash
dotnet run -- <command> [args]
```

Examples:

- Convert to Markdown (JSON two‑step):
  ```bash
  dotnet run -- markdown-json /path/to/input.pdf
  ```
- Convert to PDF (multipart):
  ```bash
  dotnet run -- pdf-multipart /path/to/input.html
  ```
- Export form data (JSON two‑step):
  ```bash
  dotnet run -- exported-form-data /path/to/input.pdf
  ```
- Merge different file types (complex flow):
  ```bash
  dotnet run -- merge-different-file-types image.png slides.pptx
  ```

## Useful Commands (sampler)

JSON two‑step (upload → JSON):
- `markdown-json <pdf>`
- `rasterized-pdf <pdf>`
- `extracted-text <pdf>`
- `extracted-images <pdf>`
- `pdf-info <pdf>`
- `exported-form-data <pdf>`

Multipart (single call with files/params):
- `pdf-multipart <file>`
- `png-multipart|jpg-multipart|gif-multipart|bmp-multipart|tif-multipart <file>`
- `word-multipart|excel-multipart|powerpoint-multipart <file>`
- `merged-pdf-multipart <file1> <file2>`
- `upload-multipart <file>` / `get-resource-multipart <id> [out]`

Complex flows (multi‑step):
- `merge-different-file-types <image> <ppt>`
- `ocr-with-extract-text <pdf>`
- `pdfa-3b-with-attachment <pdf> <xml>`
- `preserve-word-document <officeFile>`
- `protected-watermark <pdf>`
- `redact-preview-and-finalize <pdf>`

The dispatcher prints a complete list with arguments and brief descriptions.

## Handy Runner Scripts

Quick sanity runners are provided under `scripts/`:

- JSON endpoints: `scripts/run_json_samples.sh /path/to/input_dir`
- Multipart endpoints: `scripts/run_multipart_samples.sh /path/to/input_dir`
- Complex flows: `scripts/run_complex_flow.sh /path/to/input_dir`

Each script builds the project once, discovers suitable inputs inside the directory you provide, and runs a curated set of commands, reporting PASS/FAIL/SKIP.

## Output & Verification

- Most endpoints return JSON. The samples print the response body for inspection.
- Some flows produce downloadable files; use `upload*`/`get-resource*` or JSON resource IDs to retrieve binary outputs when needed.
- For binary verification, write bytes to disk and open the file (e.g., PDFs/images) in your preferred viewer.

## Optional deletion toggle

- Some samples include an optional delete step to remove uploaded/generated files from pdfRest servers. By default, this is OFF. To enable immediate deletion for supported samples, set the following environment variable:

  ```
  PDFREST_DELETE_SENSITIVE_FILES=true
  ```

  If unset or set to any value other than `true` (case-insensitive), deletion remains disabled. The default behavior retains files according to the File Retention Period shown on https://pdfrest.com/pricing.

## Project Structure & Build Notes

- Single .NET 8 console app; `Program.cs` dispatches to all samples.
- Source is organized in scenario folders; each sample is a static class with `Execute(string[] args)`.
- The project uses default compile globs and loads `.env` automatically.
- If you add new samples, follow the header + wrapper pattern already present and register the command in `Program.cs`.

## Region/GDPR

Set `PDFREST_URL=https://eu-api.pdfrest.com` to keep processing in the EU region. By default, the base URL is `https://api.pdfrest.com`.

## Support

- API docs and endpoint details: https://pdfrest.com/docs
- Solutions guides: https://pdfrest.com/solutions
