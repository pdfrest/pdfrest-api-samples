# Search and modify PDF text

Search PDF finds exact text, regular expressions, or pattern presets and returns JSON containing matched text and page coordinates without changing the PDF. AI Search PDF finds content by category or a description; it is available in Cloud only, and its results should be reviewed for accuracy and completeness. Both search tools operate on PDF text, including existing OCR text, rather than reading image pixels.

Highlight PDF and Replace PDF Text can find their own matches in a single request; no separate search is required. Redact PDF also supports direct searches through the existing redaction preview and apply examples.

Alternatively, a program can search first, parse the returned JSON, remove unwanted entries from `matches`, and pass the retained results as `selections` to another tool. Always use the PDF that was searched without modifying it between those calls.

## Direct endpoint examples

Each filename below exists in both `Endpoint Examples/JSON Payload` and `Endpoint Examples/Multipart Payload` for the six languages listed below.

| Tool | Filename (before language extension) | Java class |
| --- | --- | --- |
| Search PDF | `search-results` | `SearchResults` |
| AI Search PDF | `ai-search-results` | `AiSearchResults` |
| Highlight PDF | `pdf-with-highlighted-text` | `PdfWithHighlightedText` |
| Replace PDF Text | `pdf-with-replaced-text` | `PdfWithReplacedText` |
| Redact PDF preview and apply | `pdf-with-redacted-text-preview`, `pdf-with-redacted-text-applied` | `PDFWithRedactedTextPreview`, `PDFWithRedactedTextApplied` |

JSON examples first upload a local PDF to `/upload`, then send its resource ID with an `application/json` payload. Multipart examples send the PDF and options directly. Use the setup and dependency instructions in each language's README.

The direct Search example demonstrates literal, regex, and preset searches together. AI Search demonstrates a `person_name` preset and a custom description for medication names. Direct replacement changes every `Draft` match to `Final` and every `Pending` match to `Ready`; each rule supplies its own exact replacement string.

## Search followed by an action

These examples are in each language's `Complex Flow Examples` directory. The filenames without a suffix use JSON resource-ID requests; the `-multipart` variants use multipart requests. Java uses `Multipart` instead of `-multipart` in class names.

| Workflow | JSON filename | Multipart filename | Java classes |
| --- | --- | --- | --- |
| Search → Highlight | `search-and-highlight` | `search-and-highlight-multipart` | `SearchAndHighlight`, `SearchAndHighlightMultipart` |
| Search → Redact preview → Apply | `search-and-redact` | `search-and-redact-multipart` | `SearchAndRedact`, `SearchAndRedactMultipart` |
| Search → Replace | `search-and-replace` | `search-and-replace-multipart` | `SearchAndReplace`, `SearchAndReplaceMultipart` |

The workflows request inline JSON search output and mark the point where your program can filter `matches`. They retain all matches by default and stop without modifying a PDF when no matches are found.

Highlight and redaction accept retained results without additional match fields. Replacement requires adding `replacement` to every retained match; each match can have different replacement text and an optional `layout_mode`. The examples add `"replacement": "Final"` to each `Draft` match. This differs from a direct replacement rule, which applies the same replacement to all matches of that rule.

Replacement examples omit `layout_mode` to use the default `reflow`. Other modes are described in the API Reference. A replacement that cannot be completed safely fails the call rather than silently changing modes. Starter search results may contain masked text, which cannot validate as original text for replacement; use an entitled account for this search-to-replacement workflow or evaluate direct replacement with watermarked output.

Redaction previews contain annotations, not permanently removed content. The workflow prints the preview response and immediately applies it to demonstrate both steps. In a review workflow, inspect the preview before making the apply request.

Results are printed as JSON. For PDF outputs, use the returned `outputId` with the language's existing get-resource sample to download the file. AI Search–to–action workflows are not included in this set.

## Language directories

| Language | Endpoint examples | Workflow examples |
| --- | --- | --- |
| C# | [DotNET](DotNET/Endpoint%20Examples) | [DotNET workflows](DotNET/Complex%20Flow%20Examples) |
| Java | [Java](Java/Endpoint%20Examples) | [Java workflows](Java/Complex%20Flow%20Examples) |
| JavaScript | [JavaScript](JavaScript/Endpoint%20Examples) | [JavaScript workflows](JavaScript/Complex%20Flow%20Examples) |
| PHP | [PHP](PHP/Endpoint%20Examples) | [PHP workflows](PHP/Complex%20Flow%20Examples) |
| Python | [Python](Python/Endpoint%20Examples) | [Python workflows](Python/Complex%20Flow%20Examples) |
| cURL | [cURL](cURL/Endpoint%20Examples) | [cURL workflows](cURL/Complex%20Flow%20Examples) |

For example, from `DotNET`, run `dotnet run -- search-and-highlight /path/to/input.pdf` or `dotnet run -- search-and-highlight-multipart /path/to/input.pdf`. Direct JSON commands use the endpoint filename; add `-multipart` for the multipart version. Configure `PDFREST_API_KEY` and optionally `PDFREST_URL` as described in the .NET README.
