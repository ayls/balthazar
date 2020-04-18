export const state = () => ({
  bookmarks: [
    {
      id: "51613143",
      record: {
        parentRowKey: "20445251",
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
        parentRowKey: "1451457",
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
  append(state: any, { bookmark, isFolder }: any) {
    applyToAllBookmarks({ children: state.bookmarks }, (b: any) => {
      if (!b.id) {
        removeBookmark(state.bookmarks, b);
      } else {
        b.isEditing = false;
      }
    });    
    const newChild = { label: '', url: '', isEditing: true, isFolder: isFolder, children: [] };
    bookmark.children.push(newChild);
  },
  setParent(state: any, { bookmark, referenceBookmark, dropType }: any) {
    removeBookmark(state.bookmarks, bookmark);
    if (dropType === 'inner') {
      findBookmark(state.bookmarks, referenceBookmark).children.push(bookmark);
    } else {
      const root = { children: state.bookmarks };
      const referenceBookmarkParent = findBookmarkParent(root, referenceBookmark);
      const referenceBookmarkIndex = referenceBookmarkParent.children.indexOf(findBookmark(referenceBookmarkParent.children, referenceBookmark));
      let insertIndex: number;
      switch (dropType) {
        case 'before':
          insertIndex = Math.max(0, referenceBookmarkIndex - 1);
          break;
        case 'after':
          insertIndex = Math.max(referenceBookmarkParent.children.length - 1, referenceBookmarkIndex + 1);        
          break;     
        default:
          insertIndex = 0;
      }    
      referenceBookmarkParent.children.splice(insertIndex, 0, bookmark);
    } 
  },  
  beginEdit(state: any, { bookmark }: any) {
    applyToAllBookmarks({ children: state.bookmarks }, (b: any) => {
      if (!b.id) {
        removeBookmark(state.bookmarks, b);
      } else if (b.id !== bookmark.id) {
        b.isEditing = false;
      }
    });
    bookmark.isEditing = true;
  },
  updateName(state: any, { bookmark, text }: any) {
    bookmark.record.name = text;
  },  
  updateUrl(state: any, { bookmark, text }: any) {
    bookmark.record.url = text;
  },    
  endEdit(state: any, { bookmark, confirmed }: any) {
    if (!confirmed && !bookmark.id) {
      removeBookmark(state.bookmarks, bookmark);
    } else {
      bookmark.isEditing = false;
    }
  },
  remove(state: any, { bookmark }: any) {
    const bookmarkParent = findBookmarkParent({ children: state.bookmarks }, bookmark);
    removeBookmark(bookmarkParent.children, bookmark);    
  }
}

function applyToAllBookmarks(bookmark: any, action: (b: any) => void): any {
  action(bookmark);
  for(const item of bookmark.children) {
    applyToAllBookmarks(item, action);
  }
}

function findBookmark(bookmarks: any, bookmark: any): any {
  const foundBookmark = bookmarks.find((b: any) => bookmark.id === b.id);
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
}

function findBookmarkParent(parentBookmark: any, bookmark: any): any {
  const foundBookmark = parentBookmark.children.find((b: any) => bookmark.id === b.id);
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
}

function removeBookmark(bookmarks: any, bookmark: any): any {
  const bookmarkToRemove = bookmarks.find((b: any) => bookmark.id === b.id);
  if (bookmarkToRemove) {
    bookmarks.splice(bookmarks.indexOf(bookmarkToRemove), 1);
  } else {
    bookmarks.forEach((item: any) => {
      removeBookmark(item.children, bookmark);
    });
  }
}
