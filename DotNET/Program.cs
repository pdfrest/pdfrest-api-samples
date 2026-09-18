// Minimal .NET 8 dispatcher for DotNET samples
// - Loads .env for PDFREST_API_KEY and optional PDFREST_URL
// - Dispatches to endpoint samples located under folder structure
//   * markdown-json: two-step upload + JSON call to markdown endpoint

using DotNetEnv;

static void PrintUsage()
{
    Console.Error.WriteLine("Usage: dotnet run -- <command> [args]\n");

    Console.Error.WriteLine("JSON Payload (upload + JSON):");
    Console.Error.WriteLine("  Conversions:");
    Console.Error.WriteLine("    markdown-json <pdf>                 Convert PDF to Markdown");
    Console.Error.WriteLine("    rasterized-pdf <pdf>               Rasterize PDF pages");
    Console.Error.WriteLine("    pdf <file>                         Convert file to PDF");
    Console.Error.WriteLine("    pdf-from-email <file>              Convert Email (.eml) to PDF");
    Console.Error.WriteLine("    pdf-from-postscript <ps> <jobOptions>  Convert PostScript to PDF");
    Console.Error.WriteLine("    postscript <pdf>                   Convert PDF to PostScript");
    Console.Error.WriteLine("    pdf-from-markdown|csv|json|xml|text <file>  Structured input to PDF");
    Console.Error.WriteLine("    pdfa <file>                        Convert to PDF/A");
    Console.Error.WriteLine("    zugferd-pdf <xml> <pdf>            Create a ZUGFeRD / Factur-X PDF/A-3 invoice");
    Console.Error.WriteLine("    pdfx <file>                        Convert to PDF/X");
    Console.Error.WriteLine("    tdm-reserved-pdf <pdf>             Apply TDM rights metadata");
    Console.Error.WriteLine("    png|jpg|gif|bmp <file>             Convert to image format");
    Console.Error.WriteLine("    word|excel|powerpoint|tif <file>   Convert to Office/TIFF");
    Console.Error.WriteLine("  Info / Extract:");
    Console.Error.WriteLine("    pdf-info <pdf>                     Document properties and stats");
    Console.Error.WriteLine("    validated-zugferd <pdf>            Validate a ZUGFeRD / Factur-X PDF");
    Console.Error.WriteLine("    extracted-text <pdf>               Extract text to JSON");
    Console.Error.WriteLine("    summarized-pdf-text <pdf>          Summarize text");
    Console.Error.WriteLine("    translated-pdf-text <pdf>          Translate text");
    Console.Error.WriteLine("    extracted-images <pdf>             Extract embedded images");
    Console.Error.WriteLine("    exported-form-data <pdf>           Export form data (XML)");
    Console.Error.WriteLine("  PDF Transforms:");
    Console.Error.WriteLine("    compressed-pdf <pdf>               Compress PDF");
    Console.Error.WriteLine("    linearized-pdf <pdf>               Optimize for fast web view");
    Console.Error.WriteLine("  Security:");
    Console.Error.WriteLine("    encrypted-pdf <pdf>                Set open password");
    Console.Error.WriteLine("    decrypted-pdf <pdf>                Remove open password");
    Console.Error.WriteLine("    restricted-pdf <pdf>               Apply permissions restrictions");
    Console.Error.WriteLine("    unrestricted-pdf <pdf>             Remove permissions restrictions");
    Console.Error.WriteLine("  Edit / Add:");
    Console.Error.WriteLine("    pdf-with-ocr-text <pdf>            Add OCR text layer");
    Console.Error.WriteLine("    pdf-with-imported-form-data <pdf> <xml/fdf>  Import form data");
    Console.Error.WriteLine("    pdf-with-added-image <pdf> <image> Add image");
    Console.Error.WriteLine("    pdf-with-added-attachment <pdf> <file>  Attach file");
    Console.Error.WriteLine("    pdf-with-converted-colors <pdf>    Convert colors to profile");
    Console.Error.WriteLine("    pdf-with-added-text <pdf>          Add text objects");
    Console.Error.WriteLine("    pdf-with-added-shapes <pdf>        Add review-panel shapes");
    Console.Error.WriteLine("    pdf-with-added-tables <pdf>        Add tagged project-status table");
    Console.Error.WriteLine("    pdf-with-acroforms <pdf>           Add acroforms");
    Console.Error.WriteLine("    pdf-with-page-boxes-set <pdf>      Set page boxes");
    Console.Error.WriteLine("  Redaction:");
    Console.Error.WriteLine("    pdf-with-redacted-text-preview <pdf>  Preview redactions");
    Console.Error.WriteLine("    pdf-with-redacted-text-applied <pdf>  Apply redactions");
    Console.Error.WriteLine("  Page / Files:");
    Console.Error.WriteLine("    blank-pdf                             Generate an empty PDF");
    Console.Error.WriteLine("    split-pdf <pdf>                    Split by page ranges");
    Console.Error.WriteLine("    merged-pdf <pdf1> <pdf2>           Merge two PDFs by id");
    Console.Error.WriteLine("  Resources / Packaging / Signing:");
    Console.Error.WriteLine("    upload <file>                      Upload file (resource id)");
    Console.Error.WriteLine("    get-resource <id> [out]            Download resource");
    Console.Error.WriteLine("    delete-resource <id>               Delete resource");
    Console.Error.WriteLine("    batch-delete <id1> [id2] [...]     Delete multiple resources");
    Console.Error.WriteLine("    zip <file1> <file2>                Zip two resources");
    Console.Error.WriteLine("    unzip <zipFile>                    Unzip resource");
    Console.Error.WriteLine("    signed-pdf <pdf> <pfx> <pass> <logo>     Sign PDF with PFX");
    Console.Error.WriteLine("    signed-pdf-non-pfx <pdf> <cert> <key>   Sign PDF with cert/key");
    Console.Error.WriteLine("    up-toolkit                         Query toolkit status\n");

    Console.Error.WriteLine("Multipart Payload (multipart/form-data):");
    Console.Error.WriteLine("  Conversions:");
    Console.Error.WriteLine("    pdf-multipart <file>               Convert to PDF");
    Console.Error.WriteLine("    pdf-from-email-multipart <file>    Convert Email (.eml) to PDF");
    Console.Error.WriteLine("    pdf-from-postscript-multipart <ps> <jobOptions>  Convert PostScript to PDF");
    Console.Error.WriteLine("    postscript-multipart <pdf>         Convert PDF to PostScript");
    Console.Error.WriteLine("    pdf-from-markdown|csv|json|xml|text-multipart <file>  Structured input to PDF");
    Console.Error.WriteLine("    markdown-multipart <file>          Convert to Markdown");
    Console.Error.WriteLine("    rasterized-pdf-multipart <pdf>     Rasterize PDF");
    Console.Error.WriteLine("    pdfa-multipart <file>              Convert to PDF/A");
    Console.Error.WriteLine("    zugferd-pdf-multipart <xml> <pdf>  Create a ZUGFeRD / Factur-X PDF/A-3 invoice");
    Console.Error.WriteLine("    pdfx-multipart <file>              Convert to PDF/X");
    Console.Error.WriteLine("    tdm-reserved-pdf-multipart <pdf>   Apply TDM rights metadata");
    Console.Error.WriteLine("    png-multipart|jpg-multipart|gif-multipart|bmp-multipart|tif-multipart <file>  Convert to image");
    Console.Error.WriteLine("    word-multipart|excel-multipart|powerpoint-multipart <file>  Convert Office");
    Console.Error.WriteLine("  Info / Extract:");
    Console.Error.WriteLine("    pdf-info-multipart <pdf>           Document properties and stats");
    Console.Error.WriteLine("    validated-zugferd-multipart <pdf>  Validate a ZUGFeRD / Factur-X PDF");
    Console.Error.WriteLine("    extracted-text-multipart <pdf>     Extract text to JSON");
    Console.Error.WriteLine("    summarized-pdf-text-multipart <pdf>  Summarize text");
    Console.Error.WriteLine("    translated-pdf-text-multipart <pdf>  Translate text");
    Console.Error.WriteLine("    extracted-images-multipart <pdf>   Extract images");
    Console.Error.WriteLine("  PDF Transforms:");
    Console.Error.WriteLine("    compressed-pdf-multipart <pdf>     Compress PDF");
    Console.Error.WriteLine("    linearized-pdf-multipart <pdf>     Optimize for fast web view");
    Console.Error.WriteLine("  Security:");
    Console.Error.WriteLine("    encrypted-pdf-multipart <pdf> <pass>      Set open password");
    Console.Error.WriteLine("    decrypted-pdf-multipart <pdf> <pass>      Remove open password");
    Console.Error.WriteLine("    restricted-pdf-multipart <pdf> <permPass> Apply permissions restrictions");
    Console.Error.WriteLine("    unrestricted-pdf-multipart <pdf> <permPass> Remove permissions restrictions");
    Console.Error.WriteLine("  Edit / Add:");
    Console.Error.WriteLine("    pdf-with-ocr-text-multipart <pdf>  Add OCR text layer");
    Console.Error.WriteLine("    pdf-with-imported-form-data-multipart <pdf> <data>  Import form data");
    Console.Error.WriteLine("    pdf-with-added-image-multipart <pdf> <image>  Add image");
    Console.Error.WriteLine("    pdf-with-added-attachment-multipart <pdf> <file>  Attach file");
    Console.Error.WriteLine("    pdf-with-converted-colors-multipart <pdf>  Convert colors to profile");
    Console.Error.WriteLine("    pdf-with-added-text-multipart <pdf>   Add text objects");
    Console.Error.WriteLine("    pdf-with-added-shapes-multipart <pdf> Add review-panel shapes");
    Console.Error.WriteLine("    pdf-with-added-tables-multipart <pdf> Add tagged project-status table");
    Console.Error.WriteLine("    pdf-with-acroforms-multipart <pdf>    Add acroforms");
    Console.Error.WriteLine("    pdf-with-page-boxes-set-multipart <pdf>  Set page boxes");
    Console.Error.WriteLine("  Redaction:");
    Console.Error.WriteLine("    pdf-with-redacted-text-preview-multipart <pdf>  Preview redactions");
    Console.Error.WriteLine("    pdf-with-redacted-text-applied-multipart <pdf>  Apply redactions");
    Console.Error.WriteLine("  Page / Files:");
    Console.Error.WriteLine("    blank-pdf-multipart                Generate an empty PDF");
    Console.Error.WriteLine("    split-pdf-multipart <pdf>         Split by page ranges");
    Console.Error.WriteLine("    merged-pdf-multipart <f1> <f2>    Merge two files");
    Console.Error.WriteLine("  Resources / Packaging / Signing:");
    Console.Error.WriteLine("    upload-multipart <file>            Upload file (resource id)");
    Console.Error.WriteLine("    get-resource-multipart <id> [out]  Download resource");
    Console.Error.WriteLine("    delete-resource-multipart <id>     Delete resource");
    Console.Error.WriteLine("    batch-delete-multipart <id1> [id2] [...]  Delete multiple resources");
    Console.Error.WriteLine("    zip-multipart <f1> <f2>            Zip two files");
    Console.Error.WriteLine("    unzip-multipart <zipFile>          Unzip resource");
    Console.Error.WriteLine("    signed-pdf-multipart <pdf> <pfx> <pass> <logo>    Sign PDF with PFX");
    Console.Error.WriteLine("    signed-pdf-non-pfx-multipart <pdf> <cert> <key>  Sign PDF with cert/key");
    Console.Error.WriteLine("    request-status-multipart <pdf>     Poll processing status");
    Console.Error.WriteLine("    up-toolkit-multipart               Query toolkit status\n");

    Console.Error.WriteLine("Complex Flows:");
    Console.Error.WriteLine("  merge-different-file-types <image> <ppt>   Convert then merge");
    Console.Error.WriteLine("  decrypt-add-reencrypt <pdf> <image> [pass] Decrypt, add image, re-encrypt");
    Console.Error.WriteLine("  ocr-with-extract-text <pdf>                OCR then extract text");
    Console.Error.WriteLine("  pdfa-3b-with-attachment <pdf> <xml>        Attach XML then PDF/A-3b");
    Console.Error.WriteLine("  preserve-word-document <office>            Office → PDF → PDF/A-3b");
    Console.Error.WriteLine("  protected-watermark <pdf>                  Watermark then restrict");
    Console.Error.WriteLine("  redact-preview-and-finalize <pdf>          Preview then apply redactions\n");
    Console.Error.WriteLine("  create-invoice-from-structured-data         Generate invoice from JSON and CSV\n");
    Console.Error.WriteLine("  refry-pdf <pdf> <jobOptions> [outputPdf]    Convert PDF to PostScript and back\n");

    Console.Error.WriteLine("Environment (.env supported):");
    Console.Error.WriteLine("  PDFREST_API_KEY=...    Required API key");
    Console.Error.WriteLine("  PDFREST_URL=...        Optional base URL (e.g., https://eu-api.pdfrest.com for EU/GDPR)");
}

// Entry point
Env.Load();
var argv = Environment.GetCommandLineArgs().Skip(1).ToArray();
if (argv.Length == 0)
{
    PrintUsage();
    return;
}

var cmd = argv[0].Trim().ToLowerInvariant();
var rest = argv.Skip(1).ToArray();

switch (cmd)
{
    case "summarized-pdf-text":
        await Samples.EndpointExamples.JsonPayload.SummarizedPdfText.Execute(rest);
        break;
    case "translated-pdf-text":
        await Samples.EndpointExamples.JsonPayload.TranslatedPdfText.Execute(rest);
        break;
    case "markdown-json":
        await Samples.EndpointExamples.JsonPayload.Markdown.Execute(rest);
        break;
    case "rasterized-pdf":
        await Samples.EndpointExamples.JsonPayload.RasterizedPdf.Execute(rest);
        break;
    case "blank-pdf":
        await Samples.EndpointExamples.JsonPayload.BlankPdf.Execute(rest);
        break;
    case "pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.Pdf.Execute(rest);
        break;
    case "pdf-from-email":
        await Samples.EndpointExamples.JsonPayload.PdfFromEmail.Execute(rest);
        break;
    case "pdf-from-postscript":
        await Samples.EndpointExamples.JsonPayload.PdfFromPostscript.Execute(rest);
        break;
    case "postscript":
        await Samples.EndpointExamples.JsonPayload.Postscript.Execute(rest);
        break;
    case "pdf-from-markdown":
        await Samples.EndpointExamples.JsonPayload.PdfFromMarkdown.Execute(rest);
        break;
    case "pdf-from-csv":
        await Samples.EndpointExamples.JsonPayload.PdfFromCsv.Execute(rest);
        break;
    case "pdf-from-json":
        await Samples.EndpointExamples.JsonPayload.PdfFromJson.Execute(rest);
        break;
    case "pdf-from-xml":
        await Samples.EndpointExamples.JsonPayload.PdfFromXml.Execute(rest);
        break;
    case "pdf-from-text":
        await Samples.EndpointExamples.JsonPayload.PdfFromText.Execute(rest);
        break;
    case "zugferd-pdf":
        await Samples.EndpointExamples.JsonPayload.ZugferdPdf.Execute(rest);
        break;
    case "validated-zugferd":
        await Samples.EndpointExamples.JsonPayload.ValidatedZugferd.Execute(rest);
        break;
    case "pdf-from-email-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfFromEmail.Execute(rest);
        break;
    case "pdf-from-postscript-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfFromPostscript.Execute(rest);
        break;
    case "postscript-multipart":
        await Samples.EndpointExamples.MultipartPayload.Postscript.Execute(rest);
        break;
    case "pdf-from-markdown-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfFromMarkdown.Execute(rest);
        break;
    case "pdf-from-csv-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfFromCsv.Execute(rest);
        break;
    case "pdf-from-json-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfFromJson.Execute(rest);
        break;
    case "pdf-from-xml-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfFromXml.Execute(rest);
        break;
    case "pdf-from-text-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfFromText.Execute(rest);
        break;
    case "zugferd-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.ZugferdPdf.Execute(rest);
        break;
    case "validated-zugferd-multipart":
        await Samples.EndpointExamples.MultipartPayload.ValidatedZugferd.Execute(rest);
        break;
    case "markdown-multipart":
        await Samples.EndpointExamples.MultipartPayload.Markdown.Execute(rest);
        break;
    case "png-multipart":
        await Samples.EndpointExamples.MultipartPayload.Png.Execute(rest);
        break;
    case "jpg-multipart":
        await Samples.EndpointExamples.MultipartPayload.Jpg.Execute(rest);
        break;
    case "gif-multipart":
        await Samples.EndpointExamples.MultipartPayload.Gif.Execute(rest);
        break;
    case "bmp-multipart":
        await Samples.EndpointExamples.MultipartPayload.Bmp.Execute(rest);
        break;
    case "tif-multipart":
        await Samples.EndpointExamples.MultipartPayload.Tif.Execute(rest);
        break;
    case "excel-multipart":
        await Samples.EndpointExamples.MultipartPayload.Excel.Execute(rest);
        break;
    case "powerpoint-multipart":
        await Samples.EndpointExamples.MultipartPayload.Powerpoint.Execute(rest);
        break;
    case "word-multipart":
        await Samples.EndpointExamples.MultipartPayload.Word.Execute(rest);
        break;
    case "summarized-pdf-text-multipart":
        await Samples.EndpointExamples.MultipartPayload.SummarizedPdfText.Execute(rest);
        break;
    case "translated-pdf-text-multipart":
        await Samples.EndpointExamples.MultipartPayload.TranslatedPdfText.Execute(rest);
        break;
    case "merge-different-file-types":
    case "merge":
        await Samples.ComplexFlowExamples.MergeDifferentFileTypes.Execute(rest);
        break;
    case "decrypt-add-reencrypt":
        await Samples.ComplexFlowExamples.DecryptAddReencrypt.Execute(rest);
        break;
    case "ocr-with-extract-text":
        await Samples.ComplexFlowExamples.OcrWithExtractText.Execute(rest);
        break;
    case "pdfa-3b-with-attachment":
        await Samples.ComplexFlowExamples.Pdfa3bWithAttachment.Execute(rest);
        break;
    case "preserve-word-document":
        await Samples.ComplexFlowExamples.PreserveWordDocument.Execute(rest);
        break;
    case "protected-watermark":
        await Samples.ComplexFlowExamples.ProtectedWatermark.Execute(rest);
        break;
    case "redact-preview-and-finalize":
        await Samples.ComplexFlowExamples.RedactPreviewAndFinalize.Execute(rest);
        break;
    case "create-invoice-from-structured-data":
        await Samples.ComplexFlowExamples.CreateInvoiceFromStructuredData.Execute(rest);
        break;
    case "refry-pdf":
        await Samples.ComplexFlowExamples.RefryPdf.Execute(rest);
        break;
    case "extracted-text":
        await Samples.EndpointExamples.JsonPayload.ExtractedText.Execute(rest);
        break;
    case "extracted-text-multipart":
        await Samples.EndpointExamples.MultipartPayload.ExtractedText.Execute(rest);
        break;
    case "extracted-images":
        await Samples.EndpointExamples.JsonPayload.ExtractedImages.Execute(rest);
        break;
    case "exported-form-data":
        await Samples.EndpointExamples.JsonPayload.ExportedFormData.Execute(rest);
        break;
    case "extracted-images-multipart":
        await Samples.EndpointExamples.MultipartPayload.ExtractedImages.Execute(rest);
        break;
    case "pdf-info":
        await Samples.EndpointExamples.JsonPayload.PdfInfo.Execute(rest);
        break;
    case "pdf-info-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfInfo.Execute(rest);
        break;
    case "merged-pdf":
        await Samples.EndpointExamples.JsonPayload.MergedPdf.Execute(rest);
        break;
    case "merged-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.MergedPdf.Execute(rest);
        break;
    case "split-pdf":
        await Samples.EndpointExamples.JsonPayload.SplitPdf.Execute(rest);
        break;
    case "split-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.SplitPdf.Execute(rest);
        break;
    case "upload":
        await Samples.EndpointExamples.JsonPayload.Upload.Execute(rest);
        break;
    case "upload-multipart":
        await Samples.EndpointExamples.MultipartPayload.Upload.Execute(rest);
        break;
    case "get-resource":
        await Samples.EndpointExamples.JsonPayload.GetResource.Execute(rest);
        break;
    case "get-resource-multipart":
        await Samples.EndpointExamples.MultipartPayload.GetResource.Execute(rest);
        break;
    case "delete-resource":
        await Samples.EndpointExamples.JsonPayload.DeleteResource.Execute(rest);
        break;
    case "delete-resource-multipart":
        await Samples.EndpointExamples.MultipartPayload.DeleteResource.Execute(rest);
        break;
    case "batch-delete":
        await Samples.EndpointExamples.JsonPayload.BatchDelete.Execute(rest);
        break;
    case "batch-delete-multipart":
        await Samples.EndpointExamples.MultipartPayload.BatchDelete.Execute(rest);
        break;
    case "png":
        await Samples.EndpointExamples.JsonPayload.Png.Execute(rest);
        break;
    case "jpg":
        await Samples.EndpointExamples.JsonPayload.Jpg.Execute(rest);
        break;
    case "gif":
        await Samples.EndpointExamples.JsonPayload.Gif.Execute(rest);
        break;
    case "bmp":
        await Samples.EndpointExamples.JsonPayload.Bmp.Execute(rest);
        break;
    case "pdf":
        await Samples.EndpointExamples.JsonPayload.Pdf.Execute(rest);
        break;
    case "pdfa-multipart":
        await Samples.EndpointExamples.MultipartPayload.Pdfa.Execute(rest);
        break;
    case "pdfx-multipart":
        await Samples.EndpointExamples.MultipartPayload.Pdfx.Execute(rest);
        break;
    case "tdm-reserved-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.TdmReservedPdf.Execute(rest);
        break;
    case "pdfa":
        await Samples.EndpointExamples.JsonPayload.Pdfa.Execute(rest);
        break;
    case "pdfx":
        await Samples.EndpointExamples.JsonPayload.Pdfx.Execute(rest);
        break;
    case "tdm-reserved-pdf":
        await Samples.EndpointExamples.JsonPayload.TdmReservedPdf.Execute(rest);
        break;
    case "excel":
        await Samples.EndpointExamples.JsonPayload.Excel.Execute(rest);
        break;
    case "powerpoint":
        await Samples.EndpointExamples.JsonPayload.Powerpoint.Execute(rest);
        break;
    case "word":
        await Samples.EndpointExamples.JsonPayload.Word.Execute(rest);
        break;
    case "tif":
        await Samples.EndpointExamples.JsonPayload.Tif.Execute(rest);
        break;
    case "rasterized-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.RasterizedPdf.Execute(rest);
        break;
    case "blank-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.BlankPdf.Execute(rest);
        break;
    case "compressed-pdf":
        await Samples.EndpointExamples.JsonPayload.CompressedPdf.Execute(rest);
        break;
    case "compressed-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.CompressedPdf.Execute(rest);
        break;
    case "linearized-pdf":
        await Samples.EndpointExamples.JsonPayload.LinearizedPdf.Execute(rest);
        break;
    case "linearized-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.LinearizedPdf.Execute(rest);
        break;
    case "encrypted-pdf":
        await Samples.EndpointExamples.JsonPayload.EncryptedPdf.Execute(rest);
        break;
    case "encrypted-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.EncryptedPdf.Execute(rest);
        break;
    case "decrypted-pdf":
        await Samples.EndpointExamples.JsonPayload.DecryptedPdf.Execute(rest);
        break;
    case "decrypted-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.DecryptedPdf.Execute(rest);
        break;
    case "restricted-pdf":
        await Samples.EndpointExamples.JsonPayload.RestrictedPdf.Execute(rest);
        break;
    case "restricted-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.RestrictedPdf.Execute(rest);
        break;
    case "unrestricted-pdf":
        await Samples.EndpointExamples.JsonPayload.UnrestrictedPdf.Execute(rest);
        break;
    case "unrestricted-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.UnrestrictedPdf.Execute(rest);
        break;
    case "flattened-forms-pdf":
        await Samples.EndpointExamples.JsonPayload.FlattenedFormsPdf.Execute(rest);
        break;
    case "flattened-forms-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.FlattenedFormsPdf.Execute(rest);
        break;
    case "flattened-annotations-pdf":
        await Samples.EndpointExamples.JsonPayload.FlattenedAnnotationsPdf.Execute(rest);
        break;
    case "flattened-annotations-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.FlattenedAnnotationsPdf.Execute(rest);
        break;
    case "flattened-layers-pdf":
        await Samples.EndpointExamples.JsonPayload.FlattenedLayersPdf.Execute(rest);
        break;
    case "flattened-layers-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.FlattenedLayersPdf.Execute(rest);
        break;
    case "flattened-transparencies-pdf":
        await Samples.EndpointExamples.JsonPayload.FlattenedTransparenciesPdf.Execute(rest);
        break;
    case "flattened-transparencies-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.FlattenedTransparenciesPdf.Execute(rest);
        break;
    case "pdf-with-ocr-text":
        await Samples.EndpointExamples.JsonPayload.PdfWithOcrText.Execute(rest);
        break;
    case "pdf-with-ocr-text-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfWithOcrText.Execute(rest);
        break;
    case "pdf-with-imported-form-data":
        await Samples.EndpointExamples.JsonPayload.PdfWithImportedFormData.Execute(rest);
        break;
    case "pdf-with-imported-form-data-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfWithImportedFormData.Execute(rest);
        break;
    case "pdf-with-added-image":
        await Samples.EndpointExamples.JsonPayload.PdfWithAddedImage.Execute(rest);
        break;
    case "pdf-with-added-image-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfWithAddedImage.Execute(rest);
        break;
    case "pdf-with-added-attachment":
        await Samples.EndpointExamples.JsonPayload.PdfWithAddedAttachment.Execute(rest);
        break;
    case "pdf-with-added-attachment-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfWithAddedAttachment.Execute(rest);
        break;
    case "pdf-with-converted-colors":
        await Samples.EndpointExamples.JsonPayload.PdfWithConvertedColors.Execute(rest);
        break;
    case "pdf-with-converted-colors-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfWithConvertedColors.Execute(rest);
        break;
    case "pdf-with-added-text":
        await Samples.EndpointExamples.JsonPayload.PdfWithAddedText.Execute(rest);
        break;
    case "pdf-with-added-text-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfWithAddedText.Execute(rest);
        break;
    case "pdf-with-added-shapes":
        await Samples.EndpointExamples.JsonPayload.PdfWithAddedShapes.Execute(rest);
        break;
    case "pdf-with-added-shapes-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfWithAddedShapes.Execute(rest);
        break;
    case "pdf-with-added-tables":
        await Samples.EndpointExamples.JsonPayload.PdfWithAddedTables.Execute(rest);
        break;
    case "pdf-with-added-tables-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfWithAddedTables.Execute(rest);
        break;
    case "pdf-with-acroforms":
        await Samples.EndpointExamples.JsonPayload.PdfWithAcroforms.Execute(rest);
        break;
    case "pdf-with-acroforms-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfWithAcroforms.Execute(rest);
        break;
    case "pdf-with-page-boxes-set":
        await Samples.EndpointExamples.JsonPayload.PdfWithPageBoxesSet.Execute(rest);
        break;
    case "pdf-with-page-boxes-set-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfWithPageBoxesSet.Execute(rest);
        break;
    case "pdf-with-redacted-text-preview":
        await Samples.EndpointExamples.JsonPayload.PdfWithRedactedTextPreview.Execute(rest);
        break;
    case "pdf-with-redacted-text-preview-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfWithRedactedTextPreview.Execute(rest);
        break;
    case "pdf-with-redacted-text-applied":
        await Samples.EndpointExamples.JsonPayload.PdfWithRedactedTextApplied.Execute(rest);
        break;
    case "pdf-with-redacted-text-applied-multipart":
        await Samples.EndpointExamples.MultipartPayload.PdfWithRedactedTextApplied.Execute(rest);
        break;
    case "watermarked-pdf":
        await Samples.EndpointExamples.JsonPayload.WatermarkedPdf.Execute(rest);
        break;
    case "watermarked-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.WatermarkedPdf.Execute(rest);
        break;
    case "zip":
        await Samples.EndpointExamples.JsonPayload.Zip.Execute(rest);
        break;
    case "zip-multipart":
        await Samples.EndpointExamples.MultipartPayload.Zip.Execute(rest);
        break;
    case "unzip":
        await Samples.EndpointExamples.JsonPayload.Unzip.Execute(rest);
        break;
    case "unzip-multipart":
        await Samples.EndpointExamples.MultipartPayload.Unzip.Execute(rest);
        break;
    case "up-toolkit":
        await Samples.EndpointExamples.JsonPayload.UpToolkit.Execute(rest);
        break;
    case "up-toolkit-multipart":
        await Samples.EndpointExamples.MultipartPayload.UpToolkit.Execute(rest);
        break;
    case "signed-pdf":
        await Samples.EndpointExamples.JsonPayload.SignedPdf.Execute(rest);
        break;
    case "signed-pdf-multipart":
        await Samples.EndpointExamples.MultipartPayload.SignedPdf.Execute(rest);
        break;
    case "signed-pdf-non-pfx":
        await Samples.EndpointExamples.JsonPayload.SignedPdfNonPfx.Execute(rest);
        break;
    case "signed-pdf-non-pfx-multipart":
        await Samples.EndpointExamples.MultipartPayload.SignedPdfNonPfx.Execute(rest);
        break;
    case "exported-form-data-multipart":
        await Samples.EndpointExamples.MultipartPayload.ExportedFormData.Execute(rest);
        break;
    case "request-status-multipart":
        await Samples.EndpointExamples.MultipartPayload.RequestStatus.Execute(rest);
        break;

    default:
        Console.Error.WriteLine($"Unknown command: {cmd}\n");
        PrintUsage();
        Environment.Exit(1);
        break;
}
