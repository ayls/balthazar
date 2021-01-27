# balthazar

## Build Setup

1. Create an Azure Storage account
2. Configure settings in local.setting.json:
```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "BookmarkCollectionConnectionString": "<connection string to Azure Storage account>"
  },
  "Host": {
    "CORS": "*"
  }
}
```
3. Run the API

## Troubleshooting

If the API won't connect to the storage account (happens because the local.settings.json configuration is not copied to bin folder) rebuild the API and try again. 



