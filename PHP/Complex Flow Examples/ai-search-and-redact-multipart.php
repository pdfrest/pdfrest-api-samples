<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Utils;

// Use AI to find potential PII, including unknown names and addresses, then redact retained matches. Review AI findings before applying.
// By default, we use the US-based API service. This is the primary endpoint for global use.
$apiUrl = 'https://api.pdfrest.com';

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
// $apiUrl = 'https://eu-api.pdfrest.com';

$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'; // Replace with your API key.
$client = new Client(['http_errors' => false]);

$searches = [
    ['type' => 'ai_preset', 'value' => 'potential_pii'],
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
// AI Search selects content by meaning; ordinary Search needs literal text, a regex, or a pattern preset.
// Review the returned text and confidence; remove unwanted matches before the action.
// A program can filter $result['matches'] here before submitting the retained matches.
if (count($result['matches']) === 0) {
    exit("No matches found; no PDF was modified.\n");
}
// Reuse the same unchanged PDF that was searched.
$response = $client->request('POST', $apiUrl . '/pdf-with-redacted-text-preview', [
    'headers' => ['Api-Key' => $apiKey, 'Accept' => 'application/json'],
    'multipart' => [
        ['name' => 'id', 'contents' => $result['inputId']],
        ['name' => 'selections', 'contents' => json_encode($result)],
    ],
]);
if ($response->getStatusCode() >= 300) {
    fwrite(STDERR, (string) $response->getBody());
    exit(1);
}
$result = json_decode((string) $response->getBody(), true);
echo json_encode($result, JSON_PRETTY_PRINT) . PHP_EOL;
// Preview annotations do not remove content. Inspect the preview before applying.
// This example immediately applies it to demonstrate the complete workflow.
$response = $client->request('POST', $apiUrl . '/pdf-with-redacted-text-applied', [
    'headers' => ['Api-Key' => $apiKey, 'Accept' => 'application/json'],
    'multipart' => [
        ['name' => 'id', 'contents' => $result['outputId']],

    ],
]);
if ($response->getStatusCode() >= 300) {
    fwrite(STDERR, (string) $response->getBody());
    exit(1);
}
$result = json_decode((string) $response->getBody(), true);
echo json_encode($result, JSON_PRETTY_PRINT) . PHP_EOL;
