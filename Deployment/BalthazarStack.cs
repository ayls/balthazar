using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HeyRed.Mime;
using Pulumi;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.Core;
using Pulumi.Azure.Storage;
using Pulumi.Azure.Storage.Inputs;

class BalthazarStack : Pulumi.Stack
{
    public BalthazarStack()
    {
        // Create resource group
        var resourceGroup = new ResourceGroup("balthazar", new ResourceGroupArgs
        {
            Tags =
            {
                { "Project", Pulumi.Deployment.Instance.ProjectName },
                { "Environment", Pulumi.Deployment.Instance.StackName },
            }
        });

        // Create storage account
        var storageAccount = new Account("balthazarstrg", new AccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountReplicationType = "LRS",
            AccountTier = "Standard",
            AccountKind = "StorageV2",
            StaticWebsite = new AccountStaticWebsiteArgs
            {
                IndexDocument = "index.html"
            },
            EnableHttpsTrafficOnly = true
        });

        // Create consumption plan
        var appServicePlan = new Plan("balthazarappsvc", new PlanArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Kind = "FunctionApp",
            Sku = new PlanSkuArgs
            {
                Tier = "Dynamic",
                Size = "Y1",
            }
        });

        // Create a container for function app deployment blobs
        var container = new Container("zips", new ContainerArgs
        {
            StorageAccountName = storageAccount.Name,
            ContainerAccessType = "private"
        });

        // Create function app deployment blob
        var blob = new Blob("functionAppZip", new BlobArgs
        {
            StorageAccountName = storageAccount.Name,
            StorageContainerName = container.Name,
            Type = "Block",
            Source = new FileArchive("../API/bin/Debug/netcoreapp3.1/publish")
        });

        // deploy functions app
        var codeBlobUrl = SharedAccessSignature.SignedBlobReadUrl(blob, storageAccount);
        var app = new FunctionApp("balthazarapp", new FunctionAppArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AppServicePlanId = appServicePlan.Id,
            SiteConfig = new FunctionAppSiteConfigArgs()
            {
                Cors = new FunctionAppSiteConfigCorsArgs()
                {
                    AllowedOrigins = new[] { storageAccount.PrimaryWebEndpoint.Apply(s => s.TrimEnd('/')) }
                }
            },
            AppSettings =
            {
                {"runtime", "dotnet"},
                {"WEBSITE_RUN_FROM_PACKAGE", codeBlobUrl},
                {"BookmarkCollectionConnectionString", storageAccount.PrimaryConnectionString}
            },
            StorageAccountName = storageAccount.Name,
            StorageAccountAccessKey = storageAccount.PrimaryAccessKey,
            Version = "~3"
        });

        // Upload web files
        var currentDirectory = Directory.GetCurrentDirectory();
        var rootDirectory = Directory.GetParent(currentDirectory).FullName;
        var webDistDirectory = Path.Combine(rootDirectory, "Web", "dist");
        var files = EnumerateWebFiles(webDistDirectory);
        foreach (var file in files)
        {
            var uploadedFile = new Blob(file.name, new BlobArgs
            {
                Name = file.name,
                StorageAccountName = storageAccount.Name,
                StorageContainerName = "$web",
                Type = "Block",
                Source = new FileAsset(file.info.FullName),
                ContentType = MimeTypesMap.GetMimeType(file.info.Extension)
            });
        }
        // create the web config
        var configFile = new Blob("config.js", new BlobArgs
        {
            Name = "config.js",
            StorageAccountName = storageAccount.Name,
            StorageContainerName = "$web",
            Type = "Block",
            SourceContent = Output.Format($"window.config = {{ apiBase: \"https://{app.DefaultHostname}/api\" }}"),
            ContentType = "text/javascript"
        });

        // Export the app's web address
        this.WebEndpoint = storageAccount.PrimaryWebEndpoint;
    }

    [Output]
    public Output<string> WebEndpoint { get; set; }

    private static IEnumerable<(FileInfo info, string name)> EnumerateWebFiles(string sourceFolder)
    {
        var ignoredFiles = new[] { ".nojekyll", "config.js" };
        var sourceFolderLength = sourceFolder.Length + 1;
        return Directory.EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
            .Select(path => (
                info: new FileInfo(path),
                name: path.Remove(0, sourceFolderLength).Replace(Path.DirectorySeparatorChar, '/')            
            ))
            .Where(file => !ignoredFiles.Contains(file.name));
    }
}
