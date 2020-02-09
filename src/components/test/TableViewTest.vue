<template>
    <div class="root">
        <h1>Table View Test</h1>
        <div class="container">
            <table-view :eventBus="tableEventBus" :columns='columns' :itemsSource='itemsSource' :itemHeight='17'
                @selection-changed="onSelectionChanged">
                <template v-slot=row >
                    <tr :class="{ 'isCurrent': row.item.index == selectedIndex }">
                        <td>{{row.item.text}}</td>
                    </tr>
                </template>
            </table-view>
        </div>
        <div class="input">
            <button @click="fill">Fill</button>
            <button @click="refresh">Refresh</button>
            <div>Zeilen: {{ totalCount }}</div>
        </div>    
    </div>
</template>

<script lang="ts">
import Vue from 'vue'
import TableView, { TableViewItem, ItemsSource } from '../controls/TableView.vue'
import { loadLogFile, getLines, refresh } from '../../connection'

export default Vue.extend({
    components: {
        TableView
    },
    data() {
        return {
            tableEventBus: new Vue(),
            selectedIndex: 0,
            columns: [
                {
                    name: "Name",
                    isSortable: true
                }
            ],
            itemsSource: { count: 0, getItems: async (_: number, __: number) => await []} as ItemsSource
        }
    },
    computed: {
        totalCount(): number {
            return this.itemsSource.count
        }
    },
    methods: {
        onSelectionChanged(index: number) { this.selectedIndex = index },
        async fill(evt: Event) {
            //const count = await loadLogFile("/home/uwe/server.log")
            const count = await loadLogFile("/home/uwe/Desktop/LogTest/test.log")
            //const count = await loadLogFile("D:\\Projekte\\LogReader\\LogReader\\server.log")
            //const count = await loadLogFile("c:\\neuer ordner\\server.log")
            this.itemsSource = { count, getItems: getLines }
        },
        async refresh(evt: Event) {
            const count = await refresh()
            console.log("Refresht", count)
            this.itemsSource = { count, getItems: getLines }
        }
    }
})
</script>

<style scoped>
.root {
    display: flex;
    flex-direction: column;
    flex-grow: 1;
}
.container {
    flex-grow: 1;
    padding: 20px;
    display: flex;
}
.input {
    margin: 20px;
}
</style>
