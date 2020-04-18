namespace API

open DataAccess
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http;
open Microsoft.AspNetCore.Http;
open Microsoft.Extensions.Logging;
open System.IO

module Import =  
    [<FunctionName("Import")>]
    let Run ([<HttpTrigger(AuthorizationLevel.Anonymous, [|"post"|])>] req: HttpRequest) (log: ILogger) = 
        async {
            let! body = 
                new StreamReader(req.Body) 
                |> (fun stream -> stream.ReadToEndAsync()) 
                |> Async.AwaitTask
            BookmarkImport.import(body, Config.getConnectionString, Config.getPartitionKey)
        }
        |> Async.StartAsTask