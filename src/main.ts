import Vue from 'vue'
import App from './App.vue'
import store from './store'
import TableView from 'virtual-table-vue'

Vue.config.productionTip = false

Vue.use(TableView)

new Vue({
    store,
    render: h => h(App)
}).$mount('#app')
