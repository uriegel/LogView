<template>
    <div class="main"> 
        <event-log-view :eventBus="logEventBus" :connectionUrl="'ws://localhost:9866/logview'">
        </event-log-view>
    </div>
</template>

<script>

        //"ws://localhost:9866/logview")
        //const ws = new WebSocket("wss://cas-ws200109.caseris.intern/MonitorLog")

import Vue from 'vue'
import EventLogView from '../EventLogView.vue'
import TableView from 'virtual-table-vue'

Vue.use(TableView)

export default Vue.extend({
    components: {
        EventLogView
    },
    data() {
        return {
            logEventBus: new Vue()
        }
    },
})
</script>

<style>
:root {
    --tablevue-main-color: black;
    --tablevue-main-background-color: white;
    --tablevue-right-margin-scrollbar: 16px;
    
    --tablevue-scrollbar-width: 16px;
    --tablevue-scrollbar-border-width: 1px;
    --tablevue-scrollbar-background-color: azure;    
    --tablevue-scrollbar-border-color: gray;
    --tablevue-scrollbar-grip-width: 100%;    
    --tablevue-scrollbar-grip-color: rgb(209, 209, 209);
    --tablevue-scrollbar-grip-hover-color: #bbb;
    --tablevue-scrollbar-grip-active-color: #999;
    --tablevue-scrollbar-button-color: #666;
    --tablevue-scrollbar-button-hover-color: #555;
    --tablevue-scrollbar-button-active-color: #444;
    --tablevue-scrollbar-button-background-color: white;
    --tablevue-scrollbar-button-hover-background-color: rgb(209, 209, 209);
    --tablevue-scrollbar-button-active-background-color: #aaa;
    
    --tablevue-selected-color: white;
    --tablevue-selected-background-color: blue;
    --tablevue-columns-separator-color:  white;
    --tablevue-selected-background-hover-color: #0063ff;
    --tablevue-tr-selected-color: red;
}
body {
    height: 100vh;
    margin: 0px;
    padding: 0px;
    display: flex;
    color: #bbb;
}
.main  {
    display: flex;
    flex-direction: column;
    width: 100vw;
}
</style>