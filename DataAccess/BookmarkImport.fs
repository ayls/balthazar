namespace DataAccess

open System
open System.IO
open System.Linq
open BookmarksManager
open BookmarkStorage
open Microsoft.WindowsAzure.Storage.Table

module BookmarkImport =
    let private readExport path =
        use file = File.OpenRead(path)
        let reader = new NetscapeBookmarksReader()
        reader.Read(file)

    let rec importExport (table:CloudTable) (bookmarkCollectionId: string) (bookmarkItem:IBookmarkItem) (parentBookmarkItem:IBookmarkFolder) =
        let recordId = bookmarkItem.GetHashCode().ToString()
        let parentRecordId = 
            match parentBookmarkItem with
            | null -> ""
            | _ -> parentBookmarkItem.GetHashCode().ToString()
        match bookmarkItem with
        | :? IBookmarkFolder as folder -> new BookmarkRecord(bookmarkCollectionId, recordId, parentRecordId, folder.Title, "", true) |> BookmarkStorage.insert table
        | :? IBookmarkLink as link -> new BookmarkRecord(bookmarkCollectionId, recordId, parentRecordId, link.Title, link.Url, false) |> BookmarkStorage.insert table          
        | _ -> ()
        match bookmarkItem with
        | :? IBookmarkFolder as folder -> for item in folder do importExport table bookmarkCollectionId item folder
        | _ -> ()

    let import path connectionString = 
        let table = openTable connectionString
        let bookmarkCollectionId = Guid.NewGuid().ToString()
        let bookmarkExport = readExport path
        let rootBookmarkFolder = bookmarkExport.First()
        importExport table bookmarkCollectionId rootBookmarkFolder null

