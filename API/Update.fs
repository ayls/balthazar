namespace API

open API.DataAccess
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http;
open Microsoft.AspNetCore.Http;
open Microsoft.Extensions.Logging;
open System.IO
open Newtonsoft.Json

module Update =  
    [<FunctionName("Update")>]
    let Run ([<HttpTrigger(AuthorizationLevel.Anonymous, [|"put"|])>] req: HttpRequest) (log: ILogger) = 
        async {
            let! body = 
                new StreamReader(req.Body) 
                |> (fun stream -> stream.ReadToEndAsync()) 
                |> Async.AwaitTask
            let bookmarkRecord = JsonConvert.DeserializeObject<BookmarkStorage.BookmarkRecord>(body)
            let table = new BookmarkStorage.BookmarkTable(Config.getConnectionString)
            return table.update(bookmarkRecord)
        }
        |> Async.StartAsTask