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

    type BookmarkTable(connectionString: string) =
        member val private ConnectionString = connectionString
        member val private Table: CloudTable = null with get, set
        member private this.openTable() = 
            match this.Table with
            | null -> 
                let storageAccount = CloudStorageAccount.Parse(this.ConnectionString)
                let tableClient = storageAccount.CreateCloudTableClient()
                this.Table <- tableClient.GetTableReference("Bookmarks")        
                this.Table.CreateIfNotExistsAsync() 
                |> Async.AwaitTask 
                |> Async.RunSynchronously 
                |> ignore
            | _ -> ()
        member this.insert(bookmarkRecord:BookmarkRecord) =
            let insertOp = TableOperation.Insert(bookmarkRecord)        
            this.openTable()
            this.Table.ExecuteAsync(insertOp)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        member this.update(bookmarkRecord:BookmarkRecord) =
            let replaceOp = TableOperation.Replace(bookmarkRecord)      
            this.openTable()
            this.Table.ExecuteAsync(replaceOp)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        member this.delete(bookmarkRecord:BookmarkRecord) =
            let deleteOp = TableOperation.Delete(bookmarkRecord)        
            this.openTable()
            this.Table.ExecuteAsync(deleteOp)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        member this.list(bookmarkCollectionId: string) =
            let query =
                TableQuery<BookmarkRecord>().Where(
                    TableQuery.GenerateFilterCondition(
                        "PartitionKey", QueryComparisons.Equal, bookmarkCollectionId))
            this.openTable()
            let asyncQuery = 
                let rec loop(cont: TableContinuationToken) = async {
                    let! result = this.Table.ExecuteQuerySegmentedAsync(query, cont) |> Async.AwaitTask
                    let bookmarks = new System.Collections.Generic.List<BookmarkRecord>(result)
                    match result.ContinuationToken with
                    | null -> return bookmarks
                    | cont -> 
                        let! nextResult = loop(cont)
                        bookmarks.AddRange(nextResult)
                        return bookmarks
                }
                loop(null)
            asyncQuery |> Async.RunSynchronously
