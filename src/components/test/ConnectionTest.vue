<template>
    <div class="root">
        <h1>Connection Test</h1>
        <p>
            <button @click=onGetLineCount>GetLineCount</button>
        </p>
        <p>
            <button @click=onGetLines>GetLines</button>
        </p>
    </div>
</template>

<script lang="ts">
import Vue from 'vue'
import { loadLogFile, getLines } from '../../connection'

export default Vue.extend({
    methods: {
        async onGetLineCount(evt: Event) {
            await loadLogFile("/home/uwe/server.log")
        },
        async onGetLines(evt: Event) {
            const lines = await getLines(40, 140)
            console.log(lines)
        },
    },
})
</script>

<style scoped>
</style>
