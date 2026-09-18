<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Request;

// Upload invoice XML and PDF, then create a ZUGFeRD / Factur-X PDF/A-3 invoice by resource ID.
// By default, we use the US-based API service. This is the primary endpoint for global use.
$apiUrl = 'https://api.pdfrest.com';

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//$apiUrl = 'https://eu-api.pdfrest.com';

// Set these paths to your CII XML invoice and the corresponding visual PDF.
$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
$invoiceXml = '/path/to/invoice.xml';
$invoicePdf = '/path/to/invoice.pdf';
$client = new Client(['http_errors' => false]);

$uploadRequest = new Request('POST', $apiUrl . '/upload', ['Api-Key' => $apiKey, 'Content-Type' => 'application/xml', 'Content-Filename' => basename($invoiceXml)]);
$upload = $client->sendAsync($uploadRequest, [
    'body' => fopen($invoiceXml, 'r'),
])->wait();
if ($upload->getStatusCode() >= 300) { fwrite(STDERR, (string) $upload->getBody()); exit(1); }
$xmlId = json_decode($upload->getBody(), true)['files'][0]['id'];

$pdfUploadRequest = new Request('POST', $apiUrl . '/upload', ['Api-Key' => $apiKey, 'Content-Type' => 'application/pdf', 'Content-Filename' => basename($invoicePdf)]);
$pdfUpload = $client->sendAsync($pdfUploadRequest, [
    'body' => fopen($invoicePdf, 'r'),
])->wait();
if ($pdfUpload->getStatusCode() >= 300) { fwrite(STDERR, (string) $pdfUpload->getBody()); exit(1); }
$pdfId = json_decode($pdfUpload->getBody(), true)['files'][0]['id'];

// pdfRest preserves the supplied PDF when it agrees with the canonical XML.
// Fallback generation handles a mismatch or an unconfirmed PDF/XML match.
// The render options style only that replacement PDF, not a preserved supplied PDF.
$request = new Request('POST', $apiUrl . '/zugferd-pdf', ['Accept' => 'application/json', 'Api-Key' => $apiKey]);
$response = $client->sendAsync($request, [
    'json' => ['id' => $xmlId, 'pdf_id' => $pdfId, 'regenerate_pdf' => true, 'output' => 'zugferd_invoice', 'render_options' => ['locale' => 'de-DE', 'label_language' => 'de', 'font' => 'arial', 'bold_font' => 'arialbold', 'accent_color_rgb' => [0, 92, 171]]],
])->wait();
echo $response->getBody();
if ($response->getStatusCode() >= 300) exit(1);
