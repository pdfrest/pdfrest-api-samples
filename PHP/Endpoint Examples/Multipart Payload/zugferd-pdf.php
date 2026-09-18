<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Request;
use GuzzleHttp\Psr7\Utils;

// Create a ZUGFeRD / Factur-X PDF/A-3 invoice from XML and an existing invoice PDF.
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

$client = new Client();
$headers = ['Accept' => 'application/json', 'Api-Key' => $apiKey];

// pdfRest preserves the supplied PDF when it agrees with the canonical XML.
// Fallback: generate a replacement PDF for a mismatch or unconfirmed PDF/XML match.
// These styles apply only to that fallback-generated PDF, not to a preserved PDF.
$options = [
    'multipart' => [
        ['name' => 'file', 'contents' => Utils::tryFopen($invoiceXml, 'r'), 'filename' => basename($invoiceXml), 'headers' => ['Content-Type' => 'application/xml']],
        ['name' => 'pdf_file', 'contents' => Utils::tryFopen($invoicePdf, 'r'), 'filename' => basename($invoicePdf), 'headers' => ['Content-Type' => 'application/pdf']],
        ['name' => 'regenerate_pdf', 'contents' => 'true'],
        ['name' => 'render_options', 'contents' => json_encode(['locale' => 'de-DE', 'label_language' => 'de', 'font' => 'arial', 'bold_font' => 'arialbold', 'accent_color_rgb' => [0, 92, 171]])],
        ['name' => 'output', 'contents' => 'zugferd_invoice'],
    ]
];

$request = new Request('POST', $apiUrl . '/zugferd-pdf', $headers);
$response = $client->sendAsync($request, $options)->wait();

echo $response->getBody();
