import _ from 'lodash'

export interface BookmarkState {
  bookmarks: BookmarkStateItem[]
}

export interface BookmarkStateItem {
  id: string,
  record: BookmarkRecord,
  isEditing: boolean,
  children: BookmarkStateItem[]
}

export interface BookmarkRecord {
  parentRowKey: string,
  order: number,
  name: string,
  url: string,
  isFolder: boolean,
  partitionKey: string,
  rowKey: string,
  timestamp: string,
  eTag: string
}

export const state = (): BookmarkState => ({
  bookmarks: [
    {
      id: "51613143",
      record: {
        parentRowKey: "",
        order: 0,
        name: "Storage",
        url: "",
        isFolder: true,
        partitionKey: "0fae070c-9f1d-40e3-bf4d-f3a342588960",
        rowKey: "51613143",
        timestamp: "2020-04-18T15:41:34.5700546+02:00",
        eTag: "W/\"datetime'2020-04-18T13%3A41%3A34.5700546Z'\""
      },
      isEditing: false,
      children: [{
        id: "10134182",
        record: {
          parentRowKey: "51613143",
          order: 0,
          name: "Managing the Azure Storage Lifecycle | Microsoft Docs",
          url: "https://docs.microsoft.com/en-us/azure/storage/blobs/storage-lifecycle-management-concepts",
          isFolder: false,
          partitionKey: "0fae070c-9f1d-40e3-bf4d-f3a342588960",
          rowKey: "10134182",
          timestamp: "2020-04-18T15:41:34.5880669+02:00",
          eTag: "W/\"datetime'2020-04-18T13%3A41%3A34.5880669Z'\""
        },
        isEditing: false,
        children: []
      }]
    },
    {
      id: "61027891",
      record: {
        parentRowKey: "",
        order: 0,
        name: "Microsoft",
        url: "",
        isFolder: true,
        partitionKey: "0fae070c-9f1d-40e3-bf4d-f3a342588960",
        rowKey: "61027891",
        timestamp: "2020-04-18T15:41:33.1480519+02:00",
        eTag: "W/\"datetime'2020-04-18T13%3A41%3A33.1480519Z'\""
      },
      isEditing: false,
      children: [{
        id: "11171509",
        record:   {
          parentRowKey: "61027891",
          order: 2,
          name: "Azure Devops",
          url: "",
          isFolder: true,
          partitionKey: "0fae070c-9f1d-40e3-bf4d-f3a342588960",
          rowKey: "11171509",
          timestamp: "2020-04-18T15:41:35.0794124+02:00",
          eTag: "W/\"datetime'2020-04-18T13%3A41%3A35.0794124Z'\""
        },
        isEditing: false,
        children: [{
          id: "64006473",
          record:     {
            parentRowKey: "11171509",
            order: 3,
            name: "azure-pipelines-tasks/README.md at master · microsoft/azure-pipelines-tasks · GitHub",
            url: "https://github.com/microsoft/azure-pipelines-tasks/blob/master/Tasks/AzureResourceGroupDeploymentV2/README.md",
            isFolder: false,
            partitionKey: "0fae070c-9f1d-40e3-bf4d-f3a342588960",
            rowKey: "64006473",
            timestamp: "2020-04-18T15:41:35.1564662+02:00",
            eTag: "W/\"datetime'2020-04-18T13%3A41%3A35.1564662Z'\""
          },
          isEditing: false,
          children: []
        },
        {
          id: "1958654",
          record:     {
            parentRowKey: "11171509",
            order: 4,
            name: "Powershell ConvertFrom-Json Encoding Special Characters Issue - Stack Overflow",
            url: "https://stackoverflow.com/questions/53033242/powershell-convertfrom-json-encoding-special-characters-issue",
            isFolder: false,
            partitionKey: "0fae070c-9f1d-40e3-bf4d-f3a342588960",
            rowKey: "1958654",
            timestamp: "2020-04-18T15:41:35.1744788+02:00",
            eTag: "W/\"datetime'2020-04-18T13%3A41%3A35.1744788Z'\""
          },
          isEditing: false,
          children: []
        }]
      }]
    }
  ]
})

export const mutations = {
  append(state: BookmarkState, { bookmark, isFolder }: { bookmark: BookmarkStateItem, isFolder: boolean }) {
    applyToAllBookmarks({ children: state.bookmarks } as BookmarkStateItem, (b: BookmarkStateItem) => {
      if (!b.id && b.isEditing) {
        removeBookmark(state.bookmarks, b);
      } else {
        b.isEditing = false;
      }
    });
    const parentRowKey = bookmark ? bookmark.id : '';
    const order = bookmark ? bookmark.children.length : state.bookmarks.length;   
    const newChild = {
      id: '',
      record: { 
        parentRowKey: parentRowKey,
        order: order,
        name: '', 
        url: '',
        isFolder: isFolder,
        partitionKey: '',
        rowKey: '',
        timestamp: '',
        eTag: ''
      },
      isEditing: true, 
      children: [] 
    } as BookmarkStateItem;
    if (bookmark) {
      bookmark.children.push(newChild);
    } else {
      state.bookmarks.push(newChild);
    }
  },
  setParent(state: BookmarkState, { bookmark, referenceBookmark, type }: { bookmark: BookmarkStateItem, referenceBookmark: BookmarkStateItem, type: string }) {
    removeBookmark(state.bookmarks, bookmark);
    const bookmarkCopy = _.cloneDeep(bookmark);
    if (type === 'inner') {      
      bookmarkCopy.record.parentRowKey = referenceBookmark.record.rowKey;
      referenceBookmark.children.push(bookmarkCopy);      
    } else {
      const root = { children: state.bookmarks } as BookmarkStateItem;
      const referenceBookmarkParent = findBookmarkParent(root, referenceBookmark);
      const referenceBookmarkIndex = referenceBookmarkParent!.children.indexOf(referenceBookmark);
      let insertIndex: number;
      switch (type) {
        case 'before':
          insertIndex = referenceBookmarkIndex;
          break;
        case 'after':
          insertIndex = Math.max(referenceBookmarkParent!.children.length - 1, referenceBookmarkIndex + 1);        
          break;     
        default:
          insertIndex = 0;
      }
      bookmarkCopy.record.parentRowKey = referenceBookmarkParent != root ? referenceBookmarkParent!.record.rowKey : '';
      referenceBookmarkParent!.children.splice(insertIndex, 0, bookmarkCopy);
    } 
  },  
  beginEdit(state: BookmarkState, { bookmark }: any) {
    applyToAllBookmarks({ children: state.bookmarks } as BookmarkStateItem, (b: BookmarkStateItem) => {
      if (!b.id && b.isEditing) {
        removeBookmark(state.bookmarks, b);
      } else {
        b.isEditing = false;
      }
    });
    bookmark.isEditing = true;
  },
  updateName(state: BookmarkState, { bookmark, text }: { bookmark: BookmarkStateItem, text: string }) {
    bookmark.record.name = text;
  },  
  updateUrl(state: BookmarkState, { bookmark, text }: { bookmark: BookmarkStateItem, text: string }) {
    bookmark.record.url = text;
  },    
  endEdit(state: BookmarkState, { bookmark, confirmed }: { bookmark: BookmarkStateItem, confirmed: boolean }) {
    if (!confirmed && !bookmark.id) {
      removeBookmark(state.bookmarks, bookmark);
    } else {
      bookmark.isEditing = false;
    }
  },
  remove(state: BookmarkState, { bookmark }: { bookmark: BookmarkStateItem }) {
    const bookmarkParent = findBookmarkParent({ children: state.bookmarks } as BookmarkStateItem, bookmark);
    removeBookmark(bookmarkParent!.children, bookmark);    
  }
}

function applyToAllBookmarks(bookmark: BookmarkStateItem, action: (b: BookmarkStateItem) => void): void {
  action(bookmark);
  for(const item of bookmark.children) {
    applyToAllBookmarks(item, action);
  }
}

function findBookmark(bookmarks: BookmarkStateItem[], bookmark: BookmarkStateItem): BookmarkStateItem | null {
  const foundBookmark = bookmarks.find((b: BookmarkStateItem) => bookmark === b);
  if (foundBookmark) {
    return foundBookmark;
  } else {
    for(const item of bookmarks) {
      const foundItem = findBookmark(item.children, bookmark);
      if (foundItem) {
        return foundItem;
      }
    }
  }

  return null;
}

function findBookmarkParent(parentBookmark: BookmarkStateItem, bookmark: BookmarkStateItem): BookmarkStateItem | null {
  const foundBookmark = parentBookmark.children.find((b: BookmarkStateItem) => bookmark === b);
  if (foundBookmark) {
    return parentBookmark;    
  } else {    
    for(const item of parentBookmark.children) {
      const foundItem = findBookmarkParent(item, bookmark);
      if (foundItem) {
        return foundItem;
      }
    }
  }

  return null;
}

function removeBookmark(bookmarks: BookmarkStateItem[], bookmark: BookmarkStateItem): void {
  const bookmarkToRemove = bookmarks.find((b: BookmarkStateItem) => bookmark === b);
  if (bookmarkToRemove) {
    bookmarks.splice(bookmarks.indexOf(bookmarkToRemove), 1);
  } else {
    bookmarks.forEach((item: BookmarkStateItem) => {
      removeBookmark(item.children, bookmark);
    });
  }
}
