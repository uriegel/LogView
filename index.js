import EventLogView from "./EventLogView.vue"
//import TableView from 'virtual-table-vue'

export default {
    install: function(Vue, options) {
        // Let's register our component globally
        // https://vuejs.org/v2/guide/components-registration.html
        Vue.component("event-log-view", EventLogView)
    }
}