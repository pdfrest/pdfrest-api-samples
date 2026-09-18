<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Request;
use GuzzleHttp\Psr7\Utils;

// Validate a hybrid ZUGFeRD / Factur-X PDF without modifying it.
// By default, we use the US-based API service. This is the primary endpoint for global use.
$apiUrl = 'https://api.pdfrest.com';

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//$apiUrl = 'https://eu-api.pdfrest.com';

// Set this path to the completed hybrid ZUGFeRD or Factur-X PDF you want to validate.
$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
$zugferdPdf = '/path/to/zugferd-invoice.pdf';

$client = new Client();
$headers = ['Accept' => 'application/json', 'Api-Key' => $apiKey];
$options = [
    'multipart' => [['name' => 'file', 'contents' => Utils::tryFopen($zugferdPdf, 'r'), 'filename' => basename($zugferdPdf), 'headers' => ['Content-Type' => 'application/pdf']]],
];
$request = new Request('POST', $apiUrl . '/validated-zugferd', $headers);
$response = $client->sendAsync($request, $options)->wait();
echo $response->getBody();
