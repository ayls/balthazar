<template>
  <div class="container">
    <div>
      <h1 class="title">
        balthazar
      </h1>
      <h2 class="subtitle">
        bookmark manager
      </h2>
      <div class="bookmarks">
        <el-input
          class="bookmark-filter"
          placeholder="Search"
          v-model="filterText">
        </el-input>        
        <el-tree
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
        </el-tree>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from 'vue'
import Bookmark from '~/components/Bookmark.vue'
import _ from 'lodash'

export default Vue.extend({
  components: {
    Bookmark
  },
  data() {
    return {
      filterText: '',
      bookmarks: _.cloneDeep(this.$store.state.bookmarks) // cloning is necessary due to Tree component changing the data on drag and drop, breaking VueX store constraints
    };
  },
  watch: {
    filterText(val) {
      (this.$refs.tree as any).filter(val);
    }
  },  
  methods: {
    handleDrop(draggingNode: any, dropNode: any, dropType: any, ev: any) {
      this.$store.commit('setParent', { bookmark: draggingNode.data, referenceBookmark: dropNode.data, dropType });
    },
    allowDrop(draggingNode: any, dropNode: any, type: any) {
      return dropNode.data.isFolder;
    },
    filterNode(value: any, data: any) {
      if (!value) return true;
      const lowerCasedValue = value.toLowerCase();
      return (data.label && data.label.toLowerCase().indexOf(lowerCasedValue) !== -1) || (data.url && data.url.toLowerCase().indexOf(lowerCasedValue) !== -1);
    }
  }
})
</script>

<style>
.container {
  margin: 20px 20px;
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