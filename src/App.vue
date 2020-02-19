<template>
    <div id="app">
        <log-view></log-view>
    </div>
</template>

<script lang="ts">
import Vue from 'vue'
import LogView from './components/controls/LogView.vue'

export default Vue.extend({
    name: 'app',
    components: {
         LogView,
    }
});
</script>

<style>
:root {
    --tablevue-main-color: black;
    --tablevue-main-background-color: white;
    --tablevue-font-size: 75%;
    --tablevue-selected-background-color: blue;
    --tablevue-selected-background-hover-color: #0063ff;
    --tablevue-selected-color: white;
    --tablevue-columns-separator-color: white;
    --tablevue-scrollbar-top: 16px;
    --tablevue-scrollbar-border-color: var(--vue-electron-titlebar-button-hover-color);
    --tablevue-scrollbar-grip-color: rgb(209, 209, 209);
    --tablevue-scrollbar-button-active-color: #aaa;
    --tablevue-scrollbar-image-color: #666;
    --tablevue-scrollbar-border-active-color: #444;
    --tablevue-scrollbar-grip-color: rgb(209, 209, 209);
    --tablevue-scrollbar-grip-hover-color: #bbb;
    --tablevue-scrollbar-grip-active-color: #999;
    --tablevue-tr-selected-color: red;
    --tablevue-grip-color: var(--vue-electron-titlebar-background-color);
    --tablevue-icon-color: black;
    --tablevue-icon-selected-color: white;
} 
body {
    background-color: var(--tablevue-main-background-color);
    color: var(--tablevue-main-color);
    font-family: sans-serif;
    font-size: var(--tablevue-font-size);
    overflow:hidden;
    height: 100vh;
    padding: 0px;
    margin: 0px;
}
#app {
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
    height: 100vh;
    display: flex;
    flex-direction: column;
    /* border-top: var(--vue-electron-titlebar-top-height) solid transparent; */
}
.dialogOpen {
    filter: blur(1px);
}
</style>
