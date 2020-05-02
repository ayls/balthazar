import _ from 'lodash'

const apiBase = (window as any).config?.apiBase || process.env.apiBase;

export interface BookmarkState {
  apiBase: string,
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
  apiBase: apiBase,
  bookmarks: []
})

export const actions = {
  async load ({ commit }: { commit: any }) {
    const bookmarkRecords = await (this as any).$axios.$get(`${apiBase}/List`);
    commit('load', { bookmarkRecords });
  },
  async endEdit ({ commit }: { commit: any }, { bookmark, confirmed }: { bookmark: BookmarkStateItem, confirmed: boolean }) {
    if (confirmed && !bookmark.id) {
      const bookmarkRecord = await (this as any).$axios.$post(
        `${apiBase}/Add`, 
        {
          parentRowKey: bookmark.record.parentRowKey,
          name: bookmark.record.name,
          url: bookmark.record.url,
          isFolder: bookmark.record.isFolder,
          order: bookmark.record.order
        }
      );
      commit('endAppend', { bookmark, bookmarkRecord });      
    } 
    else if (confirmed && bookmark.id) {
      const bookmarkRecord = await (this as any).$axios.$put(
        `${apiBase}/Update`, 
        bookmark.record
      );      
      commit('endUpdate', { bookmark, bookmarkRecord });            
    }
    else {
      commit('cancelEdit', { bookmark });            
    }
  },
  async setParent ({ commit, state }: { commit: any, state: BookmarkState }, { bookmark, referenceBookmark, type }: { bookmark: BookmarkStateItem, referenceBookmark: BookmarkStateItem, type: string }) {
    commit('setParent', { bookmark, referenceBookmark, type });
    const updatedBookmark = findBookmark(state.bookmarks, bookmark);
    const updatedBookmarkParent = findBookmarkParent({ children: state.bookmarks } as BookmarkStateItem, updatedBookmark!);
    for(const item of updatedBookmarkParent!.children) {
      const bookmarkRecord = await (this as any).$axios.$put(
        `${apiBase}/Update`, 
        item.record
      );       
      commit('endUpdate', { bookmark, bookmarkRecord });            
    }
  },
  async remove ({ commit }: { commit: any }, { bookmark }: { bookmark: BookmarkStateItem }) {
    const recursiveDelete = async (bookmark: BookmarkStateItem) => {
      await (this as any).$axios.$delete(
        `${apiBase}/Delete`,
        {
          data: bookmark.record
        }
      );    
      for(const item of bookmark.children) {
        await recursiveDelete(item);
      }
    }
    await recursiveDelete(bookmark);
    commit('remove', { bookmark });                
  }
}

export const mutations = {
  load(state: BookmarkState, { bookmarkRecords }: { bookmarkRecords: BookmarkRecord[] }) {
    const bookmarks = parseApiResponse(bookmarkRecords, null);
    state.bookmarks.splice(0, state.bookmarks.length);
    for(const bookmark of bookmarks) {
      state.bookmarks.push(bookmark);
    }
  },
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
    const bookmarkChild = {
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
      bookmark.children.push(bookmarkChild);
    } else {
      state.bookmarks.push(bookmarkChild);
    }
  },
  setParent(state: BookmarkState, { bookmark, referenceBookmark, type }: { bookmark: BookmarkStateItem, referenceBookmark: BookmarkStateItem, type: string }) {
    removeBookmark(state.bookmarks, bookmark);
    const bookmarkCopy = _.cloneDeep(bookmark);
    if (type === 'inner') {      
      bookmarkCopy.record.parentRowKey = referenceBookmark.record.rowKey;
      referenceBookmark.children.push(bookmarkCopy);
      for(let i = 0; i < referenceBookmark.children.length; i++) {
        referenceBookmark.children[i].record.order = i;
      }
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
      for(let i = 0; i < referenceBookmarkParent!.children.length; i++) {
        referenceBookmarkParent!.children[i].record.order = i;
      }      
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
  endAppend(state: BookmarkState, { bookmark, bookmarkRecord }: { bookmark: BookmarkStateItem, bookmarkRecord: BookmarkRecord }) {
    bookmark.id = bookmarkRecord.rowKey;
    bookmark.record = bookmarkRecord;
    bookmark.record.eTag = '*';
    bookmark.isEditing = false;
  },
  endUpdate(state: BookmarkState, { bookmark, bookmarkRecord }: { bookmark: BookmarkStateItem, bookmarkRecord: BookmarkRecord }) {
    bookmark.record = bookmarkRecord;
    bookmark.record.eTag = '*';    
    bookmark.isEditing = false;
  },    
  cancelEdit(state: BookmarkState, { bookmark }: { bookmark: BookmarkStateItem }) {
    if (!bookmark.id) {
      removeBookmark(state.bookmarks, bookmark);
    } else {
      bookmark.isEditing = false;
    }
  },
  cancelAllEditing(state: BookmarkState) {
    applyToAllBookmarks({ children: state.bookmarks } as BookmarkStateItem, (b: BookmarkStateItem) => {
      if (!b.id && b.isEditing) {
        removeBookmark(state.bookmarks, b);
      } else {
        b.isEditing = false;
      }
    });    
  },
  remove(state: BookmarkState, { bookmark }: { bookmark: BookmarkStateItem }) {
    const bookmarkParent = findBookmarkParent({ children: state.bookmarks } as BookmarkStateItem, bookmark);
    removeBookmark(bookmarkParent!.children, bookmark);    
  }
}

function parseApiResponse(bookmarkRecords: BookmarkRecord[], parentItem: BookmarkStateItem | null):  BookmarkStateItem[] {
  const result = []
  const parentRowKey = parentItem != null ? parentItem.record.rowKey : '';
  const childRecords =  bookmarkRecords.filter((r: BookmarkRecord) => r.parentRowKey == parentRowKey);
  for(const record of childRecords) {
    const bookmark = {
      id: record.rowKey,
      record: { 
        parentRowKey: parentRowKey,
        order: record.order,
        name: record.name, 
        url: record.url,
        isFolder: record.isFolder,
        partitionKey: record.partitionKey,
        rowKey: record.rowKey,
        timestamp: record.timestamp,
        eTag: '*'
      },
      isEditing: false, 
      children: [] 
    } as BookmarkStateItem;    
    const bookmarkChildren = parseApiResponse(bookmarkRecords, bookmark);
    bookmark.children = bookmarkChildren;
    result.push(bookmark);
  }

  result.sort((a: BookmarkStateItem, b: BookmarkStateItem) => a.record.order - b.record.order);
  return result;
}

function applyToAllBookmarks(bookmark: BookmarkStateItem, action: (b: BookmarkStateItem) => void): void {
  action(bookmark);
  for(const item of bookmark.children) {
    applyToAllBookmarks(item, action);
  }
}

function findBookmark(bookmarks: BookmarkStateItem[], bookmark: BookmarkStateItem): BookmarkStateItem | null {
  const foundBookmark = bookmarks.find((b: BookmarkStateItem) => bookmark.id === b.id);
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
  const foundBookmark = parentBookmark.children.find((b: BookmarkStateItem) => bookmark.id === b.id);
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
  const bookmarkToRemove = bookmarks.find((b: BookmarkStateItem) => bookmark.id === b.id);
  if (bookmarkToRemove) {
    bookmarks.splice(bookmarks.indexOf(bookmarkToRemove), 1);
  } else {
    for(const item of bookmarks) {    
      removeBookmark(item.children, bookmark);
    }
  }
}
