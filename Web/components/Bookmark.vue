<template>
  <div class="bookmark-tree-node">
    <div v-if="isEditing" class="bookmark-edit">  
      <div>
        <el-input placeholder="Name" v-model="name" size="mini"></el-input>      
      </div>
      <div v-if="!isFolder">
        <el-input placeholder="url" v-model="url" size="mini"></el-input>                
      </div>
      <div>
        <el-button
          icon="el-icon-check"
          circle
          size="mini"
          @click="() => endEdit(true)">
        </el-button>
        <el-button
          icon="el-icon-close"
          circle
          size="mini"
          @click="() => endEdit(false)">
        </el-button>                
      </div>         
    </div>
    <div v-else class="bookmark-readonly">
      <div v-if="isFolder">{{ name }}</div>                      
      <div v-else>
        <a :href="url" target="_blank"><span>{{ name }}</span></a>              
      </div>                 
    </div>            
    <div>
      <el-dropdown 
        @command="handleCommand">
        <span class="el-dropdown-link">
          <i class="el-icon-more el-icon--right"></i>
        </span>
        <el-dropdown-menu slot="dropdown">
          <el-dropdown-item v-if="isFolder" command="addFolder">New folder</el-dropdown-item>
          <el-dropdown-item v-if="isFolder" command="addUrl">New url</el-dropdown-item>
          <el-dropdown-item :divided="isFolder" command="edit">Edit</el-dropdown-item>
          <el-dropdown-item class="danger-command" command="delete">Delete</el-dropdown-item>          
        </el-dropdown-menu>
      </el-dropdown>
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
        return this.bookmark.record.name;
      },
      set(text) {
        const bookmark = this.bookmark;
        this.$store.commit('updateName', { bookmark, text });
      }
    },
    url: {
      get() {
        return this.bookmark.record.url;
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
        return this.bookmark.record.isFolder;
      }
    }
  },  
  methods: {
    handleCommand(command: any) {
      switch (command) {
        case 'addFolder':
          this.append(true);
          break;
        case 'addUrl':
          this.append(false);
          break;
        case 'edit':
          this.beginEdit();
          break;
        case 'delete':
          this.remove();
          break;
      }
      console.log(command)
    },
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

div.bookmark-edit > div:nth-last-child(1) {
  width: 62px;
}

div.bookmark-edit > div:nth-last-child(2) {
  flex:1;
  padding-right: 5px; 
}

div.bookmark-edit > div:nth-last-child(3) {
  flex:0.4;
  padding-right: 5px;  
}

.danger-command {
  color: crimson;
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