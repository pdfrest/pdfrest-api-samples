<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Utils;

// Find and replace all matches of each rule directly, without a separate search call.
// By default, we use the US-based API service. This is the primary endpoint for global use.
$apiUrl = 'https://api.pdfrest.com';

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
// $apiUrl = 'https://eu-api.pdfrest.com';

$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'; // Replace with your API key.
$client = new Client(['http_errors' => false]);

$replacements = [
    ['search' => ['type' => 'literal', 'value' => 'Draft'], 'replacement' => 'Final'],
    ['search' => ['type' => 'literal', 'value' => 'Pending'], 'replacement' => 'Ready'],
];

// Each rule uses one exact replacement for every match; omitted layout_mode uses reflow.
$response = $client->request('POST', $apiUrl . '/pdf-with-replaced-text', [
    'headers' => ['Api-Key' => $apiKey, 'Accept' => 'application/json'],
    'multipart' => [
        ['name' => 'file', 'contents' => Utils::tryFopen('/path/to/input.pdf', 'r'), 'filename' => 'input.pdf'],
        ['name' => 'replacements', 'contents' => json_encode($replacements)],
    ],
]);
if ($response->getStatusCode() >= 300) {
    fwrite(STDERR, (string) $response->getBody());
    exit(1);
}
$result = json_decode((string) $response->getBody(), true);
echo json_encode($result, JSON_PRETTY_PRINT) . PHP_EOL;
