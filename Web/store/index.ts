export const state = () => ({
  bookmarks: [
    {
      id: "1",
      label: 'Level one 1',
      isFolder: true,
      isEditing: false,
      children: [{
        id: "1-1",
        label: 'Level two 1-1',
        url: null,
        isFolder: true,
        isEditing: false,
        children: [{
          id: "1-1-1",
          label: 'Level three 1-1-1',
          url: "https://www.ayls.org",
          isFolder: false,
          isEditing: false,
          children: []
        }]
      }]
    },
    {
      id: "2",
      label: 'Level one 2',
      url: null,
      isFolder: true,
      isEditing: false,
      children: [{
        id: "2-1",
        label: 'Level two 2-1',
        url: null,
        isFolder: true,
        isEditing: false,
        children: [{
          id: "2-1-1",
          label: 'Level three 2-1-1',
          url: "https://www.ayls.org",
          isFolder: false,
          isEditing: false,
          children: []
        }]
      },
      {
        id: "2-2",
        label: 'Level two 2-2',
        url: null,
        isFolder: true,
        children: [{
          id: "2-2-1",
          label: 'Level three 2-2-1',
          url: "https://www.ayls.org",
          isFolder: false,
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
    bookmark.label = text;
  },  
  updateUrl(state: any, { bookmark, text }: any) {
    bookmark.url = text;
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