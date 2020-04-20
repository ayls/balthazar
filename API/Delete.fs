namespace API

open API.DataAccess
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http;
open Microsoft.AspNetCore.Http;
open Microsoft.Extensions.Logging;
open System.IO
open Newtonsoft.Json

module Delete =  
    [<FunctionName("Delete")>]
    let Run ([<HttpTrigger(AuthorizationLevel.Anonymous, [|"delete"|])>] req: HttpRequest) (log: ILogger) = 
        async {
            let! body = 
                new StreamReader(req.Body) 
                |> (fun stream -> stream.ReadToEndAsync()) 
                |> Async.AwaitTask
            let bookmarkRecord = JsonConvert.DeserializeObject<BookmarkStorage.BookmarkRecord>(body)
            let table = new BookmarkStorage.BookmarkTable(Config.getConnectionString)
            table.delete(bookmarkRecord)
        }
        |> Async.StartAsTask