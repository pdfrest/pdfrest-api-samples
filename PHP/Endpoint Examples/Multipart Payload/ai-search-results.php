<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Utils;

// Find people and medication names with AI Search PDF (Cloud only). Review results for accuracy and completeness.
// AI Search PDF is available on the US Cloud service only.
$apiUrl = 'https://api.pdfrest.com';

// AI Search PDF is unavailable on the EU service and in self-hosted deployments.

$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'; // Replace with your API key.
$client = new Client(['http_errors' => false]);

$searches = [
    ['type' => 'ai_preset', 'value' => 'person_name'],
    ['type' => 'prompt', 'value' => 'Find medication names.'],
];

$response = $client->request('POST', $apiUrl . '/ai-search-results', [
    'headers' => ['Api-Key' => $apiKey, 'Accept' => 'application/json'],
    'multipart' => [
        ['name' => 'file', 'contents' => Utils::tryFopen('/path/to/input.pdf', 'r'), 'filename' => 'input.pdf'],
        ['name' => 'searches', 'contents' => json_encode($searches)],
        ['name' => 'output_type', 'contents' => 'json'],
    ],
]);
if ($response->getStatusCode() >= 300) {
    fwrite(STDERR, (string) $response->getBody());
    exit(1);
}
$result = json_decode((string) $response->getBody(), true);
echo json_encode($result, JSON_PRETTY_PRINT) . PHP_EOL;
