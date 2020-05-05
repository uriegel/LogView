<template>
    <div class="root">
        <div class="container">
            <table-view :eventBus="tableEventBus" :columns='columns' :itemsSource='itemsSource'  
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
            <button @click="scanFile">Scan</button>
            <div>Zeilen: {{ totalCount }}</div>
        </div>    
    </div>
</template>

<script lang="ts">
import Vue from 'vue'
import {ItemsSource, Column, TableViewItem } from 'virtual-table-vue'
import { loadLogFile, getLines, refresh, scanFile } from '../../connection'

interface Message {
  	method: string
}

interface ThemeMsg extends Message {
  	theme: string
}

export default Vue.extend({
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
    mounted: function () {
        const ws = new WebSocket("ws://localhost:9866/logview")
        ws.onmessage = m => {
            let msg = JSON.parse(m.data) as Message
            switch (msg.method) {
          	    case "changeTheme":
				    const themeMsg = msg as ThemeMsg
				    const styleSheet = document.getElementById("theme")  
				    if (styleSheet)
					    styleSheet.remove()

	    			const head = document.getElementsByTagName('head')[0]
				    let link = document.createElement('link')
				    link.rel = 'stylesheet'
				    link.id = 'theme'
				    link.type = 'text/css'
				    link.href = `http://localhost:9866/assets/themes/${themeMsg.theme}.css`
				    link.media = 'all'
				    head.appendChild(link)
                    break    
            }
        }
    },
    methods: {
        onSelectionChanged(index: number) { this.selectedIndex = index },
        async fill(evt: Event) {
            const count = await loadLogFile("/home/uwe/server.log")
            //const count = await loadLogFile("/home/uwe/Desktop/LogTest/test.log")
            //const count = await loadLogFile("D:\\Projekte\\LogReader\\LogReader\\server.log")
            //const count = await loadLogFile("C:\\ProgramData\\caesar\\CAEWebSrv\\log\\caesarWebServer.log")
            
            //const count = await loadLogFile("c:\\neuer ordner\\server.log")
            this.itemsSource = { count, getItems: getLines }
        },
        async refresh(evt: Event) {
            setInterval(async () => {
                const count = await refresh()
                console.log("Refresht", count)
                this.itemsSource = { count, getItems: getLines }
            }, 200)
        },
        async scanFile(evt: Event) {
            await scanFile()
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
