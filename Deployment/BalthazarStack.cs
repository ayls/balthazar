using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HeyRed.Mime;
using Pulumi;
using Pulumi.Azure.ApiManagement;
using Pulumi.Azure.ApiManagement.Inputs;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.Core;
using Pulumi.Azure.Storage;
using Pulumi.Azure.Storage.Inputs;

class BalthazarStack : Stack
{
    [Output]
    public Output<string> WebEndpoint { get; set; }

    public BalthazarStack()
    {
        var config = new Config();
        var resourceGroup = CreateResourceGroup();
        var storageAccount = CreateStorageAccount(resourceGroup);
        var functionApp = DeployFunctionApp(resourceGroup, storageAccount);
        var apiManagement = DeployApiManagement(resourceGroup, config);
        var api = DeployApi(resourceGroup, apiManagement, functionApp, config);
        DeployWebApp(storageAccount, apiManagement, api, config);

        // Export the app's web address
        this.WebEndpoint = storageAccount.PrimaryWebEndpoint;
    }

    private static ResourceGroup CreateResourceGroup()
    {
        return new ResourceGroup("balthazar", new ResourceGroupArgs
        {
            Tags =
            {
                { "Project", Deployment.Instance.ProjectName },
                { "Environment", Deployment.Instance.StackName },
            }
        });
    }

    private static Account CreateStorageAccount(ResourceGroup resourceGroup)
    {
        return new Account("balthazarstrg", new AccountArgs
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
    }

    private static FunctionApp DeployFunctionApp(ResourceGroup resourceGroup, Account storageAccount)
    {
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

        var functionAppDeploymentContainer = new Container("zips", new ContainerArgs
        {
            StorageAccountName = storageAccount.Name,
            ContainerAccessType = "private"
        });

        var functionAppDeploymentBlob = new Blob("functionAppZip", new BlobArgs
        {
            StorageAccountName = storageAccount.Name,
            StorageContainerName = functionAppDeploymentContainer.Name,
            Type = "Block",
            Source = new FileArchive("../API/bin/Debug/netcoreapp3.1/publish")
        });

        var functionAppDeploymentBlobUrl = SharedAccessSignature.SignedBlobReadUrl(functionAppDeploymentBlob, storageAccount);

        return new FunctionApp("balthazarapp", new FunctionAppArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AppServicePlanId = appServicePlan.Id,
            SiteConfig = new FunctionAppSiteConfigArgs()
            {
                Cors = new FunctionAppSiteConfigCorsArgs()
                {
                    //AllowedOrigins = new[] { storageAccount.PrimaryWebEndpoint.Apply(s => s.TrimEnd('/')) }
                    AllowedOrigins = new[] { "*" } // TODO: setup keys
                }                
            },
            AppSettings =
            {
                {"runtime", "dotnet"},
                {"WEBSITE_RUN_FROM_PACKAGE", functionAppDeploymentBlobUrl},
                {"BookmarkCollectionConnectionString", storageAccount.PrimaryConnectionString}
            },
            StorageAccountName = storageAccount.Name,
            StorageAccountAccessKey = storageAccount.PrimaryAccessKey,
            Version = "~3"
        });
    }

    private static Service DeployApiManagement(ResourceGroup resourceGroup, Config config)
    {
        var apiManagement = new Service("balthazarapm", new ServiceArgs()
        {
            ResourceGroupName = resourceGroup.Name,
            SkuName = "Developer_1",
            PublisherEmail = config.Require("apmPublisherEmail"),
            PublisherName = config.Require("apmPublisherName"),
            Identity = new ServiceIdentityArgs()
            {
                Type = "SystemAssigned"
            }
        });

        return apiManagement;
    }

    private static Api DeployApi(ResourceGroup resourceGroup, Service apiManagement, FunctionApp functionApp, Config config)
    {
        var api = new Api("balthazarappapi", new ApiArgs()
        {
            ResourceGroupName = resourceGroup.Name,
            ApiManagementName = apiManagement.Name,
            DisplayName = "Balthazar API",
            Path = "balthazar",
            Protocols = new InputList<string> { "https" },
            ServiceUrl = Output.Format($"https://{functionApp.DefaultHostname}"),
            Import = new ApiImportArgs
            {
                ContentFormat = "openapi-link",
                ContentValue = Output.Format($"https://{functionApp.DefaultHostname}/api/Swagger"),
            },
            Revision = "1"
        });

        new ApiPolicy("balthazarappapipolicy", new ApiPolicyArgs()
        {
            ResourceGroupName = resourceGroup.Name,
            ApiManagementName = apiManagement.Name,
            ApiName = api.Name,
            XmlContent = GetPolicy(config)
        });

        return api;
    }

    private static string GetPolicy(Config config)
    {
        var policyBuilder = new StringBuilder();
        policyBuilder.AppendLine("<policies>");
        policyBuilder.AppendLine("  <inbound>");
        policyBuilder.AppendLine("    <base />");
        policyBuilder.AppendLine($"    <validate-jwt header-name=\"Authorization\" failed-validation-httpcode=\"401\" require-expiration-time=\"true\" require-scheme=\"Bearer\" require-signed-tokens=\"true\">");
        policyBuilder.AppendLine($"      <openid-config url=\"{config.Require("authOpenIdConfigUrl")}\" />");
        policyBuilder.AppendLine("      <audiences>");
        policyBuilder.AppendLine($"        <audience>{config.Require("authAudience")}</audience>");
        policyBuilder.AppendLine("      </audiences>");
        policyBuilder.AppendLine("    </validate-jwt>");
        policyBuilder.AppendLine("    <cors>");
        policyBuilder.AppendLine("      <allowed-origins>");
        policyBuilder.AppendLine("        <origin>*</origin>"); // TODO: set to frontend url
        policyBuilder.AppendLine("      </allowed-origins>");
        policyBuilder.AppendLine("      <allowed-methods>");
        policyBuilder.AppendLine("        <method>GET</method>");
        policyBuilder.AppendLine("        <method>POST</method>");
        policyBuilder.AppendLine("        <method>PUT</method>");
        policyBuilder.AppendLine("        <method>DELETE</method>");
        policyBuilder.AppendLine("      </allowed-methods>");
        policyBuilder.AppendLine("      <allowed-headers>");
        policyBuilder.AppendLine("        <header>*</header>");
        policyBuilder.AppendLine("      </allowed-headers>");
        policyBuilder.AppendLine("    </cors>");
        policyBuilder.AppendLine("  </inbound>");
        policyBuilder.AppendLine("</policies>");

        return policyBuilder.ToString();
    }

    private static void DeployWebApp(Account storageAccount, Service apiManagement, Api api, Config config)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var rootDirectory = Directory.GetParent(currentDirectory).FullName;
        var webDistDirectory = Path.Combine(rootDirectory, "Web", "dist");
        var files = EnumerateWebFiles(webDistDirectory);
        foreach (var file in files)
        {
            new Blob(file.name, new BlobArgs
            {
                Name = file.name,
                StorageAccountName = storageAccount.Name,
                StorageContainerName = "$web",
                Type = "Block",
                Source = new FileAsset(file.info.FullName),
                ContentType = MimeTypesMap.GetMimeType(file.info.Extension)
            });
        }

        // create the config file
        new Blob("config.js", new BlobArgs
        {
            Name = "config.js",
            StorageAccountName = storageAccount.Name,
            StorageContainerName = "$web",
            Type = "Block",
            SourceContent = Output.Format($"window.config = {{ apiBase: \"{apiManagement.GatewayUrl}/{api.Path}/api\", authorization_endpoint: \"{config.Require("authEndpoint")}\", authorization_client_id: \"{config.Require("authAudience")}\" }}"),
            ContentType = "text/javascript"
        });
    }

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
