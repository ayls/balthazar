namespace DataAccess

open System
open System.IO
open System.Linq
open BookmarksManager
open BookmarkStorage

module BookmarkImport =
    let private readExport(path: string) =
        use file = File.OpenRead(path)
        let reader = new NetscapeBookmarksReader()
        reader.Read(file)

    let rec private importExport(table:BookmarkTable, bookmarkCollectionId: string, bookmarkItem:IBookmarkItem, parentBookmarkItem:IBookmarkFolder) =
        let recordId = bookmarkItem.GetHashCode().ToString()
        let parentRecordId = 
            match parentBookmarkItem with
            | null -> ""
            | _ -> parentBookmarkItem.GetHashCode().ToString()
        match bookmarkItem with
        | :? IBookmarkFolder as folder -> new BookmarkRecord(bookmarkCollectionId, recordId, parentRecordId, folder.Title, "", true) |> table.insert
        | :? IBookmarkLink as link -> new BookmarkRecord(bookmarkCollectionId, recordId, parentRecordId, link.Title, link.Url, false) |> table.insert
        | _ -> ()
        match bookmarkItem with
        | :? IBookmarkFolder as folder -> for item in folder do importExport(table, bookmarkCollectionId, item, folder)
        | _ -> ()

    let import(path: string, connectionString: string) = 
        let table = new BookmarkTable(connectionString)
        let bookmarkCollectionId = Guid.NewGuid().ToString()
        let bookmarkExport = readExport path
        let rootBookmarkFolder = bookmarkExport.First()
        importExport(table, bookmarkCollectionId, rootBookmarkFolder, null)

