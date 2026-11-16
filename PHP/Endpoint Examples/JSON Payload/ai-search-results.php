<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Request;

// Find people and medication names with AI Search PDF (Cloud only). Review results for accuracy and completeness.
// By default, we use the US-based API service. This is the primary endpoint for global use.
$apiUrl = 'https://api.pdfrest.com';

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
// $apiUrl = 'https://eu-api.pdfrest.com';

$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'; // Replace with your API key.
$client = new Client(['http_errors' => false]);

$searches = [
    ['type' => 'ai_preset', 'value' => 'person_name'],
    ['type' => 'prompt', 'value' => 'Find medication names.'],
];

// Upload once; subsequent JSON requests identify this PDF by resource ID.
$uploadResponse = $client->send(new Request('POST', $apiUrl . '/upload', [
    'Api-Key' => $apiKey, 'Content-Filename' => 'input.pdf', 'Content-Type' => 'application/octet-stream',
], file_get_contents('/path/to/input.pdf')));
if ($uploadResponse->getStatusCode() >= 300) {
    fwrite(STDERR, (string) $uploadResponse->getBody());
    exit(1);
}
$inputId = json_decode((string) $uploadResponse->getBody(), true)['files'][0]['id'];

$response = $client->send(new Request('POST', $apiUrl . '/ai-search-results', [
    'Api-Key' => $apiKey, 'Accept' => 'application/json', 'Content-Type' => 'application/json',
], json_encode([
    'id' => $inputId,
    'searches' => $searches,
    'output_type' => 'json',
])));
if ($response->getStatusCode() >= 300) {
    fwrite(STDERR, (string) $response->getBody());
    exit(1);
}
$result = json_decode((string) $response->getBody(), true);
echo json_encode($result, JSON_PRETTY_PRINT) . PHP_EOL;
