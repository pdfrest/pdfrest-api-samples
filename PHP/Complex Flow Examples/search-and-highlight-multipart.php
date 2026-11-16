<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Utils;

// Search PDF first, optionally remove unwanted matches, then highlight the retained matches.
// By default, we use the US-based API service. This is the primary endpoint for global use.
$apiUrl = 'https://api.pdfrest.com';

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
// $apiUrl = 'https://eu-api.pdfrest.com';

$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'; // Replace with your API key.
$client = new Client(['http_errors' => false]);

$searches = [
    ['type' => 'literal', 'value' => 'Confidential'],
];

$response = $client->request('POST', $apiUrl . '/search-results', [
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
