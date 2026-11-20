In this directory you will find sample calls to single endpoints, as well
as more advanced workflows presented as cURL scripts.

## Prerequisites

- Install [cURL](https://curl.se/) to send API requests.
- Install [jq](https://jqlang.org/) to parse and construct JSON in the search and modify samples.
- Ensure `curl` and `jq` are available on your `PATH`. Check with `curl --version` and `jq --version`.

Optional deletion of sensitive files: many samples include an optional step 
to delete uploaded or generated files from pdfRest servers. This step is off 
by default. To enable it, open the script and uncomment the local toggle line:

`# DELETE_SENSITIVE_FILES=true` → `DELETE_SENSITIVE_FILES=true`

Leave it commented (or set to anything other than `true`) to keep deletion off.
