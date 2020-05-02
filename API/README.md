# balthazar

## Build Setup

1. Create an Azure Storage account
2. Configure setting in local.setting.json:
```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "BookmarkCollectionConnectionString": "<connection string to Azure Storage account>",
    "BookmarkCollectionPartitionKey": "<partition key value here, a guid or similar>"
  },
  "Host": {
    "CORS": "*"
  }
}
```
3. Run the API

## Troubleshooting

If the API won't connect to the storage account (happens because the local.settings.json configuration is not picked up) rebuild the API and try again. 



