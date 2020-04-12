namespace DataAccess

open System.IO
open System.Linq
open BookmarksManager
open BookmarkStorage

module BookmarkImport =
    let private readExport(stream: Stream) =
        let reader = new NetscapeBookmarksReader()
        reader.Read(stream)

    let rec private importExport(table:BookmarkTable, bookmarkCollectionId: string, bookmarkItem:IBookmarkItem, parentBookmarkItem:IBookmarkFolder) =
        let recordId = bookmarkItem.GetHashCode().ToString()
        let parentRecordId = 
            match parentBookmarkItem with
            | null -> ""
            | _ -> parentBookmarkItem.GetHashCode().ToString()
        match bookmarkItem with
        | :? IBookmarkFolder as folder -> table.insert(bookmarkCollectionId, recordId, parentRecordId, folder.Title, "", true) |> ignore
        | :? IBookmarkLink as link -> table.insert(bookmarkCollectionId, recordId, parentRecordId, link.Title, link.Url, false) |> ignore
        | _ -> ()
        match bookmarkItem with
        | :? IBookmarkFolder as folder -> for item in folder do importExport(table, bookmarkCollectionId, item, folder)
        | _ -> ()

    let import(stream: Stream, connectionString: string, bookmarkCollectionId: string) = 
        let table = new BookmarkTable(connectionString)
        let bookmarkExport = readExport(stream)
        let rootBookmarkFolder = bookmarkExport.First()
        importExport(table, bookmarkCollectionId, rootBookmarkFolder, null)

