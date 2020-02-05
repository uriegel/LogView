<template>
    <div id="app">
        <!-- <columns-test></columns-test> -->
        <!-- <scrollbar-test></scrollbar-test> -->
        <table-view-test></table-view-test>
    </div>
</template>

<script lang="ts">
import Vue from 'vue'
// Tests
// import ColumnsTest from './components/test/ColumnsTest.vue'
// import ScrollbarTest from './components/test/ScrollbarTest.vue'
import TableViewTest from './components/test/TableViewTest.vue'

export default Vue.extend({
    name: 'app',
    components: {
        // Tests:
        //ColumnsTest,
        // ScrollbarTest,
        TableViewTest,
    }
});
</script>

<style>
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
