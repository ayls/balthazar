namespace DataAccess

open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table

module BookmarkStorage =
    type BookmarkRecord(bookmarkCollectionId: string, recordId: string, parentRowKey: string, name: string, url: string, isFolder: bool) =
        inherit TableEntity(partitionKey=bookmarkCollectionId, rowKey=recordId)
        new() = BookmarkRecord(null, null, null, null, null, false)
        member val ParentRowKey = parentRowKey with get, set
        member val Name = name with get, set
        member val Url = url with get, set
        member val IsFolder = isFolder with get, set

    let openTable (connectionString: string) = 
        let storageAccount = CloudStorageAccount.Parse(connectionString)
        let tableClient = storageAccount.CreateCloudTableClient()
        let table = tableClient.GetTableReference("Bookmarks")        
        table.CreateIfNotExistsAsync() 
        |> Async.AwaitTask 
        |> Async.RunSynchronously 
        |> ignore
        table

    let insert (table:CloudTable) (bookmarkRecord:BookmarkRecord) =
        let insertOp = TableOperation.Insert(bookmarkRecord)        
        table.ExecuteAsync(insertOp)
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> ignore

    let update (table:CloudTable) (bookmarkRecord:BookmarkRecord) =
        let replaceOp = TableOperation.Replace(bookmarkRecord)      
        table.ExecuteAsync(replaceOp)
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> ignore

    let delete (table:CloudTable) (bookmarkRecord:BookmarkRecord) =
        let deleteOp = TableOperation.Delete(bookmarkRecord)        
        table.ExecuteAsync(deleteOp)
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> ignore

    let list (table:CloudTable) (bookmarkCollectionId: string) =
        let bookmarks = new System.Collections.Generic.List<BookmarkRecord>();
        let query =
            TableQuery<BookmarkRecord>().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey", QueryComparisons.Equal, bookmarkCollectionId))
        let asyncQuery = 
            let rec loop (cont: TableContinuationToken) = async {
                let! result = table.ExecuteQuerySegmentedAsync(query, cont) |> Async.AwaitTask
                bookmarks.AddRange(result)
                match result.ContinuationToken with
                | null -> ()
                | cont -> return! loop cont
            }
            loop null
        asyncQuery |> Async.RunSynchronously
        bookmarks
