<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Request;

// Use AI to find people’s names without a list of names or regex, then replace retained names with Person.
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
// AI Search selects content by meaning; ordinary Search needs literal text, a regex, or a pattern preset.
// Review the returned text and confidence; remove unwanted matches before the action.
// A program can filter $result['matches'] here before submitting the retained matches.
if (count($result['matches']) === 0) {
    exit("No matches found; no PDF was modified.\n");
}
// Reuse the same unchanged PDF that was searched.
// Each retained name can have its own replacement. shrink_to_fit preserves surrounding text.
foreach ($result['matches'] as &$match) {
    $match['replacement'] = 'Person';
    $match['layout_mode'] = 'shrink_to_fit';
}
unset($match);
$response = $client->send(new Request('POST', $apiUrl . '/pdf-with-replaced-text', [
    'Api-Key' => $apiKey, 'Accept' => 'application/json', 'Content-Type' => 'application/json',
], json_encode([
    'id' => $result['inputId'],
    'selections' => $result,
])));
if ($response->getStatusCode() >= 300) {
    fwrite(STDERR, (string) $response->getBody());
    exit(1);
}
$result = json_decode((string) $response->getBody(), true);
echo json_encode($result, JSON_PRETTY_PRINT) . PHP_EOL;
