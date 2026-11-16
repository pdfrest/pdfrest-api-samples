<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Utils;

// Use AI to find confidentiality obligations by meaning, then highlight them. No exact wording or regex is supplied.
// AI Search PDF is available on the US Cloud service only.
$apiUrl = 'https://api.pdfrest.com';

// AI Search PDF is unavailable on the EU service and in self-hosted deployments.

$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'; // Replace with your API key.
$client = new Client(['http_errors' => false]);

$searches = [
    ['type' => 'prompt', 'value' => 'Find statements that require a party to keep information private or not disclose it to others, even when they do not use the word confidential.'],
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
$response = $client->request('POST', $apiUrl . '/pdf-with-highlighted-text', [
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
