<template>
  <div class="container">
    <div>
      <div class="titlebar">
        <div>
          <h1 class="title">
            balthazar
          </h1>
          <h2 class="subtitle">
            bookmark manager
          </h2>
        </div>
        <div>
          <el-dropdown 
            @command="handleCommand">
            <span class="el-dropdown-link">
              <i class="el-icon-menu el-icon--right"></i>
            </span>
            <el-dropdown-menu slot="dropdown">
              <el-upload
                :action="importActionUrl"
                :before-upload="showLoading"
                :on-success="handleImportCompleted"
                :on-error="stopLoading"
                class="import-dropdown-button"
                ref="uploadFile">
                <el-dropdown-item>Import</el-dropdown-item>
              </el-upload>
              <el-dropdown-item divided command="addFolder">New folder</el-dropdown-item>                        
              <el-dropdown-item command="addUrl">New url</el-dropdown-item>                        
              <el-dropdown-item divided command="expandAll">Expand all</el-dropdown-item>          
              <el-dropdown-item command="collapseAll">Collapse all</el-dropdown-item>                                      
            </el-dropdown-menu>
          </el-dropdown>
        </div>
      </div>
      <div class="bookmarks">
        <el-input
          class="bookmark-filter"
          placeholder="Search"
          v-model="filterText">
        </el-input>        
        <customElTree
          :data="bookmarks"
          node-key="id"
          :expand-on-click-node="false"
          :filter-node-method="filterNode"
          @node-drop="handleDrop"
          draggable
          :allow-drop="allowDrop"
          :allow-drag="allowDrag"
          ref="tree">
          <div class="bookmark-tree-node-container" slot-scope="{ node, data }">
            <bookmark :node="node" :bookmark="data"></bookmark>
          </div>        
        </customElTree>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from 'vue'
import { BookmarkStateItem } from '~/store/index.ts'
import Bookmark from '~/components/Bookmark.vue'
import CustomElTree from '~/components/CustomElTree.vue'

export default Vue.extend({
  components: {
    Bookmark,
    CustomElTree
  },
  data() {
    return {
      filterText: '',
      bookmarks: this.$store.state.bookmarks,
      isExpandingOrCollapsing: false
    };
  },
  computed: {
    importActionUrl: {
      get() {
        return `${process.env.apiBase}/Import`;
      }
    }
  },
  watch: {
    filterText(val: string) {
      if (!this.isExpandingOrCollapsing) {
        (this.$refs.tree as any).filter(val);
      }
    }
  },  
  mounted() {
    this.showLoadingAndRun(this.$loading, async () => await this.$store.dispatch('load'))    
  },
  methods: {
    showLoadingAndRun: async (loader: any, func: () => Promise<void>) => {
      const loading = loader({
        fullscreen: true,        
        lock: true,
      });
      await func();
      loading.close();
    },
    showLoading() {
      const loading = this.$loading({
        fullscreen: true,
        lock: true,
      });
    }, 
    stopLoading() {
      const loading = this.$loading({
        fullscreen: true,        
        lock: true,
      });
      loading.close();
    },    
    handleCommand(command: string) {
      console.log(command);
      switch (command) {
        case 'addFolder':
          this.append(true);
          break;
        case 'addUrl':
          this.append(false);
          break;
        case 'expandAll':
          this.expandAll();
          break;
        case 'collapseAll':
          this.collapseAll();
          break;          
      }
    },
    handleImportCompleted() {
      this.showLoadingAndRun(this.$loading, async() => {
        await this.$store.dispatch('load');
      });
    },
    append(isFolder: boolean) {
      this.$store.commit('append', { bookmark: null, isFolder });
    },    
    expandAll() {
      this.isExpandingOrCollapsing = true;
      try {
        this.filterText = '';
        this.$store.commit('cancelAllEditing');        
        (this.$refs.tree as any).expand();
      }
      finally {
        this.isExpandingOrCollapsing = false;
      }
    },    
    collapseAll() {
      this.isExpandingOrCollapsing = true;
      try {
        this.filterText = '';
        this.$store.commit('cancelAllEditing');
        (this.$refs.tree as any).collapse();
      }
      finally {
        this.isExpandingOrCollapsing = false;
      }
    },            
    handleDrop(draggingNode: any, dropNode: any, type: string, ev: DragEvent) {
      this.showLoadingAndRun(
        this.$loading,
        async() => await this.$store.dispatch('setParent', { bookmark: draggingNode.data, referenceBookmark: dropNode.data, type })
      );
    },
    allowDrop(draggingNode: any, dropNode: any, type: string) {
      return dropNode.data.record.isFolder || type != 'inner';
    },
    allowDrag(draggingNode: any) {
      return !draggingNode.data.record.isEditing;
    },    
    filterNode(value: string, data: BookmarkStateItem) {
      if (!value) return true;
      const lowerCasedValue = value.toLowerCase();
      return (data.record.name.toLowerCase().indexOf(lowerCasedValue) !== -1) || (data.record.url.toLowerCase().indexOf(lowerCasedValue) !== -1);
    }
  }
})
</script>

<style>
.container {
  margin: 20px 20px;
}

.titlebar {
  display: flex;
  width: 100%;
}

.titlebar > div:first-child {
  flex: 1;
}

.titlebar > div:last-child {
  margin-right: 5px;
}

.titlebar > div {
  display: inline-block;
}

.title {
  font-family: 'Quicksand', 'Source Sans Pro', -apple-system, BlinkMacSystemFont,
    'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  display: block;
  font-weight: 300;
  font-size: 30px;
  color: #35495e;
  letter-spacing: 1px;
}

.subtitle {
  font-weight: 300;
  font-size: 12px;
  color: #526488;
  word-spacing: 5px;
  padding-bottom: 15px;
}

.import-dropdown-button,
.import-dropdown-button > div {
  width: 100%;
}

.import-dropdown-button > div > li {
  text-align: left;
}

.bookmark-filter {
  padding-bottom: 15px;
}

.bookmarks {
  padding-top: 5px;
}

.bookmarks .el-tree-node__content {
  height: 34px;
}

.bookmark-tree-node-container {
  width: 100%;
  padding-right: 8px;
}
</style>
