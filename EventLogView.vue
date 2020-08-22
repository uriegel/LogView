<template>
    <div class="root" @focus=focus @keydown=keydown>
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
                    <td v-if="!restrictions">{{row.item.itemParts[2]}}</td>  
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
        }
    },
    data() {
        return {
            tableEventBus: new Vue(),
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
            //restrictions: ["CAESAR Proxy status", "roxy", "User"]
            //restrictions: ["CAESAR Proxy status"]
            restrictions: []
        }
    },
    mounted() {

        // const f5$ = this.keyDown$.pipe(filter(n => !n.event.which == 116))
        // this.$subscribeTo(f5$, () => console.log("rifreysch"))

        this.eventBus.$on('restrict', restriction => {
            const msg = {
                case: "SetRestriction",
                fields: [{ restriction: restriction || null }]
            }
            ws.send(JSON.stringify(msg))
        })
    },
    methods: {
        runEvents() {
            const webSocket = new WebSocket(this.connectionUrl)
            let resolves = new Map()
            webSocket.onopen = () => {
                ws = webSocket
                this.focus()
            }
            webSocket.onmessage = m => {
                let msg = JSON.parse(m.data) 
                const getItems = async (startRange, endRange) => {
                    return new Promise((res, rej) => {
                        const msg = {
                            case: "GetItems",
                            fields: [{ reqId: ++reqId, startRange, endRange:Math.min(endRange, this.itemsSource.count - 1)}]
                        }
                        resolves.set(reqId, {res, rej})
                        ws.send(JSON.stringify(msg))
                    })
                }

                switch (msg.method) {
                    case 1: {
                        const itemsSource = msg
                        this.itemsSource = { 
                            count: itemsSource.count || 0, 
                            getItems, 
                            indexToSelect: itemsSource.indexToSelect
                        }
                        break
                    }
                    case 2: {
                        const items = msg
                        console.log("items", items, this.itemsSource.count)
                        let resolve = resolves.get(items.reqId)
                        if (resolve) {
                            resolves.delete(items.reqId)
                            // Very important!!!!!!
                            // process only up to 50 request in order to avoid overfloyed queue
                            if (items.reqId < reqId + 50)
                                resolve.res(items.items)
                            else
                                resolve.rej()
                        }
                        break      
                    }              
                }
            }
        },
        onSelectionChanged(index) { 
            this.selectedIndex = index 
        },
        focus() { this.tableEventBus.$emit("focus") },
        keydown(evt) {
            if (evt.which == 116) {
                evt.stopPropagation()
                evt.preventDefault()                
                const msg = {
                    case: "Refresh"
                }
                if (ws)
                    ws.send(JSON.stringify(msg))
            }
        },
        getRestricted(item) {
            function getRestricted(itemToRestrict, res) {
                function* split(str, sep) {
                    const seplen = sep.length
                    let pos = 0
                    while (true) {
                        const index = itemToRestrict.item.indexOf(res, pos)
                        yield itemToRestrict.item.substr(pos, index - pos >= 0 ? index - pos : undefined) 
                        pos = index + seplen
                        if (index == -1)
                            break
                    }
                }
                const parts = Array.from(split(itemToRestrict.item, res))
                return { 
                    item: parts.join(`<span class='event-log-vue-selected${itemToRestrict.index}'>${res}</span>`), 
                    index: itemToRestrict.index <= 2 ? itemToRestrict.index + 1 : 3
                }
            }
            
            const result = this.restrictions.reduce((acc, res) => getRestricted(acc, res), { item, index: 1 })

            return result.item
        }
    },
    beforeDestroy() {
        if (ws)
            ws.close()
    }    
})
</script>
<style>
.event-log-vue-selected1 {
    background-color: yellow;
}
.event-log-vue-selected2 {
    background-color: red;
    color: white;
}
.event-log-vue-selected3 {
    background-color: green;
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
