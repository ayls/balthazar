<template>
  <div class="centered">
    <el-button round v-on:click="userLogin()">Login</el-button>
  </div>
</template>

<script lang="ts">
import Vue from 'vue'

export default Vue.extend({
  methods: {
    async userLogin() {
      try {
        let response = await this.$auth.loginWith('oAuth2');
      } catch (err) {
        console.log(err)
      }
    }
  }
})
</script>

<style>
div.centered {
    position: fixed;
    top: 50%;
    left: 50%;
    -webkit-transform: translate(-50%, -50%);
    transform: translate(-50%, -50%);
}
</style>
