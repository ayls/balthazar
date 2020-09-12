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
        var apiManagement = DeployApiManagement(resourceGroup, config);
        var functionApp = DeployFunctionApp(resourceGroup, storageAccount, apiManagement);
        var api = DeployApi(resourceGroup, apiManagement, functionApp, storageAccount, config);
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

    private static FunctionApp DeployFunctionApp(ResourceGroup resourceGroup, Account storageAccount, Service apiManagement)
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
                    AllowedOrigins = new[] { "*" }
                },
                IpRestrictions = new InputList<FunctionAppSiteConfigIpRestrictionArgs>()
                {
                    new FunctionAppSiteConfigIpRestrictionArgs()
                    {
                        Name = "Allow Api Management",
                        Priority = 100,
                        Action = "Allow",
                        IpAddress = Output.Format($"{apiManagement.PublicIpAddresses.First()}/32")
                    },
                    new FunctionAppSiteConfigIpRestrictionArgs()
                    {
                        Name = "Deny All",
                        Priority = 200,
                        Action = "Deny",
                        IpAddress = "0.0.0.0/0"
                    }
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

    private static Api DeployApi(ResourceGroup resourceGroup, Service apiManagement, FunctionApp functionApp, Account storageAccount, Config config)
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
            XmlContent = Output.Format(
                $@"<policies>
                     <inbound>
                       <base />
                       <validate-jwt header-name=""Authorization"" failed-validation-httpcode=""401"" require-expiration-time=""true"" require-scheme=""Bearer"" require-signed-tokens=""true"">
                         <openid-config url=""{config.Require("authOpenIdConfigUrl")}"" />
                         <audiences>
                           <audience>{config.Require("authAudience")}</audience>
                         </audiences>
                       </validate-jwt>
                       <cors>
                         <allowed-origins>
                           <origin>{storageAccount.PrimaryWebEndpoint.Apply(s => config.Get("domainUrl") ?? s.TrimEnd('/'))}</origin>
                         </allowed-origins>
                         <allowed-methods>
                           <method>GET</method>
                           <method>POST</method>
                           <method>PUT</method>
                           <method>DELETE</method>
                         </allowed-methods>
                         <allowed-headers>
                           <header>*</header>
                         </allowed-headers>
                       </cors>
                     </inbound>
                   </policies>")
        });

        return api;
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
