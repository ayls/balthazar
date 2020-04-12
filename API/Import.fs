namespace API

open DataAccess
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http;
open Microsoft.AspNetCore.Http;
open Microsoft.Extensions.Logging;

module Import =  
    [<FunctionName("Import")>]
    let Run ([<HttpTrigger(AuthorizationLevel.Anonymous, [|"post"|])>] req: HttpRequest) (log: ILogger) = 
        async {
            BookmarkImport.import(req.Body, Config.getConnectionString, Config.getPartitionKey)
        }
        |> Async.StartAsTask