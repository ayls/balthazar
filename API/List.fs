namespace API

open API.DataAccess
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http;
open Microsoft.AspNetCore.Http;
open Microsoft.Extensions.Logging;

module List =
    [<FunctionName("List")>]
    let Run ([<HttpTrigger(AuthorizationLevel.Anonymous, [|"get"|])>] req: HttpRequest) (log: ILogger) = 
        async {
            let table = new BookmarkStorage.BookmarkTable(Config.getConnectionString)
            return table.list(Config.getPartitionKey)
        }
        |> Async.StartAsTask