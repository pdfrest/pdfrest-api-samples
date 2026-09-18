<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Request;

// Upload a hybrid PDF, then validate its ZUGFeRD / Factur-X package by resource ID.
// By default, we use the US-based API service. This is the primary endpoint for global use.
$apiUrl = 'https://api.pdfrest.com';

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//$apiUrl = 'https://eu-api.pdfrest.com';

// Set this path to the completed hybrid ZUGFeRD or Factur-X PDF you want to validate.
$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
$zugferdPdf = '/path/to/zugferd-invoice.pdf';
$client = new Client(['http_errors' => false]);

$uploadRequest = new Request('POST', $apiUrl . '/upload', ['Api-Key' => $apiKey, 'Content-Type' => 'application/pdf', 'Content-Filename' => basename($zugferdPdf)]);
$upload = $client->sendAsync($uploadRequest, [
    'body' => fopen($zugferdPdf, 'r'),
])->wait();
if ($upload->getStatusCode() >= 300) { fwrite(STDERR, (string) $upload->getBody()); exit(1); }
$pdfId = json_decode($upload->getBody(), true)['files'][0]['id'];

$request = new Request('POST', $apiUrl . '/validated-zugferd', ['Accept' => 'application/json', 'Api-Key' => $apiKey]);
$response = $client->sendAsync($request, [
    'json' => ['id' => $pdfId],
])->wait();
echo $response->getBody();
if ($response->getStatusCode() >= 300) exit(1);
