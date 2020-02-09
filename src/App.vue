<template>
    <div id="app">
        <!-- <columns-test></columns-test> -->
        <!-- <scrollbar-test></scrollbar-test> -->
        <table-view-test></table-view-test>
        <!-- <connection-test></connection-test> -->
    </div>
</template>

<script lang="ts">
import Vue from 'vue'
// Tests
// import ColumnsTest from './components/test/ColumnsTest.vue'
// import ScrollbarTest from './components/test/ScrollbarTest.vue'
import TableViewTest from './components/test/TableViewTest.vue'
// import ConnectionTest from './components/test/ConnectionTest.vue'

export default Vue.extend({
    name: 'app',
    components: {
        // Tests:
        // ColumnsTest,
        // ScrollbarTest,
         TableViewTest,
        //ConnectionTest,
    }
});
</script>

<style>
:root {
    --main-color: black;
    --main-background-color: white;
    --font-size: 75%;
    --selected-background-color: blue;
    --selected-background-hover-color: #0063ff;
    --selected-color: white;
    --columns-separator-color: white;
    --scrollbar-top: 16px;
    --scrollbar-border-color: var(--vue-electron-titlebar-button-hover-color);
    --scrollbar-grip-color: rgb(209, 209, 209);
    --scrollbar-button-active-color: #aaa;
    --scrollbar-image-color: #666;
    --scrollbar-border-active-color: #444;
    --scrollbar-grip-color: rgb(209, 209, 209);
    --scrollbar-grip-hover-color: #bbb;
    --scrollbar-grip-active-color: #999;
    --status-height: 14px;
    --tr-selected-color: red;
    --grip-color: var(--vue-electron-titlebar-background-color);
    --icon-color: black;
    --icon-selected-color: white;
    --dialog-background-color: white;
    --dialog-fader-color: rgba(0, 0, 0, 0.50);
    --dialog-input-border-color: #grey;
    --dialog-input-outline-color: blue;
    --drag-highlight: aqua;
} 
body {
    background-color: var(--main-background-color);
    color: var(--main-color);
    font-family: sans-serif;
    font-size: var(--font-size);
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
