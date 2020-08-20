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
                        {{row.item.itemParts[0]}}
                    </td>
                    <td>{{row.item.itemParts[1]}}</td>
                    <td v-if="restrictions" v-html="getRestricted(row.item.itemParts[2])"></td>
                    <td v-if="!restrictions">>{{row.item.itemParts[2]}}</td>
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
var ws

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
        connectionUrl: String,
        eventBus: { type: Object, default: () => new Vue() },
    },
    watch: {
        connectionUrl: {
            immediate: true,
            handler(newVal) {
                if (newVal)
                    this.runEvents()
            }
        },
        refreshMode: {
            immediate: true,
            handler(newVal) {
                this.setRefresh(newVal)
            }
        }
    },
    data() {
        return {
            tableEventBus: new Vue(),
            refreshMode: false,
            selectedIndex: 0,
            columns: [
                {
                    name: "Zeit",
                    width: "5em"
                },
                {
                    name: "Kategorie",
                    width: "3em"
                },
                {
                    name: "Ereignis",
                },
            ],
            itemsSource: { 
                count: 0,
                indexToSelect: 0,
                getItems: async () => await []
            },
            restrictions: ["Cache"]
        }
    },
    methods: {
        runEvents() {
            ws = new WebSocket(this.connectionUrl)
            let resolves = new Map()
            ws.onmessage = m => {
                let msg = JSON.parse(m.data) 
                const getItems = async (startRange, endRange) => {
                    return new Promise((res) => {
                        const msg = {
                            case: "GetItems",
                            fields: [{ reqId: ++reqId, startRange, endRange:Math.min(endRange, this.itemsSource.count - 1)}]
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
                            indexToSelect: this.refreshMode ? itemsSource.indexToSelect : -1
                        }
                        break
                    }
                    case 2: {
                        const items = msg
                        console.log("items", items, this.itemsSource.count)
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
            this.refreshMode = this.selectedIndex == this.itemsSource.count - 1
        },
        setRefresh(value) {
            const msg = {
                case: "SetRefreshMode",
                fields: [{ value }]
            }
            ws.send(JSON.stringify(msg))
        },
        getRestricted(item) {
            const res = this.restrictions[0]
            let index = item.indexOf(res)
            if (index != -1) {
                let item1 = item.substr(0, index)
                let item2 = item.substr(index, res.length)
                let item3 = item.substr(index + res.length)
                const text =  item1 + "<span class='rot'>" + item2 +"</span>" + item3
                console.log("eitem", item)
                console.log(index, res, item1, item2, item3)
                console.log("text", text)
                return text
            } else
                return item
        }
    },
    mounted() {
        this.eventBus.$on('restrict', restriction => {
            const msg = {
                case: "SetRestriction",
                fields: [{ restriction: restriction || null }]
            }
            ws.send(JSON.stringify(msg))
        })
    },
    beforeDestroy() {
        if (ws)
            ws.close()
    }    
})
</script>
<style>
.rot {
    background-color: darkred;
    color: white;
}
</style> 
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
