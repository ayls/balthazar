# balthazar

## Pulumi Installation

Note: if you are using Windows I recommend installing all of the tools below in WSL. Windows version of Pulumi suffers from some glitches when uploading files to Azure Storage.

Install Pulumi by following the instructions [here](https://www.pulumi.com/docs/get-started/azure/install-pulumi/).

Make sure you have .NET Core 3.0 SDK or later installed, if not download it from [here](https://dotnet.microsoft.com/download) and install.

## Pulumi Azure Configuration

First make sure you have Azure CLI installed. Installation instructions are [here](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest).

Once ready, execute (from this folder):
```
pulumi stack init dev
az login
dotnet publish ../API/API.fsproj
npm run build --prefix ../Web
pulumi config set azure:location <location>
pulumi up
```

