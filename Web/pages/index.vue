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
              <el-dropdown-item command="import">Import</el-dropdown-item>
              <el-dropdown-item command="addFolder">New folder</el-dropdown-item>          
              <el-dropdown-item command="addUrl">New url</el-dropdown-item>                        
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
      bookmarks: this.$store.state.bookmarks
    };
  },
  watch: {
    filterText(val: string) {
      (this.$refs.tree as any).filter(val);
    }
  },  
  methods: {
    handleCommand(command: string) {
      switch (command) {
        case 'import':
          break;        
        case 'addFolder':
          this.append(true);
          break;
        case 'addUrl':
          this.append(false);
          break;
      }
    },
    append(isFolder: boolean) {
      this.$store.commit('append', { bookmark: null, isFolder });
    },    
    handleDrop(draggingNode: any, dropNode: any, type: string, ev: DragEvent) {
      this.$store.commit('setParent', { bookmark: draggingNode.data, referenceBookmark: dropNode.data, type });
    },
    allowDrop(draggingNode: any, dropNode: any, type: string) {
      return dropNode.data.record.isFolder;
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
