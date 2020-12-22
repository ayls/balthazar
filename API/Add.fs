namespace API

open API.DataAccess
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http;
open Microsoft.AspNetCore.Http;
open Microsoft.Extensions.Logging;
open System.IO
open Newtonsoft.Json

module Add =
    type AddRequest(parentRowKey: string, order: int, name: string, url: string, isFolder: bool) =
        new() = AddRequest(null, 0, null, null, false)
        member val ParentRowKey = parentRowKey with get, set
        member val Order = order with get, set
        member val Name = name with get, set
        member val Url = url with get, set
        member val IsFolder = isFolder with get, set
    
    [<FunctionName("Add")>]
    let Run ([<HttpTrigger(AuthorizationLevel.Admin, [|"post"|])>] req: HttpRequest) (log: ILogger) = 
        async {
            let! body = 
                new StreamReader(req.Body) 
                |> (fun stream -> stream.ReadToEndAsync()) 
                |> Async.AwaitTask
            let addRequest = JsonConvert.DeserializeObject<AddRequest>(body)
            let table = new BookmarkStorage.BookmarkTable(Config.getConnectionString)
            return table.insert(Config.getPartitionKey, addRequest.ParentRowKey, addRequest.Order, addRequest.Name, addRequest.Url, addRequest.IsFolder)
        }
        |> Async.StartAsTask