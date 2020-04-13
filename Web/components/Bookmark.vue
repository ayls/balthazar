<template>
  <div class="bookmark-tree-node">
    <div v-if="isEditing" class="bookmark-edit">
      <div>
        <el-button
          type="primary"
          icon="el-icon-check"
          circle
          size="mini"
          @click="() => endEdit(true)">
        </el-button>
        <el-button
          type="danger"
          icon="el-icon-close"
          circle
          size="mini"
          @click="() => endEdit(false)">
        </el-button>                
      </div>      
      <div>
        <el-input placeholder="Name" v-model="name" size="mini"></el-input>      
      </div>
      <div v-if="!bookmark.isFolder">
        <el-input placeholder="url" v-model="url" size="mini"></el-input>                
      </div>
    </div>
    <div v-else class="bookmark-readonly">
      <div v-if="isFolder">{{ name }}</div>                      
      <div v-else>
        <a v-bind:href="url" target="_blank"><span>{{ name }}</span></a>              
      </div>                 
    </div>            
    <div>
      <div v-if="isFolder">
        <el-button
          icon="el-icon-folder-add"
          circle
          size="mini"
          @click="() => append(true)">
        </el-button>              
        <el-button
          icon="el-icon-plus"
          circle
          size="mini"
          @click="() => append(false)">
        </el-button>
      </div>
      <div v-else class="bookmark-append-placeholder"></div>
      <div>
        <el-button
          icon="el-icon-edit"
          circle
          size="mini"
          @click="() => beginEdit()">
        </el-button>              
        <el-button
          type="danger"
          icon="el-icon-delete"
          circle
          size="mini"
          @click="() => remove()">
        </el-button>
      </div>
    </div>
  </div>  
</template>

<script lang="ts">
import Vue from 'vue'

export default Vue.component('bookmark', {
  props: ['node', 'bookmark'],
  computed: {
    name: {
      get() {
        return this.bookmark.label;
      },
      set(text) {
        const bookmark = this.bookmark;
        this.$store.commit('updateName', { bookmark, text });
      }
    },
    url: {
      get() {
        return this.bookmark.url;
      },
      set(text) {
        const bookmark = this.bookmark;
        this.$store.commit('updateUrl', { bookmark, text });
      }
    },
    isEditing: {
      get() {
        return this.bookmark.isEditing;
      }
    },
    isFolder: {
      get() {
        return this.bookmark.isFolder;
      }
    }
  },  
  methods: {
    append(isFolder: boolean) {
      const bookmark = this.bookmark;
      this.$store.commit('append', { bookmark, isFolder });
      this.node.expand();
    },
    beginEdit() {
      const bookmark = this.bookmark;
      this.$store.commit('beginEdit', { bookmark });
    },
    endEdit(confirmed: boolean) {
      const bookmark = this.bookmark;
      this.$store.commit('endEdit', { bookmark, confirmed });
    },
    remove() {
      const bookmark = this.bookmark;
      this.$store.commit('remove', { bookmark });
    }    
  }
});
</script>

<style>
.bookmark-tree-node div {
  display: inline-block;
}

.bookmark-readonly, .bookmark-edit {
  flex-grow: 1;
}

div.bookmark-edit {
  display: flex;
}

div.bookmark-edit > div:first-child {
  width: 62px;
}

div.bookmark-edit > div:nth-child(2) {
  flex:0.4;
  padding-left: 5px;
  padding-right: 5px; 
}

div.bookmark-edit > div:last-child {
  flex:1;
  padding-right: 5px;  
}

.bookmark-append-placeholder {
  width: 62px;
}

.bookmark-tree-node .el-button+.el-button  {
  margin-left: 0;
}

.bookmark-tree-node .el-input--mini .el-input__inner {
  height: 30px;
  line-height: 30px;
}

.bookmark-tree-node {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 20px;
  font-weight: 100;
}
</style>