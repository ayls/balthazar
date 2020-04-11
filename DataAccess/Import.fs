namespace DataAccess

open System
open System.IO
open System.Linq
open BookmarksManager
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table

module DataAccess =

    type BookmarkRecord(bookmarkCollectionId: string, recordId: string, parentRowKey: string, name: string, url: string, isFolder: bool) =
        inherit TableEntity(partitionKey=bookmarkCollectionId, rowKey=recordId)
        new() = BookmarkRecord(null, null, null, null, null, false)
        member val ParentRowKey = parentRowKey with get, set
        member val Name = name with get, set
        member val Url = url with get, set
        member val IsFolder = isFolder with get, set

    let private openTable connectionString = 
        let storageAccount = CloudStorageAccount.Parse(connectionString)
        let tableClient = storageAccount.CreateCloudTableClient()
        let table = tableClient.GetTableReference("Bookmarks")        
        table.CreateIfNotExistsAsync() 
        |> Async.AwaitTask 
        |> Async.RunSynchronously 
        |> ignore
        table

    let private insertBookmark (table:CloudTable) (bookmarkRecord:BookmarkRecord) =
        let insertOp = TableOperation.Insert(bookmarkRecord)        
        table.ExecuteAsync(insertOp)
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> ignore

    let private readBookmarkExport path =
        use file = File.OpenRead(path)
        let reader = new NetscapeBookmarksReader()
        reader.Read(file)

    let rec importBookmarkExport (table:CloudTable) (bookmarkCollectionId: string) (bookmarkItem:IBookmarkItem) (parentBookmarkItem:IBookmarkFolder) =
        let recordId = bookmarkItem.GetHashCode().ToString()
        let parentRecordId = 
            match parentBookmarkItem with
            | null -> ""
            | _ -> parentBookmarkItem.GetHashCode().ToString()
        match bookmarkItem with
        | :? IBookmarkFolder as folder -> new BookmarkRecord(bookmarkCollectionId, recordId, parentRecordId, folder.Title, "", true) |> insertBookmark table
        | :? IBookmarkLink as link -> new BookmarkRecord(bookmarkCollectionId, recordId, parentRecordId, link.Title, link.Url, false) |> insertBookmark table          
        | _ -> ()
        match bookmarkItem with
        | :? IBookmarkFolder as folder -> for item in folder do importBookmarkExport table bookmarkCollectionId item folder
        | _ -> ()

    let import path connectionString = 
        let table = openTable connectionString
        let bookmarkCollectionId = Guid.NewGuid().ToString()
        let bookmarkExport = readBookmarkExport path
        let rootBookmarkFolder = bookmarkExport.First()
        importBookmarkExport table bookmarkCollectionId rootBookmarkFolder null

