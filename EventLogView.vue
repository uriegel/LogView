<template>
    <div class="root" @focus=focus @keydown=keydown>
        <table-view :eventBus="tableEventBus" :columns='columns' :itemsSource='itemsSource'  
            @selection-changed="onSelectionChanged" @column-click='onColumn'>

            <template v-slot:col0>
                <div class="icontext">
                    <info-icon v-if="logType == 1" class="svg icon"></info-icon> 
                    <warning-icon v-if="logType == 2" class="svg icon"></warning-icon> 
                    <error-icon v-if="logType == 3" class="svg icon"></error-icon>
                    <stop-icon v-if="logType == 4" class="svg icon"></stop-icon>
                    <span class="col">Zeit</span>
                </div>
            </template>
            <template v-slot:col1>
                <div class="col">Kategorie</div>
            </template>
            <template v-slot:col2>
                <div class="col">Ereignis</div>
            </template>

            <template v-slot=row >
                <tr :class="{ 'isCurrent': row.item.index == selectedIndex || (selectedIndex == 0 && !row.item.index)}">
                    <td class="selectable icon-name">
                        <trace-icon v-if="row.item.msgType == 1" class="svg icon"></trace-icon> 
                        <info-icon v-if="row.item.msgType == 2" class="svg icon"></info-icon> 
                        <warning-icon v-if="row.item.msgType == 3" class="svg icon"></warning-icon> 
                        <error-icon v-if="row.item.msgType == 4" class="svg icon"></error-icon>
                        <stop-icon v-if="row.item.msgType == 5" class="svg icon"></stop-icon>
                        <new-line-icon v-if="row.item.msgType == 6" class="svg icon"></new-line-icon> 
                        {{row.item.itemParts[0]}}
                    </td>
                    <td class="selectable" v-if="restrictions" v-html="getRestricted(row.item.itemParts[1])"></td>
                    <td class="selectable" v-if="!restrictions">{{row.item.itemParts[1]}}</td>  
                    <td class="selectable" v-bind:title="row.item.itemParts[2]" v-if="restrictions" v-html="getRestricted(row.item.itemParts[2])"></td>
                    <td class="selectable" v-bind:title="row.item.itemParts[2]" v-if="!restrictions">{{row.item.itemParts[2]}}</td>  
                </tr>
            </template>
        </table-view>
        <input type="input" v-model="restriction" @keydown='onInputKeyDown'>
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
            selectedIndex: 0,
            columns: [
                {
                    width: "5em",
                    isClickable: true,
                },
                {
                    width: "3em",
                    isClickable: true
                },
                {},
            ],
            itemsSource: { 
                count: 0,
                indexToSelect: 0,
                getItems: async () => await []
            },
            restriction: "",
            restrictions: [],
            restricted: false,
            selectedLineIndex: -1,
            refreshMode: false,
            logType: 0
        }
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
        onSelectionChanged(index, item) { 
            this.selectedIndex = index 
            this.selectedLineIndex = item ? item.lineIndex : -1
            this.refreshMode = this.selectedIndex == this.itemsSource.count - 1
        },
        focus() { this.tableEventBus.$emit("focus") },
        keydown(evt) {
            switch (evt.which) {
                case 120: {       
                    this.restricted = !this.restricted            
                    const msg = {
                        case: "SetRestriction",
                        fields: [{ 
                            restriction: this.restricted ? this.restriction : null, 
                            selectedIndex: this.restricted ? this.selectedIndex : this.selectedLineIndex
                        }]
                    }
                    ws.send(JSON.stringify(msg))                    
                    break
                }
            }
        },
        onInputKeyDown(evt) {
            switch (evt.which) {
                case 9: // TAB
                    this.focus()
                    evt.stopPropagation()
                    evt.preventDefault()
                    break
                case 13: { // enter
                    this.restricted = true
                    const msg = {
                        case: "SetRestriction",
                        fields: [{ restriction: this.restriction || null }]
                    }
                    ws.send(JSON.stringify(msg))

                    this.restrictions = 
                        this.restriction
                        ? this.restriction.split(" OR ").map(n => n.split(" && ")).flat()
                        : []

                    this.focus()
                    break
                }
                default:
                    return // exit this handler for other keys
            }
            evt.preventDefault() // prevent
        },      
        onColumn(index) {
            if (index == 0) {
                this.logType++
                if (this.logType == 5)
                    this.logType = 0

                const msg = {
                    case: "SetMinimalType",
                    fields: [{ minimalType: this.logType }]
                }
                ws.send(JSON.stringify(msg))
            }
        },  
        getRestricted(item) {
            function getRestricted(itemToRestrict, res) {
                const seplen = res.length

                // get next not highlighted part, and highlighted part
                function* getParts() {
                    let index = 0
                    while (true) {
                        const pos = itemToRestrict.item.toLowerCase().indexOf(res.toLowerCase(), index)
                        yield pos != -1
                            ? { 
                                item: itemToRestrict.item.substr(index, pos - index), 
                                sep: itemToRestrict.item.substr(pos - index, seplen) 
                            }
                            : { 
                                item: itemToRestrict.item.substr(index), 
                                sep: "" 
                            }
                        if (pos == -1)
                            break                            
                        index = pos + seplen
                    }
                }

                function getHighlighted() {
                    let result = ""
                    const parts = Array.from(getParts())
                    parts.forEach(n => {
                        result += n.item
                        if (n.sep.length > 0)
                            result += `<span class='event-log-vue-selected${itemToRestrict.index}'>${n.sep}</span>`
                    })
                    return result;
                }

                return { 
                    item: getHighlighted(), 
                    index: itemToRestrict.index <= 3 ? itemToRestrict.index + 1 : 4
                }
            }
            
            const result = this.restrictions.reduce((acc, res) => getRestricted(acc, res), { item, index: 1 })

            return result.item
        },
        setRefresh(value) {
            const msg = {
                case: "SetRefreshMode",
                fields: [{ value }]
            }
            ws.send(JSON.stringify(msg))
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
.event-log-vue-selected4 {
    background-color: blue;
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
    display: inline;
    margin-right: 3px;
    vertical-align: bottom;
}
.col {
    padding-left: 5px;
    overflow: hidden;
    text-overflow: ellipsis;
}
.icontext {
    display: flex;
}
.selectable {
    user-select: text;
}

</style>
