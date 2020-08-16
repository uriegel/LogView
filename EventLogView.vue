<template>
    <div class="root">
        <table-view :eventBus="tableEventBus" :columns='columns' :itemsSource='itemsSource'  
            @selection-changed="onSelectionChanged">
            <template v-slot=row >
                <tr :class="{ 'isCurrent': row.item.index == selectedIndex || (selectedIndex == 0 && !row.item.index)}">
                    <td class="icon-name">
                        <trace-icon v-if="row.item.msgType == 1" class="svg icon"></trace-icon> 
                        <info-icon v-if="row.item.msgType == 2" class="svg icon"></info-icon> 
                        <warning-icon v-if="row.item.msgType == 3" class="svg icon"></warning-icon> 
                        <error-icon v-if="row.item.msgType == 4" class="svg icon"></error-icon>
                        <stop-icon v-if="row.item.msgType == 5" class="svg icon"></stop-icon>
                        <new-line-icon v-if="row.item.msgType == 6" class="svg icon"></new-line-icon> 
                        {{row.item.text}}
                    </td>
                </tr>
            </template>
        </table-view>
    </div>
</template>

<script>
import Vue from 'vue'
import NewLineIcon from './icons/NewLineIcon.vue'
import TraceIcon from './icons/TraceIcon.vue'
import InfoIcon from './icons/InfoIcon.vue'
import WarningIcon from './icons/WarningIcon.vue'
import ErrorIcon from './icons/ErrorIcon.vue'
import StopIcon from './icons/StopIcon.vue'

var reqId = 0
var refreshMode = false

export default Vue.extend({
    components: {
        NewLineIcon,
        TraceIcon,
        InfoIcon,
        WarningIcon,
        ErrorIcon,
        StopIcon
    },
    props: {
        connectionUrl: String
    },
    watch: {
        connectionUrl: {
            immediate: true,
            handler(newVal) {
                if (newVal)
                    this.runEvents()
            }
        }
    },
    data() {
        return {
            tableEventBus: new Vue(),
            selectedIndex: 0,
            columns: [
                {
                    name: "Ereignisse",
                    isSortable: false
                }
            ],
            itemsSource: { 
                count: 0,
                indexToSelect: 0,
                getItems: async () => await []
            } 
        }
    },
    methods: {
        runEvents() {
            const ws = new WebSocket(this.connectionUrl)
            let resolves = new Map()
            ws.onmessage = m => {
                let msg = JSON.parse(m.data) 
                const getItems = async (startRange, endRange) => {
                    return new Promise((res) => {
                        const msg = {
                            case: "GetItems",
                            fields: [{ reqId: ++reqId, startRange, endRange }]
                        }
                        resolves.set(reqId, res)
                        ws.send(JSON.stringify(msg))
                    })
                }

                switch (msg.method) {
                    case 1: {
                        const itemsSource = msg
                        this.itemsSource = { 
                            count: itemsSource.count || 0, 
                            getItems, 
                            indexToSelect: refreshMode ? itemsSource.indexToSelect : -1
                        }
                        break
                    }
                    case 2: {
                        const items = msg
                        console.log("items", items)
                        let resolve = resolves.get(items.reqId)
                        if (resolve) {
                            resolves.delete(items.reqId)
                            resolve(items.items)
                        }
                        break      
                    }              
                }
            }
        },
        onSelectionChanged(index) { 
            this.selectedIndex = index 
            refreshMode = this.selectedIndex == this.itemsSource.count - 1
        },
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
.icon-name {
    display: block;
}
.svg {
    width: 16px;
    height: 16px;
    vertical-align: bottom;
}
.icon {
    margin-right: 3px;
    vertical-align: bottom;
    height: 16px;
}
</style>