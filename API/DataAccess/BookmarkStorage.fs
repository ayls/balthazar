namespace API.DataAccess

open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open System

module BookmarkStorage =
    type BookmarkRecord(bookmarkCollectionId: string, rowKey: string, parentRowKey: string, order: int, name: string, url: string, isFolder: bool) =
        inherit TableEntity(partitionKey=bookmarkCollectionId, rowKey=rowKey)
        new() = BookmarkRecord(null, null, null, 0, null, null, false)
        member val ParentRowKey = parentRowKey with get, set
        member val Order = order with get, set
        member val Name = name with get, set
        member val Url = url with get, set
        member val IsFolder = isFolder with get, set
        static member initialize(bookmarkCollectionId: string, parentRowKey: string, order: int, name: string, url: string, isFolder: bool) = 
            let rowKey = Guid.NewGuid().ToString()
            BookmarkRecord.initialize(bookmarkCollectionId, rowKey, parentRowKey, order, name, url, isFolder)
        static member initialize(bookmarkCollectionId: string, rowKey: string, parentRowKey: string, order: int, name: string, url: string, isFolder: bool) = 
            new BookmarkRecord(bookmarkCollectionId, rowKey, parentRowKey, order, name, url, isFolder)

    type BookmarkTable(connectionString: string) =
        member val private connectionString = connectionString
        member val private table: CloudTable = null with get, set
        member private this.openTable() = 
            match this.table with
            | null -> 
                CloudStorageAccount.Parse(this.connectionString)
                |> fun storageAccount -> storageAccount.CreateCloudTableClient()
                |> fun tableClient -> this.table <- tableClient.GetTableReference("Bookmarks")
                |> fun _ -> this.table.CreateIfNotExistsAsync() 
                |> Async.AwaitTask 
                |> Async.RunSynchronously
                |> ignore
            | _ -> ()
        member this.insert(bookmarkCollectionId: string, parentRowKey: string, order: int, name: string, url: string, isFolder: bool) =
            this.openTable()
            |> fun _ -> BookmarkRecord.initialize(bookmarkCollectionId, parentRowKey, order, name, url, isFolder)
            |> fun bookmarkRecord -> TableOperation.Insert(bookmarkRecord)
            |> this.table.ExecuteAsync
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> fun tableResult -> tableResult.Result
        member this.insert(bookmarkCollectionId: string, rowKey: string, parentRowKey: string, order: int, name: string, url: string, isFolder: bool) =
            this.openTable()
            |> fun _ -> BookmarkRecord.initialize(bookmarkCollectionId, rowKey, parentRowKey, order, name, url, isFolder)
            |> fun bookmarkRecord -> TableOperation.Insert(bookmarkRecord)
            |> this.table.ExecuteAsync
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> fun tableResult -> tableResult.Result
        member this.update(bookmarkRecord:BookmarkRecord) =
            this.openTable()
            |> fun _ -> TableOperation.Replace(bookmarkRecord)        
            |> this.table.ExecuteAsync
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> fun tableResult -> tableResult.Result
        member this.delete(bookmarkRecord:BookmarkRecord) =
            this.openTable()
            |> fun _ -> TableOperation.Delete(bookmarkRecord)        
            |> this.table.ExecuteAsync
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
                    let! result = this.table.ExecuteQuerySegmentedAsync(query, cont) |> Async.AwaitTask
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
