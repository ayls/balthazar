namespace API.DataAccess

open System.Linq
open BookmarksManager

module BookmarkImport =
    let private readExport(content: string) =
        let reader = new NetscapeBookmarksReader()
        reader.Read(content)

    let rec private importExport(table:BookmarkStorage.BookmarkTable, bookmarkCollectionId: string, bookmarkItem:IBookmarkItem, parentBookmarkItem:IBookmarkFolder, order: int) =
        let recordId = bookmarkItem.GetHashCode().ToString()
        let parentRecordId = 
            match parentBookmarkItem with
            | null -> ""
            | _ -> parentBookmarkItem.GetHashCode().ToString()
        match bookmarkItem with
        | :? IBookmarkFolder as folder -> table.insert(bookmarkCollectionId, recordId, parentRecordId, order, folder.Title, "", true) |> ignore
        | :? IBookmarkLink as link -> table.insert(bookmarkCollectionId, recordId, parentRecordId, order, link.Title, link.Url, false) |> ignore
        | _ -> ()
        let mutable order = 0
        let importExportCall(table:BookmarkStorage.BookmarkTable, bookmarkCollectionId: string, item:IBookmarkItem, folder:IBookmarkFolder) =
            importExport(table, bookmarkCollectionId, item, folder, order)
            order <- order + 1
        match bookmarkItem with
        | :? IBookmarkFolder as folder -> for item in folder do importExportCall(table, bookmarkCollectionId, item, folder)
        | _ -> ()

    let import(content: string, connectionString: string, bookmarkCollectionId: string) = 
        let table = new BookmarkStorage.BookmarkTable(connectionString)
        let bookmarkExport = readExport(content)
        let rootBookmarkFolder = bookmarkExport.First()
        importExport(table, bookmarkCollectionId, rootBookmarkFolder, null, 0)

