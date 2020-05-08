<template>
    <div class="root">
        <table-view :eventBus="tableEventBus" :columns='columns' :itemsSource='itemsSource'  
            @selection-changed="onSelectionChanged">
            <template v-slot=row >
                <tr :class="{ 'isCurrent': row.item.index == selectedIndex || (selectedIndex == 0 && !row.item.index)}">
                    <td>{{row.item.text}}</td>
                </tr>
            </template>
        </table-view>
    </div>
</template>

<script lang="ts">
import Vue from 'vue'
import {ItemsSource, Column, TableViewItem } from 'virtual-table-vue'
import { loadLogFile, getLines, refresh, scanFile } from '../../connection'

enum InMsgType {
    ChangeTheme = 1,
    ItemsSource,
    Items,
}

interface InMsg {
    method: InMsgType
}

interface ThemeMsg extends InMsg {
  	theme: string
}

interface NewItemsSource extends InMsg {
    count: number,
    indexToSelect: number
}

enum OutMsgType {
    GetItems = "GetItems"
}

interface Range {
    reqId: number
    startRange: number, 
    endRange: number
}

interface OutMsg {
    case: OutMsgType
    fields: Range[]
}

interface Items extends InMsg {
    reqId: number
    path: string,
    isHidden: boolean
    items: any[]
}

var reqId = 0
var refreshMode = false

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
            itemsSource: { 
                count: 0,
                indexToSelect: 0,
                getItems: async (_: number, __: number) => await []
            } as ItemsSource
        }
    },
    computed: {
        totalCount(): number {
            return this.itemsSource.count
        }
    },
    mounted: function () {
        const ws = new WebSocket("ws://localhost:9866/logview")
        let resolves = new Map<number, (items: any[])=>void>()
        ws.onmessage = m => {
            let msg = JSON.parse(m.data) as InMsg
            const getItems = async (startRange: number, endRange: number) => {
                return new Promise<any[]>((res, rej) => {
                    const msg: OutMsg = {
                        case: OutMsgType.GetItems,
                        fields: [{ reqId: ++reqId, startRange, endRange }]
                    }
                    resolves.set(reqId, res)
                    ws.send(JSON.stringify(msg))
                })
            }

            switch (msg.method) {
          	    case InMsgType.ChangeTheme:
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
                case InMsgType.ItemsSource:
                    const itemsSource = msg as NewItemsSource
                    this.itemsSource = { 
                        count: itemsSource.count, 
                        getItems, 
                        indexToSelect: refreshMode ? itemsSource.indexToSelect : -1
                    }
                    break
                case InMsgType.Items:
                    const items = msg as Items
                    let resolve = resolves.get(items.reqId)
                    if (resolve) {
                        resolves.delete(items.reqId)
                        resolve(items.items)
                    }
                    break                    
            }
        }
    },
    methods: {
        onSelectionChanged(index: number) { 
            this.selectedIndex = index 
            refreshMode = this.selectedIndex == this.itemsSource.count - 1
        },
        async fill(evt: Event) {
            const count = await loadLogFile("/home/uwe/server.log")
            //const count = await loadLogFile("/home/uwe/Desktop/LogTest/test.log")
            //const count = await loadLogFile("D:\\Projekte\\LogReader\\LogReader\\server.log")
            //const count = await loadLogFile("C:\\ProgramData\\caesar\\CAEWebSrv\\log\\caesarWebServer.log")
            
            //const count = await loadLogFile("c:\\neuer ordner\\server.log")
            this.itemsSource = { count, indexToSelect: 0, getItems: getLines }
        },
        async refresh(evt: Event) {
            setInterval(async () => {
                const count = await refresh()
                console.log("Refresht", count)
                this.itemsSource = { count, indexToSelect: 0, getItems: getLines }
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
